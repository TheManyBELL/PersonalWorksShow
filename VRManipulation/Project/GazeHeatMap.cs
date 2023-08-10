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
        // ���������Ϣ
        public GameObject roomObject;
        [HideInInspector] public Bounds roomBounds;
        [HideInInspector] public float roomLengthX;
        [HideInInspector] public float roomLengthY;
        [HideInInspector] public float roomLengthZ;
        [HideInInspector] public Vector3 roomOrigin;

        // �������ػ������Ϣ
        public float voxelSideLength;
        public GameObject voxelPrefab; // ����Ԥ���壬���ڿ��ӻ�
        public float stableLifeTime;
        [HideInInspector] public GameObject voxelParentObject; // ���ظ�����
        [HideInInspector] public int voxelNumberX;
        [HideInInspector] public int voxelNumberY;
        [HideInInspector] public int voxelNumberZ;
        [HideInInspector] public List<List<List<GameObject>>> voxelArray;

        // ���n֡���������б�
        public Queue<GameObject> latestVoxelQueue;
        public int voxelCount;

        // ���n֡�ֱ��˶���׼������
        public int windowSize; // ���ڣ����У���С
        public float stableThreshold; // �ȶ���ֵ

        private Queue<float> dataQueue = new Queue<float>(); // �������ڣ����浱ǰ��׼��
        private int numObservations = 0; // �۲�ֵ����:ÿ֡�۲�һ������
        private int numFrames = 0; //
        public int minFramesNeed = 100;
        public float maxGazeStableStd = 0.1f;

        [HideInInspector]public bool isStationary = false; // �Ƿ�����ƽ��

        public GameObject StaringCenterPrefab; // ��������Ԥ����
        [HideInInspector] public GameObject staringCenterObject; // �������Ķ���


        // Start is called before the first frame update
        void Start()
        {
            InitialRoomBasicData();
            InitialRoomVoxelArray();

            latestVoxelQueue = new Queue<GameObject>();

            staringCenterObject = Instantiate(StaringCenterPrefab);
            staringCenterObject.SetActive(false);
            staringCenterObject.name = "�������ĵ�";

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// ��ʼ�����������Ϣ
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
        /// �������ػ���������������䷿��
        /// </summary>
        private void InitialRoomVoxelArray()
        {
            // ��������������������������ȡ��
            voxelNumberX = Mathf.CeilToInt(roomLengthX / voxelSideLength);
            voxelNumberY = Mathf.CeilToInt(roomLengthY / voxelSideLength);
            voxelNumberZ = Mathf.CeilToInt(roomLengthZ / voxelSideLength);

            // �����������и�����
            voxelParentObject = new GameObject("�������и�����");

            // �����յ���������
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
        /// �������ӵ���¶�Ӧ�����ȶ�
        /// ��Ҫ�����ӽű�����
        /// </summary>
        /// <param name="gazePoint"></param>
        public ReturnStatus UpdateHeatMap(Vector3 gazePoint)
        {

            int xVoxelIndex = Mathf.FloorToInt((gazePoint.x - roomOrigin.x) / voxelSideLength - 0.5f) + 1;
            int yVoxelIndex = Mathf.FloorToInt((gazePoint.y - roomOrigin.y) / voxelSideLength - 0.5f) + 1;
            int zVoxelIndex = Mathf.FloorToInt((gazePoint.z - roomOrigin.z) / voxelSideLength - 0.5f) + 1;

            // �ж����ӵ�Խ��
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

            // �������ض���
            UpdateVoxelQueue(targetVoxelObject);

            // ���㵱ǰ���б�׼��
            if (latestVoxelQueue.Count == voxelCount)
            {
                // float currentStdDev = CalculateStdDev(); // ������׼���㷨
                float currentStdDev = CalculateStdDevByWeight(); // ���ף����������ȶ�Ȩ��

                MaintainStdDevQueue(currentStdDev);

                // �����׼���ȶ�̬
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
            // ��ʼ�����ض���
            latestVoxelQueue.Clear();

            dataQueue.Clear();

            numObservations = 0; // �۲�ֵ����
            numFrames = 0;
            isStationary = false; // �Ƿ�����ƽ��

            // ��ʼ���ȶ�ͼ����ȡ���ظ����壬�������������
            if (voxelParentObject.transform.childCount > 0)
            {
                for(int i = 0; i< voxelParentObject.transform.childCount; i++)
                {
                    Destroy(voxelParentObject.transform.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>
        /// ���㵱ǰ���ض��еı�׼��
        /// </summary>
        private float CalculateStdDev()
        {
            Vector3[] positions = latestVoxelQueue.Select(go => go.transform.position).ToArray();
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

            // ���ӻ�
            staringCenterObject.transform.position = meanPoint;

            return standardDeviation;
        }

        private float CalculateStdDevByWeight()
        {
            GameObject[] latestVoxelArray = new GameObject[voxelCount];
            latestVoxelQueue.CopyTo(latestVoxelArray, 0);

            // �ȶȴ�������ڼ��㷽��ʱ�ᱻ��μ���
            int sample_num = 0;
            float x_mean = 0, y_mean = 0, z_mean = 0;
            // ���������ֵ�����ĵ㣩
            for (int i = 0; i < latestVoxelArray.Length; i++)
            {
                GameObject temp_voxel = latestVoxelArray[i];
                Voxel temp_script = temp_voxel.GetComponent<Voxel>();
                int voxel_gaze_times = temp_script.gazeTimes;
                if (voxel_gaze_times == 0) { continue; } // �ȶ�Ϊ0�����ز��ᱻ����
                sample_num += voxel_gaze_times; // ���ر����Ӽ��ξ��㼸������
                x_mean += voxel_gaze_times * temp_voxel.transform.position.x;
                y_mean += voxel_gaze_times * temp_voxel.transform.position.y;
                z_mean += voxel_gaze_times * temp_voxel.transform.position.z;
            }

            x_mean /= sample_num;
            y_mean /= sample_num;
            z_mean /= sample_num;

            Vector3 sample_mean_point = new Vector3(x_mean, y_mean, z_mean);

            // ���ӻ�
            staringCenterObject.transform.position = sample_mean_point;

            float sum = 0;

            for (int i = 0; i < latestVoxelArray.Length; i++)
            {
                Vector3 voxel_position = latestVoxelArray[i].transform.position;
                int gaze_times = latestVoxelArray[i].GetComponent<Voxel>().gazeTimes;
                float dis = Vector3.Distance(sample_mean_point, voxel_position);
                sum += dis * dis * gaze_times; // gaze_times=0�����ز��ᱻ����
            }

            float sample_variance = sum / (sample_num - 1);
            float sd = Mathf.Sqrt(sample_variance); // ��׼�����ɢ��

            return sd;
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
            if (numObservations == windowSize && numFrames > minFramesNeed)
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

