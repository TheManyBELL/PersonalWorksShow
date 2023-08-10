using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using Valve.VR;
using Valve.VR.Extras;
using System.Linq;
using UserStudy;

namespace SceneAware
{
    /// <summary>
    /// UNSELECT:δѡ������
    /// SELECTED��ѡ�����壬ֻ����ͷ������
    /// FAST_MANI�����ٲ���
    /// CHECK_HANDLE: ����ֱ�λ��
    /// FINE_MANI����ϸ����
    /// </summary>
    public enum ManipulateStatus { UNSELECTED = 0, SELECTED,FAST_MANI,CHECK_HANDLE,FINE_MANI,MORE_FINE_MANI };

    public struct PointInSpace
    {
        public float rate_x;
        public float rate_y;
        public float rate_z;
        public bool is_x_posiyive;
        public bool is_y_positive;
        //public bool is_z_positive;
    }

    public class SpaceManipulateController : MonoBehaviour
    {
        [Header("�ֱ���Ϣ")]
        [Tooltip("CameraRig�е������ֱ�����")]
        public GameObject rightHand;
        public GameObject leftHand;
        public SteamVR_Action_Boolean updateUserHead; // ���µ��ֲ����ռ䳯��
        public SteamVR_Action_Boolean selectUserShoulder; // ѡ���û����λ��
        private bool isUserShoulderSampled;
        public SteamVR_Action_Boolean selectManipulateObject; // ѡ����Ҫ����������
        // ��ת����������
        public SteamVR_Action_Boolean rotateObject; // ��ת����
        public SteamVR_Action_Boolean enlargeObject; // �Ŵ�����
        public SteamVR_Action_Boolean shrinkObject; // ��С����

        [Header("�û���Ϣ")]
        [Tooltip("�û��ֱ��ܳ���")]
        public float userArmLength; // �û��ֱ۳��ȣ���Ҫ����
        [Tooltip("�û���۳���")]
        public float userBoomLength; // �û���۳��ȣ����ڼ�������λ��
        
        [Header("���ֽ����ռ�")]
        [Tooltip("���ֽ����ռ�ķ�������")]
        public float userForearmScale = 1.2f; // �û��ռ�ߴ�����ű���
        private float heightOfHM; // ���ֲ����ռ�ĸߣ��̶���
        private Vector3 p_shoulder; // ������꣨�ֲ���
        private Vector3 p_elbow; // �������꣨�ֲ���

        // ���ײ��䣺���ʽ����ռ仮��
        private float comfort_level_1; // ������С�뾶
        private float comfort_level_2; // ���ʰ뾶������С�۳���
        private float comfort_level_3; // �������뾶���ٴ����ƣ��

        private GameObject userHead; // �û�ͷ��
        private GameObject userShoulder; // �û���������Ҫ����
        private GameObject localHandle; // ������ΪHead���ֱ�������

        [Header("���������ռ�")]
        [Tooltip("���������ռ��Զƽ�����")]
        public float farPlaneValue; // ��Ҫ�ֶ�����
        private float nearPlaneValue; // �������ȡ
        private Camera mainCamera; // ���������
        private float halfFov; // ��ֱFOVһ�룬����
        private float aspectFov; // ��׶��Ŀ�߱�
       
        private float heightOfFrustum; // ��ʼƽ��ͷ��ĸ�
        private float heightOfSM; // ���������ռ�ĸߣ�����£�
        private Vector3 anchor_scene; // ���������ռ�ê�㣬��ʼΪ�������ǰ����ƽ�洦

        private List<Vector3> cameraInfo; // �ӿ���Ϣ�����ꡢǰ�����Ϸ����ҷ�

        [Header("����������ű�")]
        [Tooltip("���������ķ�������")]
        public float scaleRate = 0.5f;
        [Tooltip("������-ϸ���Ƚ׶��������ӵ�������")]
        public float maxDistanceGO = 0.4f;

        [HideInInspector] public GameObject selectObject; // ��ѡ�����ڲ���������
        private ManipulateStatus manipulateStatus; // ��ǰ�����׶� 
        private Quaternion previousLeftHandRosition; // ��һ֡��ת�Ƕȣ����֣�
        private Quaternion previousRightHandRotation; // ��һ֡��ת�Ƕȣ����֣�
        private GazeHeatMap gazeHeatMap; // �����ȶ�ͼ   
        private GhostGaze ghostGaze; // �����㷨�ű�
        private HandHeatMap handHeatMap; // ���ȶ�ͼ

        // ������ؽű�    
        public bool isOpenEyeTrack = false;
        private GameObject GazeRaySample; // �۲����ٶ���
        private SRanipal_GazeRaySample_v2 sRanipalGazeSample; // ����GazeRaySample

        // -----����ʱ��Ҫά���ı���-----
        private PointInSpace handleInHM; // �ֱ��ڵ��ֲ����ռ��е�λ��
        private Vector3 previousRightHandPosition; // ��һ֡�ֱ�λ�ã����ڼ����ֱ��ƶ��ܾ���

        [Header("���ӻ���Ϣ")]
        public bool isOpenVisualize = false;
        public GameObject sceneSpacePrefab; // ���������ռ�
        public GameObject handSpacePrefab; // ���ֽ����ռ�
        public GameObject pointPrefab; // ���ڿ��ӻ�����ê��
        public GameObject LineRenderController; // ���ӻ�����

        private GameObject elbowObject;

        private GameObject spaceManipulateObject; // ���ӻ��ĳ��������ռ䣬����spacePrefab
        private LineRenderer lineRenderer; // 

        // ������ر���
        [Header("������Ϣ")]
        [Tooltip("Ŀ��λ�ô���ʾ������")]
        public GameObject targetObject; // ���ڼ�������Ŀ������
        [Tooltip("�Ƿ���������")]
        public bool isSampleData = false;
        // private SMSampler smSamplerScript; // �����ű�
        public GameObject UserStudyManager;
        private UserStudySampler usSampler; // �²����ű�
        private UserStudyController usController; // �û�ʵ����ƽű�
        private UserStudy2Controller us2Controller; // �û�ʵ����ƽű�2

