using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject parentDevice;

    // �ű������
    private DeviceController deviceController;
    private AssemblyInfo assemblyInfo;

    private List<GameObject> childList;
    private int curSN = -1;

    private GameObject mainCamera;

    // ������������y���ƫ����
    public float offset_y = 1.5f;
    // ������������x���ƫ����
    public float offset_x = 5f;

    // ����ʵ�����ƽ���ƶ�
    private float _timeStartedLerping;
    // ���һ���ƶ����ѵ�ʱ��(s)
    public float timeTakenDuringLerp = 2f; 


    private void Awake()
    {
        Debug.Log("CameraController Awake");

        assemblyInfo = GetComponent<AssemblyInfo>();
        deviceController = GetComponent<DeviceController>();

        parentDevice = assemblyInfo.parentDevice;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("CameraController Start");

        childList = new List<GameObject>();
        childList = assemblyInfo.componentAssemblySequence;
        mainCamera = GameObject.FindWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// ͨ���������������׼�����壬�Ӷ��ƶ����
    /// </summary>
    /// <param name="keyCode"></param>
    public void ControlCameraByKey(KeyCode keyCode)
    {
        if (keyCode == KeyCode.Q)
        {
            if(curSN == -1) { return; }
            else if (curSN <= 0)
            {
                // �Ѿ��ǳ�ʼ�����壬�����ƶ����
                curSN--;
            }
            else
            {
                curSN--;
                MoveCamera(curSN);
            }

        }
        if (keyCode == KeyCode.E)
        {
            if (curSN >= childList.Count - 1)
            {
                Debug.LogError("�Ѿ������һ������");
                return;
            }
            else
            {
                curSN++;
                MoveCamera(curSN);
            }
        }
    }

    /// <summary>
    /// �ƶ������nextSN��Ӧ������ӵ㴦
    /// </summary>
    /// <param name="nextSN"></param>
    public void MoveCamera(int nextSN)
    {
        GameObject nextChild = childList[nextSN];
        // ��ȡ�����Mesh��Χ������
        Vector3 nextChild_center_position = GetBoundsCenter(nextChild);

        // Debug.Log("center position:" + nextChild_center_position);
        // ����y��x���ƫ�����������Ŀ��λ��
        Vector3 target_position = nextChild_center_position + Vector3.right*offset_x + Vector3.up*offset_y;
        // Debug.Log("target position:" + target_position);

        // ������������-���Ŀ��λ�� = ����ķ�������
        Vector3 target_forward = nextChild_center_position - target_position;
        Quaternion q_forward = Quaternion.LookRotation(target_forward);

        // �ƶ��Ŀ�ʼʱ��
        _timeStartedLerping = Time.time; 

        StartCoroutine(LerpMoveCamera(target_position, q_forward));
        

        
    }

    // ͨ��Э�̽��������ƽ���ƶ���ת��
    IEnumerator LerpMoveCamera(Vector3 target_position, Quaternion q_forward)
    {
        while(mainCamera.transform.position!= target_position)
        {
            yield return new WaitForFixedUpdate();

            float timeSinceStarted = Time.time - _timeStartedLerping;
            float percentageComplete = timeSinceStarted / timeTakenDuringLerp;

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target_position, percentageComplete);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, q_forward, percentageComplete);
            
            if (percentageComplete >= 1.0f)
            {
                break;
            }
        }
        Debug.Log("����˴��ƶ�����:" + mainCamera.transform.position + ";" + target_position);
    }


    Vector3 GetBoundsCenter(GameObject target)
    {
        //��ȡĿ������������������Ⱦ
        MeshRenderer[] meshRenderers = target.GetComponentsInChildren<MeshRenderer>(true);
        if (meshRenderers.Length == 0)
        {
            Debug.LogError("������û������");
            return new Vector3();
        }
        //�����е�������Ⱦ�ı߽���кϲ�
        Bounds centerBounds = meshRenderers[0].bounds;
        for (int i = 1; i < meshRenderers.Length; i++)
        {
            centerBounds.Encapsulate(meshRenderers[i].bounds);
        }
        return centerBounds.center;
    }
}

