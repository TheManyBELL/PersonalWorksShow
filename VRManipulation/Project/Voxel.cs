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
        public float stableLifeTime; // 正常寿命,在RoomManagerment中初始化
        public float currenLifeTime; // 当前剩余寿命
        public float magnification = 2f; // 凝视点热度上升速度，数值越大上升越慢

        // 用于更新体素列表
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
        /// 被凝视到的时候调用
        /// </summary>
        public void BeGazedAt()
        {
            //Debug.Log("体素被凝视");
            if (status.Equals(VoxelStatus.DEAD) || status.Equals(VoxelStatus.ALIVE_LOSSBLOOD))
            {
                status = VoxelStatus.ALIVE_STABEL;
                currenLifeTime = stableLifeTime;
            }
            gazeTimes++;
        }


        /// <summary>
        /// 每帧减少体素稳定态生命
        /// </summary>
        public void DecreaseStableLifeTime()
        {
            if (status.Equals(VoxelStatus.ALIVE_STABEL))
            {
                // 稳定存活时调用
                currenLifeTime -= Time.fixedDeltaTime;
                if (currenLifeTime < 0)
                {
                    //Debug.Log("体素稳定态结束");
                    status = VoxelStatus.ALIVE_LOSSBLOOD;
                    // 最原始的方法是直接用体素的凝视热度代表生命值
                    currenLifeTime = gazeTimes;
                    // 一种改良是转换为热度值[0,255]
                }
            }
        }

        /// <summary>
        /// 每帧减少体素热度
        /// </summary>
        public void DecreaseGazeTimesLife()
        {
            if (status.Equals(VoxelStatus.ALIVE_LOSSBLOOD))
            {
                currenLifeTime -= magnification * Time.fixedDeltaTime;
                gazeTimes = Mathf.Max(Mathf.FloorToInt(currenLifeTime), 0);
                if (currenLifeTime < 0)
                {
                    //Debug.Log("体素死亡");
                    status = VoxelStatus.DEAD;
                    voxelObject.SetActive(false);
                }
            }
        }
    }
}

