using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloLabelController : MonoBehaviour
{
    [Tooltip("基于hololens官方样例制作的标签预制体")]
    public GameObject HoloLabelPrefab;

    // 标签对象列表
    private List<GameObject> labelObjectList;
    // 设备对象列表
    private List<GameObject> deviceList;
    // 当前装配进度
    private int curSN = -1;
    
    /// <summary>
    /// 其他脚本类
    /// </summary>
    private AssemblyInfo assemblyInfo;
    private DeviceController deviceController;

    // 标签中心点坐标
    private Vector3 pivot_dir;
    // 标签中心相对锚点的偏移量
    private float pivot_offset;

    // 主相机
    private Camera mainCamera;

    // 标签可被看见的设备最远距离
    private float deviceMaxDistance = 2f;


    // for label layout 2
    // 视口中可见标签列表
    private List<ViewPortLabelInfo> visibleLableInPortView;

    // 视口标签信息
    struct ViewPortLabelInfo
    {
        public Vector2 labelPosition;
        public int labelIndex;
    }


    // for label layout 3
    // 视口随机采样点列表
    private List<Vector2> sampleList;


    // yield动画
    // 用于实现平滑移动
    private float _timeStartedLerping;

    [Tooltip("一次移动花费的时间(s)")]
    public float timeTakenDuringLerp = 2f;


    void Awake()
    {
        assemblyInfo = GetComponent<AssemblyInfo>();
        deviceController = GetComponent<DeviceController>();

        labelObjectList = new List<GameObject>();
        deviceList = new List<GameObject>();
        visibleLableInPortView = new List<ViewPortLabelInfo>();
        sampleList = new List<Vector2>();

        pivot_dir = new Vector3(0,1,0);
        pivot_offset = 0.2f;

        mainCamera = Camera.main;
    }

    private void Start()
    {
        deviceList = assemblyInfo.deviceList;
        // CreateLabelForTargetList(deviceList);
        CreateLabelForTargetListByScript(deviceList);
    }

    // Update is called once per frame
    void Update()
    {
        LabelVisibilityControl();
    }

    /// <summary>
    /// 为目标组件列表创建标签
    /// </summary>
    private void CreateLabelForTargetList(List<GameObject> targetList)
    {
        // GameObject LabelParent = new GameObject(parentDevice.name + "LabelList");
        for (int i = 0; i < targetList.Count; i++)
        {
            GameObject target = targetList[i];
            string holoLabelName = "Label";
            // 检查是否已经手工定义标签
            GameObject holoLabel;
            if (target.transform.Find(holoLabelName))
            {
                Debug.Log(target.name + "已有标签");
                holoLabel = target.transform.Find(holoLabelName).gameObject;
                holoLabel.SetActive(false);
                labelObjectList.Add(holoLabel);
                continue;
            }
            holoLabel = Instantiate(HoloLabelPrefab);
            holoLabel.name = holoLabelName;

            // 设定文本
            ToolTip toolTip = holoLabel.GetComponent<ToolTip>();
            toolTip.ToolTipText = target.name;

            // 设定锚点（通过包围盒）
            GameObject anchor = holoLabel.transform.GetChild(0).gameObject;

            GameObject meshCenter = target.transform.Find("MeshCenter").gameObject;
            Bounds meshCenterBounds = meshCenter.GetComponent<MeshCenterInfo>().componentBounds;

            anchor.transform.position = meshCenter.transform.position + new Vector3(0, meshCenterBounds.size.y / 2f, 0);

            GameObject pivot = holoLabel.transform.GetChild(1).gameObject;
            pivot.transform.position = anchor.transform.position+ pivot_dir * pivot_offset;

            // 然后再设定一下父物体，先设定父物体会有偏差，原因不明
            holoLabel.transform.parent = target.transform;

            holoLabel.SetActive(false);

            labelObjectList.Add(holoLabel);

        }
    }

    /// <summary>
    /// 通过脚本为目标组件列表创建标签
    /// 2022.6.9
    /// </summary>
    private void CreateLabelForTargetListByScript(List<GameObject> targetList)
    {
        foreach(GameObject target in targetList)
        {
            GameObject holoLabel;
            HoloLabelGenerator holoLabelGenerator;
            if (target.GetComponent<HoloLabelGenerator>())
            {
                holoLabelGenerator = target.GetComponent<HoloLabelGenerator>();
            }
            else
            {
                holoLabelGenerator = target.AddComponent<HoloLabelGenerator>();
                holoLabelGenerator.SetPivotPosition(pivot_dir, pivot_offset);
                holoLabelGenerator.isOnTop = true;
            }
            holoLabelGenerator.SetLabelPrefab(HoloLabelPrefab);
            holoLabel = holoLabelGenerator.GenerateHoloLabel();

            labelObjectList.Add(holoLabel);
        }
    }

    /// <summary>
    /// 标签可见性控制
    /// </summary>
    private void LabelVisibilityControl()
    {
        for (int i = 0; i < deviceList.Count; i++)
        {

            GameObject curChild = deviceList[i];
            if (curChild.activeSelf == false) { continue; }
            GameObject curLabel = labelObjectList[i];
            Vector3 childCenter = curChild.transform.Find("MeshCenter").position;

            // 设备中心是否在用户视口内
            bool isDeviceInCameraView = IsPositionInCameraView(childCenter);
            
            // 设备中心与相机距离是否合适
            bool isDeviceInLegalRange = IsDeviceInLegalRange(childCenter);
            
            if (isDeviceInCameraView && isDeviceInLegalRange)
            {
                //Debug.Log(curLabel.name);
                curLabel.SetActive(true);
            }
            else
            {
                if (!isDeviceInCameraView)
                {
                    //Debug.Log(curChild.name + "不在用户视口");
                }
                if (!isDeviceInLegalRange)
                {
                    //Debug.Log(curChild.name + "距离相机太远");
                }
                curLabel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 设备中心是否在视口范围内
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    private bool IsPositionInCameraView(Vector3 targetPosition)
    {

        Transform cameraTransform = mainCamera.transform;
        Vector2 viewPos = mainCamera.WorldToViewportPoint(targetPosition);
        // Debug.Log("设备在视口的坐标:"+viewPos.ToString("2f"));
        Vector3 dir = (targetPosition - cameraTransform.position).normalized;
        float dot = Vector3.Dot(cameraTransform.forward, dir); // 判断物体是否在相机前面

        if (dot > 0f && viewPos.x >= 0f && viewPos.x <= 1f && viewPos.y >= 0f && viewPos.y <= 1f) return true;
        else return false;
    }
    
    /// <summary>
    /// 设备中心是否与相机的距离合适
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    private bool IsDeviceInLegalRange(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(mainCamera.transform.position, targetPosition);
        if (distance < deviceMaxDistance) return true;
        else return false;
    }

    /// <summary>
    /// 标签布局main函数
    /// 输入是标签总列表
    /// </summary>
    public void LabelLayoutAlgorithm()
    {
        ProjectLabelToViewport();

        MoveViewportLabel();

        ProjectLabelToWorldSpace();
        // 清空可见标签列表
        visibleLableInPortView.Clear();
    }

    /// <summary>
    /// 将所有可见标签投影到视口坐标系
    /// </summary>
    private void ProjectLabelToViewport()
    {
        for(int i = 0; i < labelObjectList.Count; i++)
        {
            GameObject curLabel = labelObjectList[i];
            if (curLabel.activeSelf==true)
            {
                Transform curLabelPivot = curLabel.transform.GetChild(1);

                visibleLableInPortView.Add(new ViewPortLabelInfo
                {
                    labelPosition = mainCamera.WorldToViewportPoint(curLabelPivot.position),
                    labelIndex = i
                });
            }
        }
    }

    /// <summary>
    /// 将标签投影回世界坐标系
    /// </summary>
    private void ProjectLabelToWorldSpace()
    {
        Plane cameraPlane = new Plane(mainCamera.transform.forward, mainCamera.transform.position);

        for (int i = 0; i < visibleLableInPortView.Count; i++)
        {
            ViewPortLabelInfo vpLabel = visibleLableInPortView[i];

            GameObject curLabelObject = labelObjectList[vpLabel.labelIndex];
            GameObject labelPivot = curLabelObject.transform.GetChild(1).gameObject;
            float dis_toworldLabel = cameraPlane.GetDistanceToPoint(labelPivot.transform.position);

            Vector3 newWorldPosition = mainCamera.ViewportToWorldPoint(new Vector3(vpLabel.labelPosition.x, vpLabel.labelPosition.y, dis_toworldLabel));

            // 移动的开始时间
            _timeStartedLerping = Time.time;

            StartCoroutine(LerpMoveLabel(labelPivot,newWorldPosition));

        }
    }

    /// <summary>
    /// 标签平滑移动
    /// </summary>
    /// <param name="labelPivot"></param>
    /// <param name="target_position"></param>
    /// <returns></returns>
    IEnumerator LerpMoveLabel(GameObject labelPivot, Vector3 target_position)
    {
        while (labelPivot.transform.position != target_position)
        {
            yield return new WaitForFixedUpdate();

            float timeSinceStarted = Time.time - _timeStartedLerping;
            float percentageComplete = timeSinceStarted / timeTakenDuringLerp;

            labelPivot.transform.position = Vector3.Lerp(labelPivot.transform.position, target_position, percentageComplete);

            if (percentageComplete >= 1.0f)
            {
                break;
            }
        }
        Debug.Log("标签此次移动结束");
    }

    /// <summary>
    /// 排序辅助函数，用于Vector2类型数据
    /// <param name="item1"></param>
    /// <param name="item2"></param>
    /// <returns></returns>
    static int SortVector2ByDistance(Vector2 item1, Vector2 item2)
    {
        if (Vector2.Distance(new Vector2(0f,0f),item1) > Vector2.Distance(new Vector2(0f, 0f), item2)) return 1;
        else return -1;
    }

    /// <summary>
    /// 排序辅助函数，用于ViewPortLabelInfo类型数据
    /// <param name="item1"></param>
    /// <param name="item2"></param>
    /// <returns></returns>
    static int SortViewPortLabelInfoByDistance(ViewPortLabelInfo item1, ViewPortLabelInfo item2)
    {
        if (Vector2.Distance(new Vector2(0f, 0f), item1.labelPosition) > Vector2.Distance(new Vector2(0f, 0f), item2.labelPosition)) return 1;
        else return -1;
    }

    /// <summary>
    /// 移动标签位置实现遮挡去除
    /// </summary>
    private void MoveViewportLabel()
    {
        int labelNum = visibleLableInPortView.Count;
        GenerateMitchellSampleList(labelNum, 20, 1, 1);
        List<Vector2> partSampleLsit = new List<Vector2>();
        for(int i = 0; i < labelNum; i++)
        {
            partSampleLsit.Add(sampleList[i]);
        }

        partSampleLsit.Sort(SortVector2ByDistance);
        visibleLableInPortView.Sort(SortViewPortLabelInfoByDistance);

        for(int i = 0; i < labelNum; i++)
        {
            visibleLableInPortView[i] = new ViewPortLabelInfo
            {
                labelIndex = visibleLableInPortView[i].labelIndex,
                labelPosition = partSampleLsit[i]
            };
        }

    }

    /// <summary>
    /// 根据Mitchell算法在视口上随机采样
    /// </summary>
    /// <param name="sampleNum">需要采样的点的总数</param>
    /// <param name="candidateNum">每次采样时参选者数量</param>
    /// <param name="width">视口宽度：1</param>
    /// <param name="height">视口高度：1</param>
    private void GenerateMitchellSampleList(int sampleNum,int candidateNum,float width,float height)
    {
        if (sampleList.Count == 0)
        {
            sampleList.Add(new Vector2(0.5f, 0.5f));
        }

        int curSampleListLength = sampleList.Count;
        if (sampleNum <= curSampleListLength) { return; }

        for(int i = curSampleListLength; i < sampleNum; i++)
        {
            sampleList.Add(MitchellSample(candidateNum, width, height));
        }
    }

    /// <summary>
    /// 根据Mitchell算法采样一个点
    /// </summary>
    /// <param name="numCandidates">参选者数量</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    private Vector2 MitchellSample(int numCandidates,float width,float height)
    {
        Vector2 bestCandidate = new Vector2();
        float bestDistance = 0;
        
        for(int i = 0; i < numCandidates; ++i)
        {
            Vector2 c = new Vector2(Random.Range(width*0.2f, width*0.8f), Random.Range(height*0.3f, height*0.8f));
            float d = FindClosestSample(sampleList, c);
            if (d > bestDistance)
            {
                bestDistance = d;
                bestCandidate = c;
            }
        }
        Debug.Log("bestCandidate:" + bestCandidate.ToString("f3"));
        return bestCandidate;
    }

    /// <summary>
    /// Mitchell算法组件：找到和采样点最近的参选者
    /// </summary>
    /// <param name="sampleList"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    private float FindClosestSample(List<Vector2> sampleList,Vector2 c)
    {
        if (sampleList.Count == 0) { return 0; }
        Vector2 closestSample = sampleList[0];
        float bestDistance = Vector2.Distance(closestSample, c);

        for(int i = 1; i < sampleList.Count; i++)
        {
            float curDistance = Vector2.Distance(sampleList[i], c);
            if (curDistance < bestDistance)
            {
                bestDistance = curDistance;
            }
        }
        return bestDistance;
    }
}
