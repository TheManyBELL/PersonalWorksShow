using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloLabelController : MonoBehaviour
{
    [Tooltip("����hololens�ٷ����������ı�ǩԤ����")]
    public GameObject HoloLabelPrefab;

    // ��ǩ�����б�
    private List<GameObject> labelObjectList;
    // �豸�����б�
    private List<GameObject> deviceList;
    // ��ǰװ�����
    private int curSN = -1;
    
    /// <summary>
    /// �����ű���
    /// </summary>
    private AssemblyInfo assemblyInfo;
    private DeviceController deviceController;

    // ��ǩ���ĵ�����
    private Vector3 pivot_dir;
    // ��ǩ�������ê���ƫ����
    private float pivot_offset;

    // �����
    private Camera mainCamera;

    // ��ǩ�ɱ��������豸��Զ����
    private float deviceMaxDistance = 2f;


    // for label layout 2
    // �ӿ��пɼ���ǩ�б�
    private List<ViewPortLabelInfo> visibleLableInPortView;

    // �ӿڱ�ǩ��Ϣ
    struct ViewPortLabelInfo
    {
        public Vector2 labelPosition;
        public int labelIndex;
    }


    // for label layout 3
    // �ӿ�����������б�
    private List<Vector2> sampleList;


    // yield����
    // ����ʵ��ƽ���ƶ�
    private float _timeStartedLerping;

    [Tooltip("һ���ƶ����ѵ�ʱ��(s)")]
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
    /// ΪĿ������б�����ǩ
    /// </summary>
    private void CreateLabelForTargetList(List<GameObject> targetList)
    {
        // GameObject LabelParent = new GameObject(parentDevice.name + "LabelList");
        for (int i = 0; i < targetList.Count; i++)
        {
            GameObject target = targetList[i];
            string holoLabelName = "Label";
            // ����Ƿ��Ѿ��ֹ������ǩ
            GameObject holoLabel;
            if (target.transform.Find(holoLabelName))
            {
                Debug.Log(target.name + "���б�ǩ");
                holoLabel = target.transform.Find(holoLabelName).gameObject;
                holoLabel.SetActive(false);
                labelObjectList.Add(holoLabel);
                continue;
            }
            holoLabel = Instantiate(HoloLabelPrefab);
            holoLabel.name = holoLabelName;

            // �趨�ı�
            ToolTip toolTip = holoLabel.GetComponent<ToolTip>();
            toolTip.ToolTipText = target.name;

            // �趨ê�㣨ͨ����Χ�У�
            GameObject anchor = holoLabel.transform.GetChild(0).gameObject;

            GameObject meshCenter = target.transform.Find("MeshCenter").gameObject;
            Bounds meshCenterBounds = meshCenter.GetComponent<MeshCenterInfo>().componentBounds;

            anchor.transform.position = meshCenter.transform.position + new Vector3(0, meshCenterBounds.size.y / 2f, 0);

            GameObject pivot = holoLabel.transform.GetChild(1).gameObject;
            pivot.transform.position = anchor.transform.position+ pivot_dir * pivot_offset;

            // Ȼ�����趨һ�¸����壬���趨���������ƫ�ԭ����
            holoLabel.transform.parent = target.transform;

            holoLabel.SetActive(false);

            labelObjectList.Add(holoLabel);

        }
    }

    /// <summary>
    /// ͨ���ű�ΪĿ������б�����ǩ
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
    /// ��ǩ�ɼ��Կ���
    /// </summary>
    private void LabelVisibilityControl()
    {
        for (int i = 0; i < deviceList.Count; i++)
        {

            GameObject curChild = deviceList[i];
            if (curChild.activeSelf == false) { continue; }
            GameObject curLabel = labelObjectList[i];
            Vector3 childCenter = curChild.transform.Find("MeshCenter").position;

            // �豸�����Ƿ����û��ӿ���
            bool isDeviceInCameraView = IsPositionInCameraView(childCenter);
            
            // �豸��������������Ƿ����
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
                    //Debug.Log(curChild.name + "�����û��ӿ�");
                }
                if (!isDeviceInLegalRange)
                {
                    //Debug.Log(curChild.name + "�������̫Զ");
                }
                curLabel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// �豸�����Ƿ����ӿڷ�Χ��
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    private bool IsPositionInCameraView(Vector3 targetPosition)
    {

        Transform cameraTransform = mainCamera.transform;
        Vector2 viewPos = mainCamera.WorldToViewportPoint(targetPosition);
        // Debug.Log("�豸���ӿڵ�����:"+viewPos.ToString("2f"));
        Vector3 dir = (targetPosition - cameraTransform.position).normalized;
        float dot = Vector3.Dot(cameraTransform.forward, dir); // �ж������Ƿ������ǰ��

        if (dot > 0f && viewPos.x >= 0f && viewPos.x <= 1f && viewPos.y >= 0f && viewPos.y <= 1f) return true;
        else return false;
    }
    
    /// <summary>
    /// �豸�����Ƿ�������ľ������
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
    /// ��ǩ����main����
    /// �����Ǳ�ǩ���б�
    /// </summary>
    public void LabelLayoutAlgorithm()
    {
        ProjectLabelToViewport();

        MoveViewportLabel();

        ProjectLabelToWorldSpace();
        // ��տɼ���ǩ�б�
        visibleLableInPortView.Clear();
    }

    /// <summary>
    /// �����пɼ���ǩͶӰ���ӿ�����ϵ
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
    /// ����ǩͶӰ����������ϵ
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

            // �ƶ��Ŀ�ʼʱ��
            _timeStartedLerping = Time.time;

            StartCoroutine(LerpMoveLabel(labelPivot,newWorldPosition));

        }
    }

    /// <summary>
    /// ��ǩƽ���ƶ�
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
        Debug.Log("��ǩ�˴��ƶ�����");
    }

    /// <summary>
    /// ����������������Vector2��������
    /// <param name="item1"></param>
    /// <param name="item2"></param>
    /// <returns></returns>
    static int SortVector2ByDistance(Vector2 item1, Vector2 item2)
    {
        if (Vector2.Distance(new Vector2(0f,0f),item1) > Vector2.Distance(new Vector2(0f, 0f), item2)) return 1;
        else return -1;
    }

    /// <summary>
    /// ����������������ViewPortLabelInfo��������
    /// <param name="item1"></param>
    /// <param name="item2"></param>
    /// <returns></returns>
    static int SortViewPortLabelInfoByDistance(ViewPortLabelInfo item1, ViewPortLabelInfo item2)
    {
        if (Vector2.Distance(new Vector2(0f, 0f), item1.labelPosition) > Vector2.Distance(new Vector2(0f, 0f), item2.labelPosition)) return 1;
        else return -1;
    }

    /// <summary>
    /// �ƶ���ǩλ��ʵ���ڵ�ȥ��
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
    /// ����Mitchell�㷨���ӿ����������
    /// </summary>
    /// <param name="sampleNum">��Ҫ�����ĵ������</param>
    /// <param name="candidateNum">ÿ�β���ʱ��ѡ������</param>
    /// <param name="width">�ӿڿ�ȣ�1</param>
    /// <param name="height">�ӿڸ߶ȣ�1</param>
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
    /// ����Mitchell�㷨����һ����
    /// </summary>
    /// <param name="numCandidates">��ѡ������</param>
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
    /// Mitchell�㷨������ҵ��Ͳ���������Ĳ�ѡ��
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
