using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using Valve.VR;
using Valve.VR.Extras;

namespace SceneAware
{
    public enum ReturnStatus { SUCCESS=0,WAR_GAZE_NOT_HIT,WAR_GAZE_CROSS_BORDER,GAIN_CHANGED,GAIN_NOT_CHANGED}

    public class GhostGaze : MonoBehaviour
    {
        public GameObject GazeRaySample;
        public bool isOpenEyeTrack = false;
        private SRanipal_GazeRaySample_v2 sRanipalGazeSample;
        private SpaceManipulateController spaceManipulateController;
        private GameObject targetObject; // 操作对象
        private ManipulateStatus manipulateStatus; // 操作状态，由控制器更改

        public GameObject ghostPlanePrefab; // 幽灵平面预制体
        private GameObject ghostPlane; // 幽灵平面
        public float radius_gp; //幽灵平面的半径

        // 凝视点可视化
        public bool isGazePointVisible = false;
        public GameObject staringPointPrefab;
        private GameObject staringPointObject;
        

        // Start is called before the first frame update
        void Start()
        {
            if (isOpenEyeTrack)
            {
                // 获取凝视脚本
                sRanipalGazeSample = GazeRaySample.GetComponent<SRanipal_GazeRaySample_v2>();
            }
            
            // 初始化幽灵平面
            ghostPlane = Instantiate(ghostPlanePrefab); // 实例化

            ghostPlane.transform.localScale = new Vector3(radius_gp, radius_gp, 1f); // 设置尺寸

            // 初始化凝视点
            if (isGazePointVisible)
            {
                staringPointObject = Instantiate(staringPointPrefab);
                staringPointObject.SetActive(true);// 关闭了
            }
            
        }

        // Update is called once per frame
        void Update()
        {
            if (!manipulateStatus.Equals(ManipulateStatus.UNSELECTED))
            {
                ghostPlane.transform.position = targetObject.transform.position;

                ghostPlane.transform.forward = Camera.main.transform.forward; // 和相机的朝向同步
            }
        }

        /// <summary>
        /// 由SpaceManipulateController调用，在获取物体后调用进行赋值
        /// </summary>
        /// <param name="target_object"></param>
        public void SetTargetObject(GameObject target_object)
        {
            targetObject = target_object;
        }

        public void SetManipulateStatus(ManipulateStatus status)
        {
            manipulateStatus = status;

            // 根据操作状态开关物体
            if (manipulateStatus.Equals(ManipulateStatus.SELECTED))
            {
                ghostPlane.SetActive(true);
                if (isGazePointVisible)
                {
                    staringPointObject.SetActive(true);
                }
            }
            else if (manipulateStatus.Equals(ManipulateStatus.UNSELECTED))
            {
                ghostPlane.SetActive(false);
                if (isGazePointVisible)
                {
                    staringPointObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 根据眼动仪的凝视向量gaze_direct和眼睛位置生成射线并碰撞
        /// </summary>
        /// <param name="gaze_point"></param>
        /// <returns></returns>
        public ReturnStatus GetStaringPoint(ref Vector3 gaze_point)
        {
            // Vector3 gazeDirect = sRanipalGazeSample.GazeDirectionCombined;
            Vector3 gazeDirect = Camera.main.transform.forward;
            Vector3 eyePosition = Camera.main.transform.position;

            Ray gazeRay = new Ray(eyePosition, gazeDirect);
            RaycastHit hitInfo;

            if (Physics.Raycast(gazeRay, out hitInfo))
            {
                gaze_point = hitInfo.point;
                // 更新凝视点可视化对象的位置和朝向
                if (isGazePointVisible)
                {
                    staringPointObject.transform.position = hitInfo.point + hitInfo.normal * .02f;
                    staringPointObject.transform.forward = hitInfo.normal;
                }
                return ReturnStatus.SUCCESS;
            }
            else
            {
                return ReturnStatus.WAR_GAZE_NOT_HIT;
            }
            
        }


    }
}

