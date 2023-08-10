using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SceneAware
{
    public class HandHeatMap : MonoBehaviour
    {
        // ����GazeHeatMap�еĳ���������Ϣ
        private GazeHeatMap ghm;

        // ���n֡�ֱ����ʵ������б�
        public Queue<GameObject> latestHandVoxelQueue;
        public int handVoxelCount;

        // ���n֡�ֱ��˶���׼������
        public int windowSize; // ���ڣ����У���С
        public float stableThreshold; // �ȶ���ֵ

        private Queue<float> dataQueue = new Queue<float>(); // �������ڣ����浱ǰ��׼��
        private int numObservations = 0; // �۲�ֵ����
        private int numFrames = 0;
        [HideInInspector]public bool isStationary = false; // �Ƿ�����ƽ��
        public int minFramesNeed = 100;

        // ����
        public bool isSampleData = false;
        private SMSampler smSamplerScript; // �����ű�


        // Start is called before the first frame update
        void Start()
        {
            ghm = GetComponent<GazeHeatMap>();

            latestHandVoxelQueue = new Queue<GameObject>();

            // ����
            if (isSampleData) { smSamplerScript = GetComponent<SMSampler>(); }
            

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// ��manipulateController���ã������ֱ���ǰ֡����
        /// </summary>
        /// <param name="handPosition"></param>
        /// <returns></returns>
        public ReturnStatus UpdateHandHeatMap(Vector3 handPosition)
        {
            int xVoxelIndex = Mathf.FloorToInt((handPosition.x - ghm.roomOrigin.x) / ghm.voxelSideLength - 0.5f) + 1;
            int yVoxelIndex = Mathf.FloorToInt((handPosition.y - ghm.roomOrigin.y) / ghm.voxelSideLength - 0.5f) + 1;
            int zVoxelIndex = Mathf.FloorToInt((handPosition.z - ghm.roomOrigin.z) / ghm.voxelSideLength - 0.5f) + 1;

            // �ж��ֱ�λ��Խ��
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

            // �����ֱ����ض���
            UpdateHandVoxelQueue(curVoxelObject);

            //Debug.Log("�ֱ����ض���Ԫ��������"+latestHandVoxelQueue.Count);

            // ���㵱ǰ���б�׼��
            if (latestHandVoxelQueue.Count == handVoxelCount)
            {
                float currentStdDev = CalculateStdDev();
                
                // ���±�׼��ڶ���
                MaintainStdDevQueue(currentStdDev);

                // �����׼���ȶ�̬
                isStationary = StdDevStableCheck() && currentStdDev < 0.5f;
            }
            return ReturnStatus.SUCCESS;

        }

        private void UpdateHandVoxelQueue(GameObject newVoxel)
        {
            Voxel voxelScrpit = newVoxel.GetComponent<Voxel>();
            // ֻ�в��ڶ����е����زŻᱻ����
            // �����ظ����ؼ�����Ը��ù۲��ֱ��Ŀռ�켣
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
        /// ���㵱ǰ���ض��еı�׼��
        /// </summary>
        private float CalculateStdDev()
        {
            Vector3[] positions = latestHandVoxelQueue.Select(go => go.transform.position).ToArray();
            List<Vector3> positionList = positions.ToList();

            // �����ƽ������
            Vector3 meanPoint = Vector3.zero;
            foreach (Vector3 pos in positionList)
            {
                meanPoint += pos;
            }
            meanPoint /= positionList.Count;

            // ���������
            float variance = 0f;
            foreach (Vector3 pos in positionList)
            {
                variance += (pos - meanPoint).sqrMagnitude;
            }
            variance /= positionList.Count;

            // ��������ı�׼��
            float standardDeviation = Mathf.Sqrt(variance);

            if (isSampleData)
            {
                smSamplerScript.sampleHandleStdInfo(standardDeviation);
            }

            return standardDeviation;
        }

        /// <summary>
        /// ά����׼�����
        /// </summary>
        /// <param name="sd"></param>
        private void MaintainStdDevQueue(float currentStdDev)
        {
            // ����ǰ��׼����ӵ�����������
            dataQueue.Enqueue(currentStdDev);
            numObservations++;
            numFrames++;

            // ����۲�ֵ�����������ֵ�����Ƴ������һ��ֵ
            if (numObservations > windowSize)
            {
                dataQueue.Dequeue();
                numObservations--;
            }
        }

        /// <summary>
        /// ����׼��ʱ�������Ƿ�����ȶ�״̬
        /// Ŀǰ�Ǽ򵥵�ʵ�֣����Ӹ��ӵķ����е�λ������
        /// </summary>
        /// <returns></returns>
        private bool StdDevStableCheck()
        {
            // �����ǰ�۲�ֵ�������ڵ��ڻ������ڴ�С��������ȶ�̬����
            if (numObservations == windowSize && numFrames>minFramesNeed)
            {
                // ����׼������ת��Ϊfloat����
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
            latestHandVoxelQueue.Clear(); // ���ض���
            dataQueue.Clear(); // �������ڣ����浱ǰ��׼��
            numObservations = 0; // �۲�ֵ����
            numFrames = 0;
            isStationary = false; // �Ƿ�����ƽ��

        }

    }
}

