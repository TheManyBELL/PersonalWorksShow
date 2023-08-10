using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneAware
{
    public enum VoxelStatus { DEAD = 0, ALIVE_STABEL, ALIVE_LOSSBLOOD }
    public class Voxel : MonoBehaviour
    {
        [HideInInspector] public GameObject voxelObject;
        [HideInInspector] public int gazeTimes;
        [HideInInspector] public VoxelStatus status;
        public int serialNumberX;
        public int serialNumberY;
        public int serialNumberZ;
        public float stableLifeTime; // ��������,��RoomManagerment�г�ʼ��
        public float currenLifeTime; // ��ǰʣ������
        public float magnification = 2f; // ���ӵ��ȶ������ٶȣ���ֵԽ������Խ��

        // ���ڸ��������б�
        public bool isInQueue = false;
        public bool isInHandQueue = false;

        // Start is called before the first frame update
        void Start()
        {
            voxelObject = this.gameObject;
            gazeTimes = 0;
            status = VoxelStatus.DEAD;
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void FixedUpdate()
        {
            DecreaseStableLifeTime();
            DecreaseGazeTimesLife();
        }

        /// <summary>
        /// �����ӵ���ʱ�����
        /// </summary>
        public void BeGazedAt()
        {
            //Debug.Log("���ر�����");
            if (status.Equals(VoxelStatus.DEAD) || status.Equals(VoxelStatus.ALIVE_LOSSBLOOD))
            {
                status = VoxelStatus.ALIVE_STABEL;
                currenLifeTime = stableLifeTime;
            }
            gazeTimes++;
        }


        /// <summary>
        /// ÿ֡���������ȶ�̬����
        /// </summary>
        public void DecreaseStableLifeTime()
        {
            if (status.Equals(VoxelStatus.ALIVE_STABEL))
            {
                // �ȶ����ʱ����
                currenLifeTime -= Time.fixedDeltaTime;
                if (currenLifeTime < 0)
                {
                    //Debug.Log("�����ȶ�̬����");
                    status = VoxelStatus.ALIVE_LOSSBLOOD;
                    // ��ԭʼ�ķ�����ֱ�������ص������ȶȴ�������ֵ
                    currenLifeTime = gazeTimes;
                    // һ�ָ�����ת��Ϊ�ȶ�ֵ[0,255]
                }
            }
        }

        /// <summary>
        /// ÿ֡���������ȶ�
        /// </summary>
        public void DecreaseGazeTimesLife()
        {
            if (status.Equals(VoxelStatus.ALIVE_LOSSBLOOD))
            {
                currenLifeTime -= magnification * Time.fixedDeltaTime;
                gazeTimes = Mathf.Max(Mathf.FloorToInt(currenLifeTime), 0);
                if (currenLifeTime < 0)
                {
                    //Debug.Log("��������");
                    status = VoxelStatus.DEAD;
                    voxelObject.SetActive(false);
                }
            }
        }
    }
}

