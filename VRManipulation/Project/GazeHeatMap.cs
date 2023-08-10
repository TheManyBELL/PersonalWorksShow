using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using Valve.VR;
using Valve.VR.Extras;
using System.Linq;

namespace SceneAware
{
    public class GazeHeatMap : MonoBehaviour
    {
        // 房间基本信息
        public GameObject roomObject;
        [HideInInspector] public Bounds roomBounds;
        [HideInInspector] public float roomLengthX;
        [HideInInspector] public float roomLengthY;
        [HideInInspector] public float roomLengthZ;
        [HideInInspector] public Vector3 roomOrigin;

        // 房间体素化相关信息
        public float voxelSideLength;
        public GameObject voxelPrefab; // 体素预制体，用于可视化
        public float stableLifeTime;
        [HideInInspector] public GameObject voxelParentObject; // 体素父物体
        [HideInInspector] public int voxelNumberX;
        [HideInInspector] public int voxelNumberY;
        [HideInInspector] public int voxelNumberZ;
        [HideInInspector] public List<List<List<GameObject>>> voxelArray;

        // 最近n帧访问体素列表
        public Queue<GameObject> latestVoxelQueue;
        public int voxelCount;

        // 最近n帧手柄运动标准差序列
        public int windowSize; // 窗口（队列）大小
        public float stableThreshold; // 稳定阈值

        private Queue<float> dataQueue = new Queue<float>(); // 滑动窗口，保存当前标准差
        private int numObservations = 0; // 观测值数量:每帧观测一次凝视
        private int numFrames = 0; //
        public int minFramesNeed = 100;
        public float maxGazeStableStd = 0.1f;

        [HideInInspector]public bool isStationary = false; // 是否趋于平稳

        public GameObject StaringCenterPrefab; // 凝视中心预制体
        [HideInInspector] public GameObject staringCenterObject; // 凝视中心对象


        // Start is called before the first frame update
        void Start()
        {
            InitialRoomBasicData();
            InitialRoomVoxelArray();

            latestVoxelQueue = new Queue<GameObject>();

            staringCenterObject = Instantiate(StaringCenterPrefab);
            staringCenterObject.SetActive(false);
            staringCenterObject.name = "凝视中心点";

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 初始化房间基本信息
        /// </summary>
        void InitialRoomBasicData()
        {
            roomBounds = roomObject.GetComponent<BoxCollider>().bounds;
            roomLengthX = roomBounds.size.x;
            roomLengthY = roomBounds.size.y;
            roomLengthZ = roomBounds.size.z;
            roomOrigin = roomBounds.min;
        }

        /// <summary>
        /// 房间体素化，用体素阵列填充房间
        /// </summary>
        private void InitialRoomVoxelArray()
        {
            // 计算三个轴上体素数量，向上取整
            voxelNumberX = Mathf.CeilToInt(roomLengthX / voxelSideLength);
            voxelNumberY = Mathf.CeilToInt(roomLengthY / voxelSideLength);
            voxelNumberZ = Mathf.CeilToInt(roomLengthZ / voxelSideLength);

            // 创建体素阵列父物体
            voxelParentObject = new GameObject("体素阵列父物体");

            // 创建空的体素阵列
            voxelArray = new List<List<List<GameObject>>>();
            for (int ix = 0; ix < voxelNumberX; ix++)
            {
                List<List<GameObject>> tempXList = new List<List<GameObject>>();
                for (int iy = 0; iy < voxelNumberY; iy++)
                {
                    List<GameObject> tempYList = new List<GameObject>();
                    for (int iz = 0; iz < voxelNumberZ; iz++)
                    {
                        tempYList.Add(null);
                    }
                    tempXList.Add(tempYList);
                }
                voxelArray.Add(tempXList);
            }
        }

        /// <summary>
        /// 根据凝视点更新对应体素热度
        /// 需要被凝视脚本调用
        /// </summary>
        /// <param name="gazePoint"></param>
        public ReturnStatus UpdateHeatMap(Vector3 gazePoint)
        {

            int xVoxelIndex = Mathf.FloorToInt((gazePoint.x - roomOrigin.x) / voxelSideLength - 0.5f) + 1;
            int yVoxelIndex = Mathf.FloorToInt((gazePoint.y - roomOrigin.y) / voxelSideLength - 0.5f) + 1;
            int zVoxelIndex = Mathf.FloorToInt((gazePoint.z - roomOrigin.z) / voxelSideLength - 0.5f) + 1;

            // 判断凝视点越界
            if (xVoxelIndex >= voxelNumberX || xVoxelIndex < 0 || yVoxelIndex >= voxelNumberY || yVoxelIndex < 0 || zVoxelIndex >= voxelNumberZ || zVoxelIndex < 0)
            {
                return ReturnStatus.WAR_GAZE_CROSS_BORDER;
            }

            GameObject targetVoxelObject = voxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];
            if (targetVoxelObject == null)
            {
                targetVoxelObject = Instantiate(voxelPrefab, voxelParentObject.transform);
                targetVoxelObject.name = string.Format("voxel_{0}{1}{2}", xVoxelIndex, yVoxelIndex, zVoxelIndex);
                targetVoxelObject.SetActive(false);
                targetVoxelObject.isStatic = true;
                targetVoxelObject.transform.localScale = new Vector3(voxelSideLength, voxelSideLength, voxelSideLength);
                targetVoxelObject.transform.position = roomOrigin + xVoxelIndex * Vector3.right * voxelSideLength + yVoxelIndex * Vector3.up * voxelSideLength + zVoxelIndex * Vector3.forward * voxelSideLength;
                Voxel newVoxel = targetVoxelObject.AddComponent<Voxel>();
                newVoxel.stableLifeTime = stableLifeTime;
                voxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex] = targetVoxelObject;
            }
            targetVoxelObject.SetActive(true);
            Voxel targetVoxelScript = targetVoxelObject.GetComponent<Voxel>();
            targetVoxelScript.BeGazedAt();