        public bool isUserStudy1 = true;
        public bool isUserStudy2 = false;



        /// <summary>
        /// ��������
        /// 1.�ֱ���������ͷ�����򣨼��Ϊ�������壩
        /// 1.�ֱ�������ȡ���λ��
        /// 3.�ֱ����������ӿڳ���
        /// 4.�ֱ�����ѡȡĿ������
        /// </summary>

        void Start()
        {
            // ���û���ض���
            userHead = GameObject.Find("Head");
            userShoulder = GameObject.Find("Shoulder");
            localHandle = GameObject.Find("HandleRight");
            // ���۶�����
            if (isOpenEyeTrack)
            {
                GazeRaySample = GameObject.Find("Gaze Ray Sample v2");
                sRanipalGazeSample = GazeRaySample.GetComponent<SRanipal_GazeRaySample_v2>(); // ��ʼ�����ӽű�
            }
            // ���������
            mainCamera = Camera.main;
            halfFov = (mainCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad;
            aspectFov = mainCamera.aspect;
            nearPlaneValue = mainCamera.nearClipPlane; // �����ƽ��
            cameraInfo = new List<Vector3>(new Vector3[4]); // �ڳ���λ�׶λᶯ̬����
            // �����������ռ䡿
            heightOfFrustum = farPlaneValue-nearPlaneValue; // ���������ռ�ʵ�ʳߴ磨�ߣ�
            heightOfSM = heightOfFrustum; // ��ʼΪƽ��ͷ��ĸ�
            // �����ֽ����ռ䡿
            heightOfHM =(userArmLength - userBoomLength) * userForearmScale;
            // ��ʼ����������뾶
            comfort_level_1 = (userArmLength - userBoomLength) * 0.8f; // ��Ȧ
            comfort_level_2 = (userArmLength - userBoomLength) * 1f; // ��Ȧ
            comfort_level_3 = (userArmLength - userBoomLength) * 1.2f; //��Ȧ
            //Debug.Log("һ����������뾶��" + comfort_level_1.ToString());
            // ������������
            manipulateStatus = ManipulateStatus.UNSELECTED;
            isUserShoulderSampled = false;

            gazeHeatMap = GetComponent<GazeHeatMap>();// ��ȡ�����ȶ�ͼ�ű�����
            handHeatMap = GetComponent<HandHeatMap>();// ���ȶ�ͼ�ű�
            ghostGaze = GetComponent<GhostGaze>();// ��ȡ�������ӽű�����

            // �������ű���

            if (isSampleData)
            {
                usSampler = UserStudyManager.GetComponent<UserStudySampler>();
                if (isUserStudy1)
                {
                    usController = UserStudyManager.GetComponent<UserStudyController>();
                }
                else
                {
                    us2Controller = UserStudyManager.GetComponent<UserStudy2Controller>();
                }
            }

            // �����ӻ���
            if (isOpenVisualize)
            {
                spaceManipulateObject = Instantiate(sceneSpacePrefab); // ���������ռ���ӻ�
                spaceManipulateObject.SetActive(false);
                spaceManipulateObject.name = "���������ռ�";
            }

            lineRenderer = LineRenderController.GetComponent<LineRenderer>(); // ����-�ֱ����߿��ӻ�
            lineRenderer.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            // �����ֱ�����������꣬���ֱ�����ͬ��
            UpdateLocalHandle();

            // ----׼������----

            // ����ȷ�����ֲ����ռ�(Head)�����ê��λ��
            GetSHSForwardByPress();
            // ����ȷ�����λ��Head->Shoulder
            GetShoulderPosition();

            // ----��ʼ����----
            // δѡ���κ����壬������ȡ��������

            // 1.��⵽���°����&ϵͳ״̬Ϊδѡ�����Ի�ȡ����
            if (manipulateStatus.Equals(ManipulateStatus.UNSELECTED)){
                if (selectManipulateObject.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    Debug.Log("δѡ�����壬����ѡ������..");
                    GetObjectByHandleRay();
                }
            }
            // 2.��ϵͳ״̬����δѡ�����ȡ���ӵ㣬���ҿ�����ת������Ȩ��
            Vector3 p_gaze = new Vector3(); // ���ӵ�����
            if (!manipulateStatus.Equals(ManipulateStatus.UNSELECTED))
            {               
                ReturnStatus get_staring_point = ghostGaze.GetStaringPoint(ref p_gaze); // ��ȡ���ӵ�����
                if (get_staring_point.Equals(ReturnStatus.WAR_GAZE_NOT_HIT))
                {
                    Debug.LogWarning("WAR: Gaze not hit");
                }

                // �����¼�ֱ��ƶ��ܾ������ת�ܽǶ�
                if (isSampleData)
                {
                    usSampler.translatePathLength += Vector3.Distance(rightHand.transform.position, previousRightHandPosition);
                    usSampler.rotationTotalAngle += Quaternion.Angle(leftHand.transform.rotation, previousLeftHandRosition);

                    // �ж��Ƿ����ϸ���Ȳ�����ֵ
                    if (usSampler.isCoarse2FineRecorded == false && 
                        Vector3.Distance(targetObject.transform.position, selectObject.transform.position) < usSampler.coarse2FineThreshold){
                        usSampler.isCoarse2FineRecorded = true;
                        usSampler.coarse2FineTimePoint = Time.time;
                        Debug.Log("[������¼]���뾫ϸ�����׶�");
                    }  
                }

            }
            // 3.ϵͳ����ѡ��״̬
            if (manipulateStatus.Equals(ManipulateStatus.SELECTED))
            {
                // 3.1. ����ס��������򿪷��ӿ���ת
                if (selectManipulateObject.GetState(SteamVR_Input_Sources.RightHand))
                {
                    cameraInfo[0] = mainCamera.transform.position;
                    cameraInfo[1] = mainCamera.transform.forward;
                    cameraInfo[2] = mainCamera.transform.right;
                    cameraInfo[3] = mainCamera.transform.up;
                    // Debug.Log("����ѡ��׶Σ����������" + mainCamera.transform.forward.ToString());

                    anchor_scene = cameraInfo[0] + nearPlaneValue * cameraInfo[1]; // ���³���ê��

                    if (isOpenVisualize)
                    {
                        spaceManipulateObject.transform.position = anchor_scene; // ���³������ӻ�
                        spaceManipulateObject.transform.forward = cameraInfo[1];
                    }
                    

                    selectObject.transform.position = GetObjecPosition(anchor_scene, heightOfSM, localHandle.transform.localPosition, heightOfHM); // ������������
                }

                // 3.2.���ɿ�������ҵ�ǰ״̬Ϊ��ѡ�����л�ģʽ�����ٲ���
                if (selectManipulateObject.GetStateUp(SteamVR_Input_Sources.RightHand))
                {
                    Debug.Log("�ɿ��˰��,������ٲ����׶�");
                    manipulateStatus = ManipulateStatus.FAST_MANI;
                }
            }

            // 4.���ٲ����׶Σ����²�������λ�ã�������״̬
            if (manipulateStatus.Equals(ManipulateStatus.FAST_MANI))
            {
                // ���������ȶ�ͼ
                ReturnStatus update_heat_map = gazeHeatMap.UpdateHeatMap(p_gaze);
                if (update_heat_map.Equals(ReturnStatus.WAR_GAZE_CROSS_BORDER))
                {
                    Debug.LogWarning("WAR: Gaze point cross border");
                }

                // ��������������Ŀ�����ĵľ���distanceGazeObjet
                float distanceGazeObjet = Vector3.Distance(p_gaze, selectObject.transform.position);
                // Debug.Log("��ʱ���ӵ�������ľ���:" + distanceGazeObjet.ToString());

                // ����û��Ƿ���ͼ���뾫ϸ����ģʽ
                if (gazeHeatMap.isStationary == true && distanceGazeObjet < maxDistanceGO)
                {
                    Debug.Log("�뿪���ٲ��������뾫ϸ������");
                    heightOfSM = 1.5f * heightOfHM; // ���³����ռ��СΪ���ֿռ�:Ŀǰ��2��

                    // ���³��������ռ��ê��λ�úʹ�С
                    anchor_scene = GetSceneAnchor(selectObject.transform.position, heightOfSM, handleInHM);

                    // �ı����״̬�������ֱ����׶�
                    // 1.����ʱ�ֱ�������ê��ľ��룬���������ʰ뾶������ֱ�����״̬
                    // 2.����ֱ�ӽ��뾫ϸ�����׶�
                    float d_handle = Vector3.Distance(localHandle.transform.localPosition, p_elbow);
                    if(d_handle> comfort_level_3 || d_handle<comfort_level_1)
                    {
                        Debug.Log("�ֱ�λ����Ҫ���ã�"+d_handle.ToString());
                        manipulateStatus = ManipulateStatus.CHECK_HANDLE;
                        // ��¼һ�£�������ͬ��
                        if (isSampleData)
                        {
                            usSampler.isHandleReset = 1;
                        }
                    }
                    else
                    {
                        Debug.Log("�ֱ�λ������������");
                        manipulateStatus = ManipulateStatus.FINE_MANI;
                        if (isSampleData)
                        {
                            usSampler.isHandleReset = 0;
                        }
                    }

                    // ---- ���ӻ����� ----
                    if (isOpenVisualize)
                    {
                        spaceManipulateObject.transform.position = anchor_scene;
                        spaceManipulateObject.transform.localScale = heightOfSM * Vector3.one;
                        gazeHeatMap.staringCenterObject.SetActive(false);
                    }
                    
                }

                // ���²�������λ��
                selectObject.transform.position = GetObjecPosition(anchor_scene, heightOfSM, localHandle.transform.localPosition, heightOfHM);

            }

            // 5.�ֱ����ý׶Σ��Ǳ��룩
            if (manipulateStatus.Equals(ManipulateStatus.CHECK_HANDLE))
            {
                float d_handle = Vector3.Distance(localHandle.transform.localPosition ,p_elbow);

                // Vector3 v_elbow2handle = localHandle.transform.localPosition - p_elbow;
                Vector3 v_elbow2handle = rightHand.transform.position - elbowObject.transform.position;
                
                float d_angle = Vector3.Angle(v_elbow2handle, userHead.transform.forward) ; // TODO: δ������-���������뵥�ֿռ���ǰ���ļн�
                //Debug.DrawRay(elbowObject.transform.position, v_elbow2handle);
                //Debug.DrawRay(elbowObject.transform.position, userHead.transform.forward);
                // TODO��������Ը��ֱ����ӿ��ӻ����ڲ�ͬ�����������ֱ�����ɫ��ͬ
                Debug.Log("�����ֱ�����״̬,��ǰ����Ϊ:"+d_handle.ToString()+",�н�Ϊ:"+d_angle.ToString());
                if (d_handle < comfort_level_2 && d_handle > comfort_level_1 && d_angle < 20f)
                {
                    Debug.Log("�������ֱ�λ�ã�ӳ��ͬ�����ã���ǰ�Ƕȣ�"+d_angle.ToString());
                    handleInHM = GetAxisRateByPosition(localHandle.transform.localPosition, heightOfHM); // ���¼����ֱ�λ��
                    anchor_scene = GetSceneAnchor(selectObject.transform.position, heightOfSM, handleInHM); // �����µĳ���ê��
                    manipulateStatus = ManipulateStatus.FINE_MANI;

                    // ���ӻ�
                    if (isOpenVisualize)
                    {
                        spaceManipulateObject.transform.position = anchor_scene;
                    }
                    
                }
            }

            // 6.��ϸ�����׶�:��������������û�������ͼ�Խ��������ϸ����
            if (manipulateStatus.Equals(ManipulateStatus.FINE_MANI))
            {
                Debug.Log("����һ����ϸ�����׶�");
                // �������пռ�ӳ���Ը�������λ��
                selectObject.transform.position = GetObjecPosition(anchor_scene, heightOfSM, localHandle.transform.localPosition, heightOfHM);

                // ��ʼ�Գ����ռ���о�ϸ������⣬���ֱ������ȶ���������ϸ�����׶�
                if (!handHeatMap.isStationary)
                {
                    // �ȶ���UpdateHandHeatMap�ὫisStationary��Ϊtrue
                    handHeatMap.UpdateHandHeatMap(rightHand.transform.position);
                    //handHeatMap.UpdateHandHeatMap(selectObject.transform.position);
                }
                else
                {
                    manipulateStatus = ManipulateStatus.MORE_FINE_MANI;

                    Vector3[] positions = handHeatMap.latestHandVoxelQueue.Select(go => go.transform.position).ToArray();
                    
                    //Debug.Log("�ֱ��ȶȶ���Ԫ��������" + handHeatMap.latestHandVoxelQueue.Count);
                    //Debug.Log("�ֱ��ȶȶ���ת����Ԫ��������"+positions.Length);
                    
                    List<Vector3> positionList = new List<Vector3>();
                    // �Ե����һ�μ򵥹��ˣ�������̫Զ�ĵ�����
                    foreach(Vector3 pos in positions)
                    {
                        float dis = Vector3.Distance(pos, rightHand.transform.position);
                        if (dis< userArmLength)
                        {
                            positionList.Add(pos);
                        }
                        Debug.Log("�����ֱ�����:"+dis);
                    }
                    //Debug.Log("һ������:"+(handHeatMap.handVoxelCount-positionList.Count)+"����.");

                    // Ӧ����С���˷�������������ֱ���켣���������뾶����Ϊ�µĳ����ռ�ߴ�
                    Vector3 handSphereCenter;
                    float handSphereRadius;
                    SphereFitter.Fit(positionList, out handSphereCenter, out handSphereRadius);
                    Debug.Log("׼������MORE_FINE�׶Σ���ϵõ�������뾶Ϊ��" + handSphereRadius.ToString());

                    // ֻ����ê�������ռ䣬�ֲ��ռ䲻�䣬ֱ�Ӹ��³����ߴ�
                    heightOfSM = 2 * 8 * handSphereRadius; // ���¿ռ��С0505����һ���������

                    // �����hadnleInHM������һ�ε�GetObjecPosition����
                    anchor_scene = GetSceneAnchor(selectObject.transform.position, heightOfSM, handleInHM);

                    // ���ӻ�
                    if (isOpenVisualize)
                    {
                        spaceManipulateObject.transform.position = anchor_scene;
                        spaceManipulateObject.transform.localScale = heightOfSM * Vector3.one;
                    }
                    
                }

            }

            // ������Ӿ�ϸ�Ĳ����׶�
            if (manipulateStatus.Equals(ManipulateStatus.MORE_FINE_MANI))
            {
                // ���￪����ת������Ȩ�ޣ���ת������ʱ�������ƶ�
                if (rotateObject.GetState(SteamVR_Input_Sources.RightHand)) // �����ֱ���ת
                {
                    rotateObjectInterface();

                    // ��¼��תʱ��
                    if (isSampleData)
                    {
                        usSampler.rotationTime += Time.deltaTime;
                        if (usSampler.isCoarse2FineRecorded == false)
                        {
                            usSampler.translateCoarseTime -= Time.deltaTime;
                        }
                        else
                        {
                            usSampler.translateFineTime -= Time.deltaTime;
                        }
                    }

                }
                else if (enlargeObject.GetState(SteamVR_Input_Sources.RightHand)) // �ֱ���������
                {
                    selectObject.transform.localScale += Vector3.one * scaleRate * Time.deltaTime;
                    // ��¼����ʱ��
                    if (isSampleData)
                    {
                        usSampler.scaleTime += Time.deltaTime;
                        if (usSampler.isCoarse2FineRecorded == false)
                        {
                            usSampler.translateCoarseTime -= Time.deltaTime;
                        }
                        else
                        {
                            usSampler.translateFineTime -= Time.deltaTime;
                        }
                    }
                }
                else if (shrinkObject.GetState(SteamVR_Input_Sources.RightHand))
                {
                    if (selectObject.transform.localScale.x < 0.1f) { return; }
                    selectObject.transform.localScale -= Vector3.one * scaleRate * Time.deltaTime;
                    // ��¼����ʱ��
                    if (isSampleData)
                    {
                        usSampler.scaleTime += Time.deltaTime;
                        if (usSampler.isCoarse2FineRecorded == false)
                        {
                            usSampler.translateCoarseTime -= Time.deltaTime;
                        }
                        else
                        {
                            usSampler.translateFineTime -= Time.deltaTime;
                        }
                    }
                }
                else
                {
                    // �������пռ�ӳ���Ը�������λ��
                    selectObject.transform.position = GetObjecPosition(anchor_scene, heightOfSM, localHandle.transform.localPosition, heightOfHM);
                }


                // �����ֱ������ʾ�ͷ�����
                if (selectManipulateObject.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    // Debug.Log("�ͷ����壬�������������ξ���Ϊ��"+Vector3.Distance(selectObject.transform.position,targetObject.transform.position));

                    InitialManipulateEvironment(); // ��ʼ����������

                    ghostGaze.SetManipulateStatus(manipulateStatus); // ͬ���������ӵ�״̬

                    gazeHeatMap.InitialHeatmap(); // ��ʼ���ȶ�ͼ�����ض���

                    handHeatMap.InitialHandHeatmap(); // ��ʼ�����ȶ�ͼ

                    if (isUserStudy1)
                    {
                        selectObject.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().enabled = true;// ��Ŀ���������ײ������
                    }
                    if (isUserStudy2)
                    {
                        selectObject.GetComponent<BoxCollider>().enabled = true;
                    }

                    // ʱ�����
                    if (isSampleData)
                    {
                        Debug.Log("����������ʼд�뱾�β�������");
                        //smSamplerScript.manipulateStopTime = Time.time; // ��ȡ��Ϸ��ǰʱ��
                        //smSamplerScript.CalculateManipulateError(selectObject.transform.position, targetObject.transform.position);
                        //smSamplerScript.WriteExperimentData2File();
                        //smSamplerScript.isFine2AlignRecorded = false; // ����
                        usSampler.mFinishTimePoint = Time.time;
                        usSampler.WriteManipulateResult2File(selectObject, targetObject);
                        // ��������
                        usSampler.UpdateSubMission(); // ��ʵ�����++��������ʵ�����ݹ���
                        // ��������λ�á��ǶȺͷ���
                        if (isUserStudy1)
                        {
                            usController.ResetSubMission();
                        }
                        else
                        {
                            //us2Controller.ResetSubMission(selectObject);
                        }

                    }

                    
                }
            }


            // ������һ֡�ֱ���Ϣ
            previousLeftHandRosition = leftHand.transform.rotation;
            previousRightHandRotation = rightHand.transform.rotation;
            previousRightHandPosition = rightHand.transform.position;


            UpdateLine();

            // ----��������----

            // SampleManipulateData();
        }

        /// <summary>
        /// �����ֱ����ƶ�����������꣬���ֱ�ͬ��
        /// </summary>
        void UpdateLocalHandle()
        {
            localHandle.transform.position = rightHand.transform.position;
        }

        /// <summary>
        /// ͨ�����������ֲ����ռ�ĳ����ê����VR�������ͬ��
        /// </summary>
        void GetSHSForwardByPress()
        {
            if (updateUserHead.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                // ֻͬ��y���x���ƫת��z�᲻����
                Vector3 eyeEulerAngle = new Vector3(0, mainCamera.transform.eulerAngles.y, mainCamera.transform.eulerAngles.z);

                userHead.transform.eulerAngles = eyeEulerAngle;
                // ͬ���ռ�ê��
                userHead.transform.position = mainCamera.transform.position;
                Debug.Log("�����˵��ֲ����ռ�ĳ����ê��");
            }

        }

        /// <summary>
        /// ������ȡ�����������
        /// </summary>
        void GetShoulderPosition()
        {
            if (selectUserShoulder.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                if (!isUserShoulderSampled)
                {
                    // ��ȡ��ʱ�������������겢��ֵ�����
                    userShoulder.transform.position = rightHand.transform.position;
                    isUserShoulderSampled = true;
                    Debug.Log("�ɹ���ȡ�������");

                    // ���ֲ����ռ������ʼ��
                    p_shoulder = userShoulder.transform.localPosition; 
                    p_elbow = p_shoulder + userBoomLength * Vector3.down; // ����λ��

                    // ---------���п��ӻ�------------
                    // 1.���ӻ����
                    GameObject shouderObject = Instantiate(pointPrefab, userShoulder.transform);
                    shouderObject.name = "����";
                    shouderObject.transform.localScale = 0.05f * Vector3.one;
                    // 2.���ӻ�����
                    elbowObject = Instantiate(pointPrefab, userShoulder.transform);
                    elbowObject.name = "�����";
                    elbowObject.transform.localScale = 0.05f * Vector3.one;
                    elbowObject.transform.localPosition = userBoomLength * Vector3.down;
                    if (isOpenVisualize)
                    {
                        
                        // 3.���ӻ����ֲ����ռ�
                        GameObject handSpace = Instantiate(handSpacePrefab, userShoulder.transform);
                        handSpace.name = "���ֲ����ռ�";
                        handSpace.transform.localScale = heightOfHM * Vector3.one;
                        handSpace.transform.localPosition = userBoomLength * Vector3.down;
                    }

                    
                }
            }
        }


        /// <summary>
        /// ͨ���ֱ�����ѡȡĿ������
        /// Ŀǰʹ�����߻�ȡ:GetObjectByRaycast()
        /// </summary>
        void GetManipulateObject()
        {
            if (manipulateStatus.Equals(ManipulateStatus.UNSELECTED))
            {
                if (selectManipulateObject.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    Debug.Log("�����˻�ȡ����İ���...");
                    GetObjectByHandleRay();
                }
            }
        }

        

        /// <summary>
        /// ��Ҫ��GetManipulateObject����
        /// �㷨1��ͨ�����߻�ȡ��Ҫ����������
        /// </summary>
        /// <param name="handPosition"></param>
        /// <param name="handForward"></param>
        void GetObjectByRaycast(Vector3 handPosition, Vector3 handForward)
        {
            Ray handRay = new Ray(handPosition, handForward);
            RaycastHit hitInfo;
            if (Physics.Raycast(handRay, out hitInfo, Mathf.Infinity, (1 << 10)))
            {
                selectObject = hitInfo.collider.gameObject;
                while (selectObject.transform.parent != null)
                {
                    selectObject = selectObject.transform.parent.gameObject;
                }

                // �ı����״̬
                manipulateStatus = ManipulateStatus.SELECTED;
                
                // �ֱ�-�������߿��ӻ�����
                SetHandleRay(false);
                lineRenderer.enabled = true;

                // ����GhostGaze�еĲ�������
                ghostGaze.SetTargetObject(selectObject);
                ghostGaze.SetManipulateStatus(manipulateStatus);

                // �������Ŀ��ӻ�
                gazeHeatMap.staringCenterObject.SetActive(true);

                // �ر�������ײ��
                selectObject.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().enabled = false;


                // ʱ�����
                if (isSampleData)
                {
                    //smSamplerScript.manipulateStartTime = Time.time; // ��ȡ��Ϸ��ǰʱ��
                    
                }

                Debug.Log("��ȡĿ������:" + selectObject.name);
            }
            else
            {
                // ��ѡ����Ϊ������ǰ����
                Debug.Log("��Ŀ���������ų��Һ���û�꣡");
                if (manipulateStatus.Equals(ManipulateStatus.SELECTED))
                {
                    Debug.Log("�ͷ����壬��������");

                    InitialManipulateEvironment(); // ��ʼ����������

                    ghostGaze.SetManipulateStatus(manipulateStatus); // ͬ���������ӵ�״̬

                    gazeHeatMap.InitialHeatmap(); // ��ʼ���ȶ�ͼ�����ض���

                    selectObject.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().enabled = true; // ��Ŀ���������ײ������


                    // ʱ�����
                    //if (isSampleData)
                    //{
                    //    Debug.Log("��ʼд�뱾�β�������");
                    //    smSamplerScript.manipulateStopTime = Time.time; // ��ȡ��Ϸ��ǰʱ��
                    //    smSamplerScript.CalculateManipulateError(selectObject.transform.position, targetObject.transform.position);
                    //    smSamplerScript.WriteExperimentData2File();
                    //}

                }
                
            }
        }


        /// <summary>
        /// 3.23 new
        /// �ֱ��������߳��Ի�ȡ����
        /// </summary>
        void GetObjectByHandleRay()
        {
            Ray handRay = new Ray(rightHand.transform.position, rightHand.transform.forward);
            RaycastHit hitInfo;
            if (Physics.Raycast(handRay, out hitInfo, Mathf.Infinity, (1 << 10)))
            {
                selectObject = hitInfo.collider.gameObject;
                while (selectObject.transform.parent != null && selectObject.transform.parent.gameObject.layer == 10)
                {
                    selectObject = selectObject.transform.parent.gameObject;
                }

                // �ı����״̬
                manipulateStatus = ManipulateStatus.SELECTED;

                if (isOpenVisualize)
                {
                    spaceManipulateObject.SetActive(true);
                    spaceManipulateObject.transform.localScale = heightOfSM * Vector3.one;

                    // �������Ŀ��ӻ�
                    gazeHeatMap.staringCenterObject.SetActive(true);
                }

                // �ֱ�-�������߿��ӻ�����
                SetHandleRay(false);
                lineRenderer.enabled = true;
                // ����GhostGaze�еĲ�������
                ghostGaze.SetTargetObject(selectObject);
                ghostGaze.SetManipulateStatus(manipulateStatus);
                
                // �ر�������ײ��
                if (isUserStudy1)
                {
                    selectObject.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().enabled = false;
                }
                if (isUserStudy2)
                {
                    selectObject.GetComponent<BoxCollider>().enabled = false;
                }

                // ʱ�����
                if (isSampleData)
                {
                    //smSamplerScript.manipulateStartTime = Time.time; // ��ȡ��Ϸ��ǰʱ��
                    //Debug.Log("��ʼ����ʱ�䣺" + smSamplerScript.manipulateStartTime.ToString());
                    //smSamplerScript.manipulateDistance = Vector3.Distance(selectObject.transform.position, targetObject.transform.position); // ���������յ�ľ���
                    usSampler.mStartTimePoint = Time.time;
                    Debug.Log("��ʼ����ʱ�䣺" + usSampler.mStartTimePoint.ToString());
                    if (isUserStudy2)
                    {
                        targetObject = us2Controller.targetList[us2Controller.curMission];
                    }
                }
                Debug.Log("��ȡĿ������:" + selectObject.name);
            }
        }

        /// <summary>
        /// �����������ݣ��ֱ������塢���ӵ�
        /// </summary>
        void SampleManipulateData()
        {
            if (!isSampleData) { return; }
            if (manipulateStatus.Equals(ManipulateStatus.UNSELECTED)) { return; }
            // �������ӵ� 
            Vector3 gazeDirect = sRanipalGazeSample.GazeDirectionCombined;
            Vector3 eyePosition = Camera.main.transform.position;
            Ray gazeRay = new Ray(eyePosition, gazeDirect);
            RaycastHit hitInfo;
            if (Physics.Raycast(gazeRay, out hitInfo))
            {
                //smSamplerScript.SampleEyeGazePoint(hitInfo.point);
            }

            // �����ֱ�
            //smSamplerScript.SampleHandlePoint(rightHand.transform.position);
            // ���������ƶ�
            //smSamplerScript.SampleObjectPoint(selectObject.transform.position);
        }



        /// <summary>
        /// ��ȡ���������ռ����ê��
        /// </summary>
        /// <param name="p_obj_old"></param>
        /// <param name="handle_in_hm"></param>
        /// <returns></returns>
        Vector3 GetSceneAnchor(Vector3 p_obj_old,float depth,PointInSpace handle_in_hm)
        {
            float z_offset = depth * handle_in_hm.rate_z;
            float r_xy = Mathf.Sqrt(Mathf.Pow(depth, 2) - Mathf.Pow((depth - z_offset), 2));
            float y_offset = handle_in_hm.is_y_positive ? -r_xy * handle_in_hm.rate_y : r_xy * handle_in_hm.rate_y;
            float x_offset = handle_in_hm.is_x_posiyive ? -r_xy * handle_in_hm.rate_x : r_xy * handle_in_hm.rate_x;
            Vector3 p_scene_new = p_obj_old - z_offset * cameraInfo[1] + x_offset * cameraInfo[2] + y_offset * cameraInfo[3];
            return p_scene_new;
        }
        
        PointInSpace GetAxisRateByPosition(Vector3 p_h,float r_arm)
        {
            PointInSpace res;

            float z_distance = p_h.z - p_elbow.z;
            float z_abs_dis;

            if (z_distance > r_arm)
            {
                z_abs_dis = r_arm;
            }
            else if (z_distance > 0)
            {
                z_abs_dis = z_distance;
            }
            else
            {
                z_abs_dis = 0f;
            }
            res.rate_z = z_abs_dis / r_arm;

            float r_xy_2 = Mathf.Pow(r_arm, 2) - Mathf.Pow(r_arm - z_abs_dis, 2);
            if (r_xy_2 < 0) { Debug.LogError("ӳ��Բ��뾶����С��0,���������ʼ��.."); }
            float r_xy = Mathf.Sqrt(r_xy_2); // ӳ��Բ��뾶

            float x_abs_dis = Mathf.Abs(p_h.x - p_elbow.x);
            if (x_abs_dis > r_xy)
            {
                res.rate_x = 1;
            }
            else
            {
                res.rate_x = x_abs_dis / r_xy;
            }

            
            res.is_x_posiyive = p_h.x > p_elbow.x ? true : false;

            float y_abs_dis = Mathf.Abs(p_h.y - p_elbow.y);

            if (y_abs_dis > r_xy)
            {
                res.rate_y = 1;
            }
            else
            {
                res.rate_y = y_abs_dis / r_xy;
            }

            res.is_y_positive = p_h.y > p_elbow.y ? true : false;

            return res;

        }

        /// <summary>
        /// ��ȡ����������λ�ã����������ռ�Ϊ��������
        /// </summary>
        /// <param name="p_scene"></param>
        /// <param name="depth"></param>
        /// <param name="p_h"></param>
        /// <param name="r_arm"></param>
        /// <returns></returns>
        Vector3 GetObjecPosition(Vector3 p_scene,float depth, Vector3 p_h, float r_arm)
        {
            handleInHM = GetAxisRateByPosition(p_h, r_arm);

            float z_offset = depth * handleInHM.rate_z;
            float r_xy_2 = Mathf.Pow(depth, 2) - Mathf.Pow((depth - z_offset), 2);
            if (r_xy_2 < 0f) { Debug.LogError("����xyԲƽ��뾶ʱ����:"+depth.ToString()+","+z_offset.ToString()); }
            float r_xy = Mathf.Sqrt(r_xy_2);
            float y_offset = handleInHM.is_y_positive ? r_xy * handleInHM.rate_y : -r_xy * handleInHM.rate_y;
            float x_offset = handleInHM.is_x_posiyive ? r_xy * handleInHM.rate_x : -r_xy * handleInHM.rate_x;

            Vector3 p_obj = p_scene+z_offset*cameraInfo[1]+ x_offset * cameraInfo[2] + y_offset * cameraInfo[3];
            return p_obj;
        }

        /// <summary>
        /// ��ʼ��������������ÿ�β�����ʼǰ����
        /// </summary>
        void InitialManipulateEvironment()
        {
            // ���������ռ�
            cameraInfo = new List<Vector3>(new Vector3[4]);// �û��ӿ���Ϣ
            heightOfSM = heightOfFrustum;

            // ����״̬
            manipulateStatus = ManipulateStatus.UNSELECTED; // ����״̬

            // ���ӻ�
            if (isOpenVisualize)
            {
                spaceManipulateObject.SetActive(false); // �رտ��ӻ����������ռ�
                gazeHeatMap.staringCenterObject.SetActive(false); // �ر����ӵ����Ķ���
            }
            lineRenderer.enabled = false; // �ر��ֱ������������
            SetHandleRay(true); // ԭ���ֱ�����
        }

        private void UpdateObjectByHand()
        {
            if (rotateObject.GetState(SteamVR_Input_Sources.RightHand))
            {
                // �����ֱ���ת
                Quaternion rotationDifference = rightHand.transform.rotation * Quaternion.Inverse(previousLeftHandRosition);
                selectObject.transform.rotation *= rotationDifference;
            }

            // �ֱ���������
            if (enlargeObject.GetState(SteamVR_Input_Sources.LeftHand))
            {
                Debug.Log("�Ŵ�");
                selectObject.transform.localScale += Vector3.one * scaleRate * Time.deltaTime;
            }
            if (shrinkObject.GetState(SteamVR_Input_Sources.LeftHand))
            {
                Debug.Log("��С");
                if (selectObject.transform.localScale.x < 0.1f) { return; }
                selectObject.transform.localScale -= Vector3.one * scaleRate * Time.deltaTime;
            }

        }

        private void rotateObjectInterface()
        {
            if (isUserStudy1)
            {
                rotateObjectXYZ();
            }
            if (isUserStudy2)
            {
                //rotateObjectXYZ();
                rotateObjectY();
            }
        }

        // ������ת����
        private void rotateObjectXYZ()
        {
            // Quaternion rotationDifference = leftHand.transform.rotation * Quaternion.Inverse(previousLeftHandRosition);
            Quaternion rotationDifference = rightHand.transform.rotation * Quaternion.Inverse(previousRightHandRotation);
            selectObject.transform.rotation *= rotationDifference;
        }

        private void rotateObjectY()
        {
            float selectObjectx = selectObject.transform.eulerAngles.x;
            float selectObjectz = selectObject.transform.eulerAngles.z;

            float deltaY = rightHand.transform.rotation.eulerAngles.y - previousRightHandRotation.eulerAngles.y;

            float selectObjecty = selectObject.transform.eulerAngles.y + deltaY;
            selectObject.transform.eulerAngles = new Vector3(selectObjectx, selectObjecty, selectObjectz);
        }


        // ���ӻ�����
        void SetHandleRay(bool state)
        {
            Transform rayObject = rightHand.transform.GetChild(1);
            rayObject.gameObject.SetActive(state);
        }

        void UpdateLine()
        {
            if (!manipulateStatus.Equals(ManipulateStatus.UNSELECTED))
            {
                lineRenderer.SetPosition(0, rightHand.transform.position);
                lineRenderer.SetPosition(1, selectObject.transform.position);
            }
            
        }

        void UpdateStaringCenter(Vector3 pos)
        {
            gazeHeatMap.staringCenterObject.transform.position = pos;
        }



        //-----------��ʱ����---------------

        /// <summary>
        /// �����ֱ��ֲ����������������ڳ��������ռ��е�λ��
        /// ���ֲ����ռ�Ϊ����
        /// </summary>
        /// <param name="p_h">�ֱ�����</param>
        /// <returns>�������������</returns>
        Vector3 GetSMSObjectPosition(Vector3 p_h)
        {
            float d_np = mainCamera.nearClipPlane; // distance_near_plane
            float d_fp = mainCamera.farClipPlane; // distance_far_plane
            d_fp = 7f; // ����ʱ�涨
            float d_fn = d_fp - d_np; // ƽ��ͷ��ĸ�
            float l_arm = userArmLength;
            Vector3 p_s = userShoulder.transform.localPosition;

            Vector3 p_c = cameraInfo[0]; // ���λ��

            //Debug.Log("p_s:" + p_s.ToString());
            //Debug.Log("p_h:" + p_h.ToString());

            // ����zo
            float z_rate = Mathf.Abs(p_h.z - p_s.z) / l_arm;
            float z_o; // ������֮����Ҫ���Է�������
            if (p_h.z > p_s.z)
            {
                z_o = d_np + d_fn * z_rate;
            }
            else
            {
                z_o = d_np - d_fn * z_rate;
            }

            // ����z_o������Ļ����
            float height_o = z_o * Mathf.Tan(halfFov);
            float width_o = height_o * aspectFov;


            // ����xo
            float l_yx = Mathf.Sqrt(Mathf.Pow(l_arm, 2) - Mathf.Pow(p_h.y - p_s.y, 2) - Mathf.Pow(p_h.z - p_s.z, 2));
            //Debug.Log("l_yx:" + l_yx.ToString());
            float rate_h_x = Mathf.Abs(p_h.x - p_s.x) / l_yx;
            //Debug.Log("rate_h_x:" + rate_h_x.ToString());
            float x_o;
            if (p_h.x > p_s.x)
            {
                x_o = (width_o / 2f) * rate_h_x;
            }
            else
            {
                x_o = -(width_o / 2f) * rate_h_x;
            }

            // ����yo
            float l_xz = Mathf.Sqrt(Mathf.Pow(l_arm, 2) - Mathf.Pow(p_h.y - p_s.y, 2) - Mathf.Pow(p_h.x - p_s.x, 2));
            float rate_h_y = Mathf.Abs(p_h.y - p_s.y) / l_xz;
            float y_o;

            if (p_h.y > p_s.y)
            {
                y_o = (height_o / 2f) * rate_h_y;
            }
            else
            {
                y_o = -(height_o / 2f) * rate_h_y;
            }

            // ����p_o
            Vector3 p_o = p_c + cameraInfo[1] * z_o + cameraInfo[2] * x_o + cameraInfo[3] * y_o;
            return p_o;

        }

        /// <summary>
        /// �����ֱ��ֲ���������������λ�ã����ֲ����ռ�Ϊ��������
        /// </summary>
        /// <param name="p_h"></param>
        /// <returns></returns>
        Vector3 GetSMSObjectPositionHemi(Vector3 p_h)
        {
            float d_np = mainCamera.nearClipPlane; // distance_near_plane
            float d_fp = mainCamera.farClipPlane; // distance_far_plane
            d_fp = farPlaneValue; // ����ʱ�涨
            float d_fn = d_fp - d_np; // ƽ��ͷ��ĸ�
            float r_arm = heightOfHM; // �ʵ��Ŵ�userArmLength

            Vector3 p_shoulder = userShoulder.transform.localPosition; // ���ֲ�����
            Vector3 p_elbow = p_shoulder - userBoomLength * Vector3.down; // ��������=�������-��۳���*(0,-1,0)
            Vector3 p_camera = cameraInfo[0]; // ���λ��

            // ����zo

            float z_distance = p_h.z - p_elbow.z;
            float z_abs_dis;
            if (z_distance > r_arm)
            {
                z_abs_dis = r_arm;
            }
            else if (z_distance > 0)
            {
                z_abs_dis = z_distance;
            }
            else
            {
                z_abs_dis = 0;
            }

            float z_rate = z_distance / r_arm;

            float z_object = d_np + d_fn * z_rate;

            // ����z_object������Ļ����
            float height_object = z_object * Mathf.Tan(halfFov);
            float width_object = height_object * aspectFov;



            float r_xy_2 = Mathf.Pow(r_arm, 2) - Mathf.Pow(r_arm - z_abs_dis, 2);
            if (r_xy_2 < 0) { Debug.LogError("ӳ��Բ��뾶����С��0,���������ʼ��.."); }
            float r_xy = Mathf.Sqrt(r_xy_2); // ӳ��Բ��뾶

            // ����x_object
            float x_abs_dis = Mathf.Abs(p_h.x - p_elbow.x);
            float x_rate;
            if (x_abs_dis > r_xy)
            {
                x_rate = 1;
            }
            else
            {
                x_rate = x_abs_dis / r_xy;
            }

            float x_object;
            if (p_h.x > p_elbow.x)
            {
                x_object = (width_object / 2f) * x_rate;
            }
            else
            {
                x_object = -(width_object / 2f) * x_rate;
            }

            // ����y_object

            float y_abs_dis = Mathf.Abs(p_h.y - p_elbow.y);
            float y_rate;

            if (y_abs_dis > r_xy)
            {
                y_rate = 1;
            }
            else
            {
                y_rate = y_abs_dis / r_xy;
            }

            float y_object;
            if (p_h.y > p_elbow.y)
            {
                y_object = (height_object / 2f) * y_rate;
            }
            else
            {
                y_object = -(height_object / 2f) * y_rate;
            }

            // ����p_o
            Vector3 p_object = p_camera + cameraInfo[1] * z_object + cameraInfo[2] * x_object + cameraInfo[3] * y_object;
            return p_object;
        }



    }

}
