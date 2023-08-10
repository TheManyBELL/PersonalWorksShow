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
        private GameObject targetObject; // ��������
        private ManipulateStatus manipulateStatus; // ����״̬���ɿ���������

        public GameObject ghostPlanePrefab; // ����ƽ��Ԥ����
        private GameObject ghostPlane; // ����ƽ��
        public float radius_gp; //����ƽ��İ뾶

        // ���ӵ���ӻ�
        public bool isGazePointVisible = false;
        public GameObject staringPointPrefab;
        private GameObject staringPointObject;
        

        // Start is called before the first frame update
        void Start()
        {
            if (isOpenEyeTrack)
            {
                // ��ȡ���ӽű�
                sRanipalGazeSample = GazeRaySample.GetComponent<SRanipal_GazeRaySample_v2>();
            }
            
            // ��ʼ������ƽ��
            ghostPlane = Instantiate(ghostPlanePrefab); // ʵ����

            ghostPlane.transform.localScale = new Vector3(radius_gp, radius_gp, 1f); // ���óߴ�

            // ��ʼ�����ӵ�
            if (isGazePointVisible)
            {
                staringPointObject = Instantiate(staringPointPrefab);
                staringPointObject.SetActive(true);// �ر���
            }
            
        }

        // Update is called once per frame
        void Update()
        {
            if (!manipulateStatus.Equals(ManipulateStatus.UNSELECTED))
            {
                ghostPlane.transform.position = targetObject.transform.position;

                ghostPlane.transform.forward = Camera.main.transform.forward; // ������ĳ���ͬ��
            }
        }

        /// <summary>
        /// ��SpaceManipulateController���ã��ڻ�ȡ�������ý��и�ֵ
        /// </summary>
        /// <param name="target_object"></param>
        public void SetTargetObject(GameObject target_object)
        {
            targetObject = target_object;
        }

        public void SetManipulateStatus(ManipulateStatus status)
        {
            manipulateStatus = status;

            // ���ݲ���״̬��������
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
        /// �����۶��ǵ���������gaze_direct���۾�λ���������߲���ײ
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
                // �������ӵ���ӻ������λ�úͳ���
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

