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
    /// UNSELECT:未选择物体
    /// SELECTED：选中物体，只允许头动操作
    /// FAST_MANI：快速操作
    /// CHECK_HANDLE: 检查手柄位置
    /// FINE_MANI：精细操作
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
        [Header("手柄信息")]
        [Tooltip("CameraRig中的右手手柄对象")]
        public GameObject rightHand;
        public GameObject leftHand;
        public SteamVR_Action_Boolean updateUserHead; // 更新单手操作空间朝向
        public SteamVR_Action_Boolean selectUserShoulder; // 选择用户肩膀位置
        private bool isUserShoulderSampled;
        public SteamVR_Action_Boolean selectManipulateObject; // 选择需要操作的物体
        // 旋转和缩放物体
        public SteamVR_Action_Boolean rotateObject; // 旋转物体
        public SteamVR_Action_Boolean enlargeObject; // 放大物体
        public SteamVR_Action_Boolean shrinkObject; // 缩小物体

        [Header("用户信息")]
        [Tooltip("用户手臂总长度")]
        public float userArmLength; // 用户手臂长度，需要输入
        [Tooltip("用户大臂长度")]
        public float userBoomLength; // 用户大臂长度，用于计算手肘位置
        
        [Header("单手交互空间")]
        [Tooltip("单手交互空间的放缩比例")]
        public float userForearmScale = 1.2f; // 用户空间尺寸的缩放比例
        private float heightOfHM; // 单手操作空间的高（固定）
        private Vector3 p_shoulder; // 肩膀坐标（局部）
        private Vector3 p_elbow; // 手肘坐标（局部）

        // 进阶补充：舒适交互空间划分
        private float comfort_level_1; // 舒适最小半径
        private float comfort_level_2; // 舒适半径，等于小臂长度
        private float comfort_level_3; // 舒适最大半径，再大就是疲劳

        private GameObject userHead; // 用户头部
        private GameObject userShoulder; // 用户肩膀对象，需要拖入
        private GameObject localHandle; // 父物体为Head的手柄对象复制

        [Header("场景交互空间")]
        [Tooltip("场景交互空间的远平面距离")]
        public float farPlaneValue; // 需要手动设置
        private float nearPlaneValue; // 从相机获取
        private Camera mainCamera; // 主相机对象
        private float halfFov; // 垂直FOV一半，弧度
        private float aspectFov; // 视锥体的宽高比
       
        private float heightOfFrustum; // 初始平截头体的高
        private float heightOfSM; // 场景操作空间的高（会更新）
        private Vector3 anchor_scene; // 场景操作空间锚点，初始为主相机正前方近平面处

        private List<Vector3> cameraInfo; // 视口信息：坐标、前方、上方、右方

        [Header("操作参数与脚本")]
        [Tooltip("放缩操作的放缩比例")]
        public float scaleRate = 0.5f;
        [Tooltip("粗粒度-细粒度阶段物体与视点最大距离")]
        public float maxDistanceGO = 0.4f;

        [HideInInspector] public GameObject selectObject; // 被选择用于操作的物体
        private ManipulateStatus manipulateStatus; // 当前操作阶段 
        private Quaternion previousLeftHandRosition; // 上一帧旋转角度（左手）
        private Quaternion previousRightHandRotation; // 上一帧旋转角度（右手）
        private GazeHeatMap gazeHeatMap; // 凝视热度图   
        private GhostGaze ghostGaze; // 凝视算法脚本
        private HandHeatMap handHeatMap; // 手热度图

        // 凝视相关脚本    
        public bool isOpenEyeTrack = false;
        private GameObject GazeRaySample; // 眼部跟踪对象
        private SRanipal_GazeRaySample_v2 sRanipalGazeSample; // 来自GazeRaySample

        // -----运行时需要维护的变量-----
        private PointInSpace handleInHM; // 手柄在单手操作空间中的位置
        private Vector3 previousRightHandPosition; // 上一帧手柄位置，用于计算手柄移动总距离

        [Header("可视化信息")]
        public bool isOpenVisualize = false;
        public GameObject sceneSpacePrefab; // 场景交互空间
        public GameObject handSpacePrefab; // 单手交互空间
        public GameObject pointPrefab; // 用于可视化各种锚点
        public GameObject LineRenderController; // 可视化连线

        private GameObject elbowObject;

        private GameObject spaceManipulateObject; // 可视化的场景操作空间，来自spacePrefab
        private LineRenderer lineRenderer; // 

        // 采样相关变量
        [Header("采样信息")]
        [Tooltip("目标位置处的示意物体")]
        public GameObject targetObject; // 用于计算误差的目标物体
        [Tooltip("是否启动采样")]
        public bool isSampleData = false;
        // private SMSampler smSamplerScript; // 采样脚本
        public GameObject UserStudyManager;
        private UserStudySampler usSampler; // 新采样脚本
        private UserStudyController usController; // 用户实验控制脚本
        private UserStudy2Controller us2Controller; // 用户实验控制脚本2

        public bool isUserStudy1 = true;
        public bool isUserStudy2 = false;



        /// <summary>
        /// 操作流程
        /// 1.手柄按键更新头部朝向（肩膀为其子物体）
        /// 1.手柄按键获取肩膀位置
        /// 3.手柄按键更新视口朝向
        /// 4.手柄按键选取目标物体
        /// </summary>

        void Start()
        {
            // 【用户相关对象】
            userHead = GameObject.Find("Head");
            userShoulder = GameObject.Find("Shoulder");
            localHandle = GameObject.Find("HandleRight");
            // 【眼动对象】
            if (isOpenEyeTrack)
            {
                GazeRaySample = GameObject.Find("Gaze Ray Sample v2");
                sRanipalGazeSample = GazeRaySample.GetComponent<SRanipal_GazeRaySample_v2>(); // 初始化凝视脚本
            }
            // 【主相机】
            mainCamera = Camera.main;
            halfFov = (mainCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad;
            aspectFov = mainCamera.aspect;
            nearPlaneValue = mainCamera.nearClipPlane; // 相机近平面
            cameraInfo = new List<Vector3>(new Vector3[4]); // 在初定位阶段会动态更新
            // 【场景交互空间】
            heightOfFrustum = farPlaneValue-nearPlaneValue; // 场景操作空间实际尺寸（高）
            heightOfSM = heightOfFrustum; // 初始为平截头体的高
            // 【单手交互空间】
            heightOfHM =(userArmLength - userBoomLength) * userForearmScale;
            // 初始化舒适区域半径
            comfort_level_1 = (userArmLength - userBoomLength) * 0.8f; // 内圈
            comfort_level_2 = (userArmLength - userBoomLength) * 1f; // 内圈
            comfort_level_3 = (userArmLength - userBoomLength) * 1.2f; //外圈
            //Debug.Log("一级舒适区域半径：" + comfort_level_1.ToString());
            // 【操作参数】
            manipulateStatus = ManipulateStatus.UNSELECTED;
            isUserShoulderSampled = false;

            gazeHeatMap = GetComponent<GazeHeatMap>();// 获取凝视热度图脚本对象
            handHeatMap = GetComponent<HandHeatMap>();// 手热度图脚本
            ghostGaze = GetComponent<GhostGaze>();// 获取幽灵凝视脚本对象

            // 【采样脚本】

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

            // 【可视化】
            if (isOpenVisualize)
            {
                spaceManipulateObject = Instantiate(sceneSpacePrefab); // 场景交互空间可视化
                spaceManipulateObject.SetActive(false);
                spaceManipulateObject.name = "场景操作空间";
            }

            lineRenderer = LineRenderController.GetComponent<LineRenderer>(); // 物体-手柄连线可视化
            lineRenderer.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            // 更新手柄复制体的坐标，与手柄本体同步
            UpdateLocalHandle();

            // ----准备工作----

            // 按键确定单手操作空间(Head)朝向和锚点位置
            GetSHSForwardByPress();
            // 按键确定肩膀位置Head->Shoulder
            GetShoulderPosition();

            // ----开始操作----
            // 未选择任何物体，按键获取操作对象

            // 1.检测到按下扳机键&系统状态为未选择，则尝试获取物体
            if (manipulateStatus.Equals(ManipulateStatus.UNSELECTED)){
                if (selectManipulateObject.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    Debug.Log("未选择物体，尝试选择物体..");
                    GetObjectByHandleRay();
                }
            }
            // 2.若系统状态不是未选择，则获取凝视点，并且开放旋转和缩放权限
            Vector3 p_gaze = new Vector3(); // 凝视点坐标
            if (!manipulateStatus.Equals(ManipulateStatus.UNSELECTED))
            {               
                ReturnStatus get_staring_point = ghostGaze.GetStaringPoint(ref p_gaze); // 获取凝视点坐标
                if (get_staring_point.Equals(ReturnStatus.WAR_GAZE_NOT_HIT))
                {
                    Debug.LogWarning("WAR: Gaze not hit");
                }

                // 这里记录手柄移动总距离和旋转总角度
                if (isSampleData)
                {
                    usSampler.translatePathLength += Vector3.Distance(rightHand.transform.position, previousRightHandPosition);
                    usSampler.rotationTotalAngle += Quaternion.Angle(leftHand.transform.rotation, previousLeftHandRosition);

                    // 判断是否进入细粒度操作阈值
                    if (usSampler.isCoarse2FineRecorded == false && 
                        Vector3.Distance(targetObject.transform.position, selectObject.transform.position) < usSampler.coarse2FineThreshold){
                        usSampler.isCoarse2FineRecorded = true;
                        usSampler.coarse2FineTimePoint = Time.time;
                        Debug.Log("[采样记录]进入精细操作阶段");
                    }  
                }

            }
            // 3.系统进入选择状态
            if (manipulateStatus.Equals(ManipulateStatus.SELECTED))
            {
                // 3.1. 若按住扳机键，则开放视口旋转
                if (selectManipulateObject.GetState(SteamVR_Input_Sources.RightHand))
                {
                    cameraInfo[0] = mainCamera.transform.position;
                    cameraInfo[1] = mainCamera.transform.forward;
                    cameraInfo[2] = mainCamera.transform.right;
                    cameraInfo[3] = mainCamera.transform.up;
                    // Debug.Log("初步选择阶段，更新相机：" + mainCamera.transform.forward.ToString());

                    anchor_scene = cameraInfo[0] + nearPlaneValue * cameraInfo[1]; // 更新场景锚点

                    if (isOpenVisualize)
                    {
                        spaceManipulateObject.transform.position = anchor_scene; // 更新场景可视化
                        spaceManipulateObject.transform.forward = cameraInfo[1];
                    }
                    

                    selectObject.transform.position = GetObjecPosition(anchor_scene, heightOfSM, localHandle.transform.localPosition, heightOfHM); // 更新物体坐标
                }

                // 3.2.若松开扳机键且当前状态为已选择，则切换模式到快速操作
                if (selectManipulateObject.GetStateUp(SteamVR_Input_Sources.RightHand))
                {
                    Debug.Log("松开了扳机,进入快速操作阶段");
                    manipulateStatus = ManipulateStatus.FAST_MANI;
                }
            }

            // 4.快速操作阶段，更新操作对象位置，检测操作状态
            if (manipulateStatus.Equals(ManipulateStatus.FAST_MANI))
            {
                // 更新凝视热度图
                ReturnStatus update_heat_map = gazeHeatMap.UpdateHeatMap(p_gaze);
                if (update_heat_map.Equals(ReturnStatus.WAR_GAZE_CROSS_BORDER))
                {
                    Debug.LogWarning("WAR: Gaze point cross border");
                }

                // 计算物体中心与目标中心的距离distanceGazeObjet
                float distanceGazeObjet = Vector3.Distance(p_gaze, selectObject.transform.position);
                // Debug.Log("此时凝视点与物体的距离:" + distanceGazeObjet.ToString());

                // 检测用户是否意图进入精细操作模式
                if (gazeHeatMap.isStationary == true && distanceGazeObjet < maxDistanceGO)
                {
                    Debug.Log("离开快速操作，进入精细操作。");
                    heightOfSM = 1.5f * heightOfHM; // 更新场景空间大小为单手空间:目前是2倍

                    // 更新场景操作空间的锚点位置和大小
                    anchor_scene = GetSceneAnchor(selectObject.transform.position, heightOfSM, handleInHM);

                    // 改变操作状态：进入手柄检查阶段
                    // 1.检查此时手柄与手肘锚点的距离，若大于舒适半径则进入手柄重置状态
                    // 2.否则直接进入精细操作阶段
                    float d_handle = Vector3.Distance(localHandle.transform.localPosition, p_elbow);
                    if(d_handle> comfort_level_3 || d_handle<comfort_level_1)
                    {
                        Debug.Log("手柄位置需要重置："+d_handle.ToString());
                        manipulateStatus = ManipulateStatus.CHECK_HANDLE;
                        // 记录一下，发生了同步
                        if (isSampleData)
                        {
                            usSampler.isHandleReset = 1;
                        }
                    }
                    else
                    {
                        Debug.Log("手柄位置在舒适区域");
                        manipulateStatus = ManipulateStatus.FINE_MANI;
                        if (isSampleData)
                        {
                            usSampler.isHandleReset = 0;
                        }
                    }

                    // ---- 可视化更新 ----
                    if (isOpenVisualize)
                    {
                        spaceManipulateObject.transform.position = anchor_scene;
                        spaceManipulateObject.transform.localScale = heightOfSM * Vector3.one;
                        gazeHeatMap.staringCenterObject.SetActive(false);
                    }
                    
                }

                // 更新操作对象位置
                selectObject.transform.position = GetObjecPosition(anchor_scene, heightOfSM, localHandle.transform.localPosition, heightOfHM);

            }

            // 5.手柄重置阶段（非必须）
            if (manipulateStatus.Equals(ManipulateStatus.CHECK_HANDLE))
            {
                float d_handle = Vector3.Distance(localHandle.transform.localPosition ,p_elbow);

                // Vector3 v_elbow2handle = localHandle.transform.localPosition - p_elbow;
                Vector3 v_elbow2handle = rightHand.transform.position - elbowObject.transform.position;
                
                float d_angle = Vector3.Angle(v_elbow2handle, userHead.transform.forward) ; // TODO: 未测试手-手肘连线与单手空间正前方的夹角
                //Debug.DrawRay(elbowObject.transform.position, v_elbow2handle);
                //Debug.DrawRay(elbowObject.transform.position, userHead.transform.forward);
                // TODO：这里可以给手柄增加可视化，在不同的舒适区域手柄的颜色不同
                Debug.Log("处于手柄重置状态,当前距离为:"+d_handle.ToString()+",夹角为:"+d_angle.ToString());
                if (d_handle < comfort_level_2 && d_handle > comfort_level_1 && d_angle < 20f)
                {
                    Debug.Log("重置了手柄位置，映射同步重置，当前角度："+d_angle.ToString());
                    handleInHM = GetAxisRateByPosition(localHandle.transform.localPosition, heightOfHM); // 重新计算手柄位置
                    anchor_scene = GetSceneAnchor(selectObject.transform.position, heightOfSM, handleInHM); // 计算新的场景锚点
                    manipulateStatus = ManipulateStatus.FINE_MANI;

                    // 可视化
                    if (isOpenVisualize)
                    {
                        spaceManipulateObject.transform.position = anchor_scene;
                    }
                    
                }
            }

            // 6.精细操作阶段:正常操作，检测用户操作意图以进入二级精细操作
            if (manipulateStatus.Equals(ManipulateStatus.FINE_MANI))
            {
                Debug.Log("处于一级精细操作阶段");
                // 正常进行空间映射以更新物体位置
                selectObject.transform.position = GetObjecPosition(anchor_scene, heightOfSM, localHandle.transform.localPosition, heightOfHM);

                // 开始对场景空间进行精细操作检测，当手柄操作稳定后进入更精细操作阶段
                if (!handHeatMap.isStationary)
                {
                    // 稳定后，UpdateHandHeatMap会将isStationary置为true
                    handHeatMap.UpdateHandHeatMap(rightHand.transform.position);
                    //handHeatMap.UpdateHandHeatMap(selectObject.transform.position);
                }
                else
                {
                    manipulateStatus = ManipulateStatus.MORE_FINE_MANI;

                    Vector3[] positions = handHeatMap.latestHandVoxelQueue.Select(go => go.transform.position).ToArray();
                    
                    //Debug.Log("手柄热度队列元素数量：" + handHeatMap.latestHandVoxelQueue.Count);
                    //Debug.Log("手柄热度队列转数组元素数量："+positions.Length);
                    
                    List<Vector3> positionList = new List<Vector3>();
                    // 对点进行一次简单过滤，将距离太远的点抛弃
                    foreach(Vector3 pos in positions)
                    {
                        float dis = Vector3.Distance(pos, rightHand.transform.position);
                        if (dis< userArmLength)
                        {
                            positionList.Add(pos);
                        }
                        Debug.Log("点与手柄距离:"+dis);
                    }
                    //Debug.Log("一共抛弃:"+(handHeatMap.handVoxelCount-positionList.Count)+"个点.");

                    // 应用最小二乘法，用球体拟合手柄活动轨迹，计算出活动半径，作为新的场景空间尺寸
                    Vector3 handSphereCenter;
                    float handSphereRadius;
                    SphereFitter.Fit(positionList, out handSphereCenter, out handSphereRadius);
                    Debug.Log("准备进入MORE_FINE阶段，拟合得到的球体半径为：" + handSphereRadius.ToString());

                    // 只重新锚定场景空间，手部空间不变，直接更新场景尺寸
                    heightOfSM = 2 * 8 * handSphereRadius; // 更新空间大小0505方大一倍方便操作

                    // 这里的hadnleInHM来自上一次的GetObjecPosition计算
                    anchor_scene = GetSceneAnchor(selectObject.transform.position, heightOfSM, handleInHM);

                    // 可视化
                    if (isOpenVisualize)
                    {
                        spaceManipulateObject.transform.position = anchor_scene;
                        spaceManipulateObject.transform.localScale = heightOfSM * Vector3.one;
                    }
                    
                }

            }

            // 进入更加精细的操作阶段
            if (manipulateStatus.Equals(ManipulateStatus.MORE_FINE_MANI))
            {
                // 这里开放旋转和缩放权限，旋转和缩放时不允许移动
                if (rotateObject.GetState(SteamVR_Input_Sources.RightHand)) // 跟随手柄旋转
                {
                    rotateObjectInterface();

                    // 记录旋转时间
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
                else if (enlargeObject.GetState(SteamVR_Input_Sources.RightHand)) // 手柄控制缩放
                {
                    selectObject.transform.localScale += Vector3.one * scaleRate * Time.deltaTime;
                    // 记录放缩时间
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
                    // 记录放缩时间
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
                    // 正常进行空间映射以更新物体位置
                    selectObject.transform.position = GetObjecPosition(anchor_scene, heightOfSM, localHandle.transform.localPosition, heightOfHM);
                }


                // 按下手柄扳机表示释放物体
                if (selectManipulateObject.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    // Debug.Log("释放物体，操作结束，本次精度为："+Vector3.Distance(selectObject.transform.position,targetObject.transform.position));

                    InitialManipulateEvironment(); // 初始化操作环境

                    ghostGaze.SetManipulateStatus(manipulateStatus); // 同步幽灵凝视的状态

                    gazeHeatMap.InitialHeatmap(); // 初始化热度图和体素队列

                    handHeatMap.InitialHandHeatmap(); // 初始化手热度图

                    if (isUserStudy1)
                    {
                        selectObject.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().enabled = true;// 打开目标物体的碰撞检测组件
                    }
                    if (isUserStudy2)
                    {
                        selectObject.GetComponent<BoxCollider>().enabled = true;
                    }

                    // 时间采样
                    if (isSampleData)
                    {
                        Debug.Log("【采样】开始写入本次操作数据");
                        //smSamplerScript.manipulateStopTime = Time.time; // 获取游戏当前时间
                        //smSamplerScript.CalculateManipulateError(selectObject.transform.position, targetObject.transform.position);
                        //smSamplerScript.WriteExperimentData2File();
                        //smSamplerScript.isFine2AlignRecorded = false; // 重置
                        usSampler.mFinishTimePoint = Time.time;
                        usSampler.WriteManipulateResult2File(selectObject, targetObject);
                        // 重置数据
                        usSampler.UpdateSubMission(); // 子实验次数++，将其他实验数据归零
                        // 重置物体位置、角度和放缩
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


            // 更新上一帧手柄信息
            previousLeftHandRosition = leftHand.transform.rotation;
            previousRightHandRotation = rightHand.transform.rotation;
            previousRightHandPosition = rightHand.transform.position;


            UpdateLine();

            // ----采样程序----

            // SampleManipulateData();
        }

        /// <summary>
        /// 更新手柄复制对象的世界坐标，与手柄同步
        /// </summary>
        void UpdateLocalHandle()
        {
            localHandle.transform.position = rightHand.transform.position;
        }

        /// <summary>
        /// 通过按键将单手操作空间的朝向和锚点与VR相机朝向同步
        /// </summary>
        void GetSHSForwardByPress()
        {
            if (updateUserHead.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                // 只同步y轴和x轴的偏转，z轴不考虑
                Vector3 eyeEulerAngle = new Vector3(0, mainCamera.transform.eulerAngles.y, mainCamera.transform.eulerAngles.z);

                userHead.transform.eulerAngles = eyeEulerAngle;
                // 同步空间锚点
                userHead.transform.position = mainCamera.transform.position;
                Debug.Log("更新了单手操作空间的朝向和锚点");
            }

        }

        /// <summary>
        /// 按键获取肩膀世界坐标
        /// </summary>
        void GetShoulderPosition()
        {
            if (selectUserShoulder.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                if (!isUserShoulderSampled)
                {
                    // 获取此时的右手世界坐标并赋值给肩膀
                    userShoulder.transform.position = rightHand.transform.position;
                    isUserShoulderSampled = true;
                    Debug.Log("成功获取肩膀坐标");

                    // 单手操作空间变量初始化
                    p_shoulder = userShoulder.transform.localPosition; 
                    p_elbow = p_shoulder + userBoomLength * Vector3.down; // 手肘位置

                    // ---------进行可视化------------
                    // 1.可视化肩膀
                    GameObject shouderObject = Instantiate(pointPrefab, userShoulder.transform);
                    shouderObject.name = "肩膀点";
                    shouderObject.transform.localScale = 0.05f * Vector3.one;
                    // 2.可视化手肘
                    elbowObject = Instantiate(pointPrefab, userShoulder.transform);
                    elbowObject.name = "手肘点";
                    elbowObject.transform.localScale = 0.05f * Vector3.one;
                    elbowObject.transform.localPosition = userBoomLength * Vector3.down;
                    if (isOpenVisualize)
                    {
                        
                        // 3.可视化单手操作空间
                        GameObject handSpace = Instantiate(handSpacePrefab, userShoulder.transform);
                        handSpace.name = "单手操作空间";
                        handSpace.transform.localScale = heightOfHM * Vector3.one;
                        handSpace.transform.localPosition = userBoomLength * Vector3.down;
                    }

                    
                }
            }
        }


        /// <summary>
        /// 通过手柄按键选取目标物体
        /// 目前使用射线获取:GetObjectByRaycast()
        /// </summary>
        void GetManipulateObject()
        {
            if (manipulateStatus.Equals(ManipulateStatus.UNSELECTED))
            {
                if (selectManipulateObject.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    Debug.Log("按下了获取物体的按键...");
                    GetObjectByHandleRay();
                }
            }
        }

        

        /// <summary>
        /// 需要被GetManipulateObject调用
        /// 算法1：通过射线获取需要操作的物体
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

                // 改变操作状态
                manipulateStatus = ManipulateStatus.SELECTED;
                
                // 手柄-物体连线可视化控制
                SetHandleRay(false);
                lineRenderer.enabled = true;

                // 设置GhostGaze中的操作物体
                ghostGaze.SetTargetObject(selectObject);
                ghostGaze.SetManipulateStatus(manipulateStatus);

                // 凝视中心可视化
                gazeHeatMap.staringCenterObject.SetActive(true);

                // 关闭物体碰撞体
                selectObject.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().enabled = false;


                // 时间采样
                if (isSampleData)
                {
                    //smSamplerScript.manipulateStartTime = Time.time; // 获取游戏当前时间
                    
                }

                Debug.Log("获取目标物体:" + selectObject.name);
            }
            else
            {
                // 空选，认为放弃当前物体
                Debug.Log("我目标物体呢团长我和你没完！");
                if (manipulateStatus.Equals(ManipulateStatus.SELECTED))
                {
                    Debug.Log("释放物体，操作结束");

                    InitialManipulateEvironment(); // 初始化操作环境

                    ghostGaze.SetManipulateStatus(manipulateStatus); // 同步幽灵凝视的状态

                    gazeHeatMap.InitialHeatmap(); // 初始化热度图和体素队列

                    selectObject.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().enabled = true; // 打开目标物体的碰撞检测组件


                    // 时间采样
                    //if (isSampleData)
                    //{
                    //    Debug.Log("开始写入本次操作数据");
                    //    smSamplerScript.manipulateStopTime = Time.time; // 获取游戏当前时间
                    //    smSamplerScript.CalculateManipulateError(selectObject.transform.position, targetObject.transform.position);
                    //    smSamplerScript.WriteExperimentData2File();
                    //}

                }
                
            }
        }


        /// <summary>
        /// 3.23 new
        /// 手柄发射射线尝试获取物体
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

                // 改变操作状态
                manipulateStatus = ManipulateStatus.SELECTED;

                if (isOpenVisualize)
                {
                    spaceManipulateObject.SetActive(true);
                    spaceManipulateObject.transform.localScale = heightOfSM * Vector3.one;

                    // 凝视中心可视化
                    gazeHeatMap.staringCenterObject.SetActive(true);
                }

                // 手柄-物体连线可视化控制
                SetHandleRay(false);
                lineRenderer.enabled = true;
                // 设置GhostGaze中的操作物体
                ghostGaze.SetTargetObject(selectObject);
                ghostGaze.SetManipulateStatus(manipulateStatus);
                
                // 关闭物体碰撞体
                if (isUserStudy1)
                {
                    selectObject.transform.GetChild(0).gameObject.GetComponent<MeshCollider>().enabled = false;
                }
                if (isUserStudy2)
                {
                    selectObject.GetComponent<BoxCollider>().enabled = false;
                }

                // 时间采样
                if (isSampleData)
                {
                    //smSamplerScript.manipulateStartTime = Time.time; // 获取游戏当前时间
                    //Debug.Log("开始操作时间：" + smSamplerScript.manipulateStartTime.ToString());
                    //smSamplerScript.manipulateDistance = Vector3.Distance(selectObject.transform.position, targetObject.transform.position); // 计算起点和终点的距离
                    usSampler.mStartTimePoint = Time.time;
                    Debug.Log("开始操作时间：" + usSampler.mStartTimePoint.ToString());
                    if (isUserStudy2)
                    {
                        targetObject = us2Controller.targetList[us2Controller.curMission];
                    }
                }
                Debug.Log("获取目标物体:" + selectObject.name);
            }
        }

        /// <summary>
        /// 采样操作数据：手柄、物体、凝视点
        /// </summary>
        void SampleManipulateData()
        {
            if (!isSampleData) { return; }
            if (manipulateStatus.Equals(ManipulateStatus.UNSELECTED)) { return; }
            // 采样凝视点 
            Vector3 gazeDirect = sRanipalGazeSample.GazeDirectionCombined;
            Vector3 eyePosition = Camera.main.transform.position;
            Ray gazeRay = new Ray(eyePosition, gazeDirect);
            RaycastHit hitInfo;
            if (Physics.Raycast(gazeRay, out hitInfo))
            {
                //smSamplerScript.SampleEyeGazePoint(hitInfo.point);
            }

            // 采样手柄
            //smSamplerScript.SampleHandlePoint(rightHand.transform.position);
            // 采样物体移动
            //smSamplerScript.SampleObjectPoint(selectObject.transform.position);
        }



        /// <summary>
        /// 获取场景操作空间的新锚点
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
            if (r_xy_2 < 0) { Debug.LogError("映射圆面半径计算小于0,请检查坐标初始化.."); }
            float r_xy = Mathf.Sqrt(r_xy_2); // 映射圆面半径

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
        /// 获取操作对象新位置，场景操作空间为倒立半球
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
            if (r_xy_2 < 0f) { Debug.LogError("计算xy圆平面半径时出错:"+depth.ToString()+","+z_offset.ToString()); }
            float r_xy = Mathf.Sqrt(r_xy_2);
            float y_offset = handleInHM.is_y_positive ? r_xy * handleInHM.rate_y : -r_xy * handleInHM.rate_y;
            float x_offset = handleInHM.is_x_posiyive ? r_xy * handleInHM.rate_x : -r_xy * handleInHM.rate_x;

            Vector3 p_obj = p_scene+z_offset*cameraInfo[1]+ x_offset * cameraInfo[2] + y_offset * cameraInfo[3];
            return p_obj;
        }

        /// <summary>
        /// 初始化操作环境，在每次操作开始前进行
        /// </summary>
        void InitialManipulateEvironment()
        {
            // 场景操作空间
            cameraInfo = new List<Vector3>(new Vector3[4]);// 用户视口信息
            heightOfSM = heightOfFrustum;

            // 操作状态
            manipulateStatus = ManipulateStatus.UNSELECTED; // 操作状态

            // 可视化
            if (isOpenVisualize)
            {
                spaceManipulateObject.SetActive(false); // 关闭可视化场景操作空间
                gazeHeatMap.staringCenterObject.SetActive(false); // 关闭凝视点中心对象
            }
            lineRenderer.enabled = false; // 关闭手柄和物体的连线
            SetHandleRay(true); // 原本手柄射线
        }

        private void UpdateObjectByHand()
        {
            if (rotateObject.GetState(SteamVR_Input_Sources.RightHand))
            {
                // 跟随手柄旋转
                Quaternion rotationDifference = rightHand.transform.rotation * Quaternion.Inverse(previousLeftHandRosition);
                selectObject.transform.rotation *= rotationDifference;
            }

            // 手柄控制缩放
            if (enlargeObject.GetState(SteamVR_Input_Sources.LeftHand))
            {
                Debug.Log("放大");
                selectObject.transform.localScale += Vector3.one * scaleRate * Time.deltaTime;
            }
            if (shrinkObject.GetState(SteamVR_Input_Sources.LeftHand))
            {
                Debug.Log("缩小");
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

        // 两种旋转代码
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


        // 可视化代码
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



        //-----------暂时弃用---------------

        /// <summary>
        /// 根据手柄局部坐标计算虚拟对象在场景操作空间中的位置
        /// 单手操作空间为球体
        /// </summary>
        /// <param name="p_h">手柄坐标</param>
        /// <returns>操作对象的坐标</returns>
        Vector3 GetSMSObjectPosition(Vector3 p_h)
        {
            float d_np = mainCamera.nearClipPlane; // distance_near_plane
            float d_fp = mainCamera.farClipPlane; // distance_far_plane
            d_fp = 7f; // 测试时规定
            float d_fn = d_fp - d_np; // 平截头体的高
            float l_arm = userArmLength;
            Vector3 p_s = userShoulder.transform.localPosition;

            Vector3 p_c = cameraInfo[0]; // 相机位置

            //Debug.Log("p_s:" + p_s.ToString());
            //Debug.Log("p_h:" + p_h.ToString());

            // 计算zo
            float z_rate = Mathf.Abs(p_h.z - p_s.z) / l_arm;
            float z_o; // 标量，之后需要乘以方向向量
            if (p_h.z > p_s.z)
            {
                z_o = d_np + d_fn * z_rate;
            }
            else
            {
                z_o = d_np - d_fn * z_rate;
            }

            // 计算z_o处的屏幕长宽
            float height_o = z_o * Mathf.Tan(halfFov);
            float width_o = height_o * aspectFov;


            // 计算xo
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

            // 计算yo
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

            // 计算p_o
            Vector3 p_o = p_c + cameraInfo[1] * z_o + cameraInfo[2] * x_o + cameraInfo[3] * y_o;
            return p_o;

        }

        /// <summary>
        /// 根据手柄局部坐标计算虚拟对象位置，单手操作空间为倒立半球
        /// </summary>
        /// <param name="p_h"></param>
        /// <returns></returns>
        Vector3 GetSMSObjectPositionHemi(Vector3 p_h)
        {
            float d_np = mainCamera.nearClipPlane; // distance_near_plane
            float d_fp = mainCamera.farClipPlane; // distance_far_plane
            d_fp = farPlaneValue; // 测试时规定
            float d_fn = d_fp - d_np; // 平截头体的高
            float r_arm = heightOfHM; // 适当放大userArmLength

            Vector3 p_shoulder = userShoulder.transform.localPosition; // 肩膀局部坐标
            Vector3 p_elbow = p_shoulder - userBoomLength * Vector3.down; // 手肘坐标=肩膀坐标-大臂长度*(0,-1,0)
            Vector3 p_camera = cameraInfo[0]; // 相机位置

            // 计算zo

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

            // 计算z_object处的屏幕长宽
            float height_object = z_object * Mathf.Tan(halfFov);
            float width_object = height_object * aspectFov;



            float r_xy_2 = Mathf.Pow(r_arm, 2) - Mathf.Pow(r_arm - z_abs_dis, 2);
            if (r_xy_2 < 0) { Debug.LogError("映射圆面半径计算小于0,请检查坐标初始化.."); }
            float r_xy = Mathf.Sqrt(r_xy_2); // 映射圆面半径

            // 计算x_object
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

            // 计算y_object

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

            // 计算p_o
            Vector3 p_object = p_camera + cameraInfo[1] * z_object + cameraInfo[2] * x_object + cameraInfo[3] * y_object;
            return p_object;
        }



    }

}