            // 更新体素队列
            UpdateVoxelQueue(targetVoxelObject);

            // 计算当前队列标准差
            if (latestVoxelQueue.Count == voxelCount)
            {
                // float currentStdDev = CalculateStdDev(); // 基础标准差算法
                float currentStdDev = CalculateStdDevByWeight(); // 进阶：考虑体素热度权重

                MaintainStdDevQueue(currentStdDev);

                // 检验标准差稳定态
                isStationary = StdDevStableCheck();
            }

            return ReturnStatus.SUCCESS;

        }

        private void UpdateVoxelQueue(GameObject newVoxel)
        {
            Voxel voxelScrpit = newVoxel.GetComponent<Voxel>();
            if (voxelScrpit.isInQueue) { return; }
            if (latestVoxelQueue.Count == voxelCount)
            {
                GameObject dropVoxel = latestVoxelQueue.Dequeue();
                dropVoxel.GetComponent<Voxel>().isInQueue = false;

            }
            latestVoxelQueue.Enqueue(newVoxel);
            voxelScrpit.isInQueue = true;

        }

        public void InitialHeatmap()
        {
            // 初始化体素队列
            latestVoxelQueue.Clear();

            dataQueue.Clear();

            numObservations = 0; // 观测值数量
            numFrames = 0;
            isStationary = false; // 是否趋于平稳

            // 初始化热度图：获取体素父物体，清空所有子物体
            if (voxelParentObject.transform.childCount > 0)
            {
                for(int i = 0; i< voxelParentObject.transform.childCount; i++)
                {
                    Destroy(voxelParentObject.transform.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>
        /// 计算当前体素队列的标准差
        /// </summary>
        private float CalculateStdDev()
        {
            Vector3[] positions = latestVoxelQueue.Select(go => go.transform.position).ToArray();
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

            // 可视化
            staringCenterObject.transform.position = meanPoint;

            return standardDeviation;
        }

        private float CalculateStdDevByWeight()
        {
            GameObject[] latestVoxelArray = new GameObject[voxelCount];
            latestVoxelQueue.CopyTo(latestVoxelArray, 0);

            // 热度大的体素在计算方差时会被多次计算
            int sample_num = 0;
            float x_mean = 0, y_mean = 0, z_mean = 0;
            // 计算总体均值（中心点）
            for (int i = 0; i < latestVoxelArray.Length; i++)
            {
                GameObject temp_voxel = latestVoxelArray[i];
                Voxel temp_script = temp_voxel.GetComponent<Voxel>();
                int voxel_gaze_times = temp_script.gazeTimes;
                if (voxel_gaze_times == 0) { continue; } // 热度为0的体素不会被计算
                sample_num += voxel_gaze_times; // 体素被凝视几次就算几个样本
                x_mean += voxel_gaze_times * temp_voxel.transform.position.x;
                y_mean += voxel_gaze_times * temp_voxel.transform.position.y;
                z_mean += voxel_gaze_times * temp_voxel.transform.position.z;
            }

            x_mean /= sample_num;
            y_mean /= sample_num;
            z_mean /= sample_num;

            Vector3 sample_mean_point = new Vector3(x_mean, y_mean, z_mean);

            // 可视化
            staringCenterObject.transform.position = sample_mean_point;

            float sum = 0;

            for (int i = 0; i < latestVoxelArray.Length; i++)
            {
                Vector3 voxel_position = latestVoxelArray[i].transform.position;
                int gaze_times = latestVoxelArray[i].GetComponent<Voxel>().gazeTimes;
                float dis = Vector3.Distance(sample_mean_point, voxel_position);
                sum += dis * dis * gaze_times; // gaze_times=0的体素不会被算入
            }

            float sample_variance = sum / (sample_num - 1);
            float sd = Mathf.Sqrt(sample_variance); // 标准差代表散度

            return sd;
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
            if (numObservations == windowSize && numFrames > minFramesNeed)
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
    }
}

