using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SceneAware
{
    public class HandHeatMap : MonoBehaviour
    {
        // 调用GazeHeatMap中的场景体素信息
        private GazeHeatMap ghm;

        // 最近n帧手柄访问的体素列表
        public Queue<GameObject> latestHandVoxelQueue;
        public int handVoxelCount;

        // 最近n帧手柄运动标准差序列
        public int windowSize; // 窗口（队列）大小
        public float stableThreshold; // 稳定阈值

        private Queue<float> dataQueue = new Queue<float>(); // 滑动窗口，保存当前标准差
        private int numObservations = 0; // 观测值数量
        private int numFrames = 0;
        [HideInInspector]public bool isStationary = false; // 是否趋于平稳
        public int minFramesNeed = 100;

        // 采样
        public bool isSampleData = false;
        private SMSampler smSamplerScript; // 采样脚本


        // Start is called before the first frame update
        void Start()
        {
            ghm = GetComponent<GazeHeatMap>();

            latestHandVoxelQueue = new Queue<GameObject>();

            // 采样
            if (isSampleData) { smSamplerScript = GetComponent<SMSampler>(); }
            

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 由manipulateController调用，传入手柄当前帧坐标
        /// </summary>
        /// <param name="handPosition"></param>
        /// <returns></returns>
        public ReturnStatus UpdateHandHeatMap(Vector3 handPosition)
        {
            int xVoxelIndex = Mathf.FloorToInt((handPosition.x - ghm.roomOrigin.x) / ghm.voxelSideLength - 0.5f) + 1;
            int yVoxelIndex = Mathf.FloorToInt((handPosition.y - ghm.roomOrigin.y) / ghm.voxelSideLength - 0.5f) + 1;
            int zVoxelIndex = Mathf.FloorToInt((handPosition.z - ghm.roomOrigin.z) / ghm.voxelSideLength - 0.5f) + 1;

            // 判断手柄位置越界
            if (xVoxelIndex >= ghm.voxelNumberX || xVoxelIndex < 0 || 
                yVoxelIndex >= ghm.voxelNumberY || yVoxelIndex < 0 || 
                zVoxelIndex >= ghm.voxelNumberZ || zVoxelIndex < 0)
            {
                return ReturnStatus.WAR_GAZE_CROSS_BORDER;
            }

            GameObject curVoxelObject = ghm.voxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];

            if (curVoxelObject == null)
            {
                curVoxelObject = Instantiate(ghm.voxelPrefab, ghm.voxelParentObject.transform);
                curVoxelObject.name = string.Format("voxel_{0}{1}{2}", xVoxelIndex, yVoxelIndex, zVoxelIndex);
                curVoxelObject.SetActive(false);
                curVoxelObject.isStatic = true;
                curVoxelObject.transform.localScale = new Vector3(ghm.voxelSideLength, ghm.voxelSideLength, ghm.voxelSideLength);
                curVoxelObject.transform.position = ghm.roomOrigin + 
                    xVoxelIndex * Vector3.right * ghm.voxelSideLength + 
                    yVoxelIndex * Vector3.up * ghm.voxelSideLength + 
                    zVoxelIndex * Vector3.forward * ghm.voxelSideLength;
                Voxel newVoxel = curVoxelObject.AddComponent<Voxel>();
                newVoxel.stableLifeTime = ghm.stableLifeTime;
                ghm.voxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex] = curVoxelObject;
            }

            curVoxelObject.SetActive(true);
            Voxel curVoxelScript = curVoxelObject.GetComponent<Voxel>();
            curVoxelScript.BeGazedAt();

            // 更新手柄体素队列
            UpdateHandVoxelQueue(curVoxelObject);

            //Debug.Log("手柄体素队列元素数量："+latestHandVoxelQueue.Count);

            // 计算当前队列标准差
            if (latestHandVoxelQueue.Count == handVoxelCount)
            {
                float currentStdDev = CalculateStdDev();
                
                // 更新标准差窗口队列
                MaintainStdDevQueue(currentStdDev);

                // 检验标准差稳定态
                isStationary = StdDevStableCheck() && currentStdDev < 0.5f;
            }
            return ReturnStatus.SUCCESS;

        }

        private void UpdateHandVoxelQueue(GameObject newVoxel)
        {
            Voxel voxelScrpit = newVoxel.GetComponent<Voxel>();
            // 只有不在队列中的体素才会被加入
            // 避免重复体素加入可以更好观察手柄的空间轨迹
            if (voxelScrpit.isInHandQueue) { return; }
            if (latestHandVoxelQueue.Count == handVoxelCount)
            {
                GameObject dropVoxel = latestHandVoxelQueue.Dequeue();
                dropVoxel.GetComponent<Voxel>().isInHandQueue = false;
            }
            latestHandVoxelQueue.Enqueue(newVoxel);
            voxelScrpit.isInHandQueue = true;
        }

        /// <summary>
        /// 计算当前体素队列的标准差
        /// </summary>
        private float CalculateStdDev()
        {
            Vector3[] positions = latestHandVoxelQueue.Select(go => go.transform.position).ToArray();
            List<Vector3> positionList = positions.ToList();

            // 计算出平均坐标
            Vector3 meanPoint = Vector3.zero;
            foreach (Vector3 pos in positionList)
            {
                meanPoint += pos;
            }
            meanPoint /= positionList.Count;

            // 计算出方差
            float variance = 0f;
            foreach (Vector3 pos in positionList)
            {
                variance += (pos - meanPoint).sqrMagnitude;
            }
            variance /= positionList.Count;

            // 计算坐标的标准差
            float standardDeviation = Mathf.Sqrt(variance);

            if (isSampleData)
            {
                smSamplerScript.sampleHandleStdInfo(standardDeviation);
            }

            return standardDeviation;
        }

        /// <summary>
        /// 维护标准差队列
        /// </summary>
        /// <param name="sd"></param>
        private void MaintainStdDevQueue(float currentStdDev)
        {
            // 将当前标准差添加到滑动窗口中
            dataQueue.Enqueue(currentStdDev);
            numObservations++;
            numFrames++;

            // 如果观测值数量超过最大值，则移除最早的一个值
            if (numObservations > windowSize)
            {
                dataQueue.Dequeue();
                numObservations--;
            }
        }

        /// <summary>
        /// 检测标准差时间序列是否进入稳定状态
        /// 目前是简单的实现，更加复杂的方法有单位根检验
        /// </summary>
        /// <returns></returns>
        private bool StdDevStableCheck()
        {
            // 如果当前观测值数量大于等于滑动窗口大小，则进行稳定态检验
            if (numObservations == windowSize && numFrames>minFramesNeed)
            {
                // 将标准差序列转化为float数组
                float[] data = dataQueue.ToArray();

                float mean = 0f;
                foreach (float var in data)
                {
                    mean += var;
                }
                mean /= data.Length;

                float varianceOfMean = 0f;
                foreach (float var in data)
                {
                    varianceOfMean += Mathf.Pow(var - mean, 2f);
                }
                varianceOfMean /= data.Length;

                float standardDeviationOfMean = Mathf.Sqrt(varianceOfMean);

                if (isSampleData)
                {
                    smSamplerScript.sampleHandleStdStdInfo(standardDeviationOfMean);
                }

                if (standardDeviationOfMean < stableThreshold)
                {
                    Debug.Log("Standard deviation has stabilized.");
                    return true;
                }

                // KPSS
                //float kpssState,pValue;
                //isStationary = KPSSValidate.Test(data, 1,out kpssState,out pValue);
            }

            return false;
        }

        public void InitialHandHeatmap()
        {
            latestHandVoxelQueue.Clear(); // 体素队列
            dataQueue.Clear(); // 滑动窗口，保存当前标准差
            numObservations = 0; // 观测值数量
            numFrames = 0;
            isStationary = false; // 是否趋于平稳

        }

    }
}

