using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject parentDevice;

    // 脚本类对象
    private DeviceController deviceController;
    private AssemblyInfo assemblyInfo;

    private List<GameObject> childList;
    private int curSN = -1;

    private GameObject mainCamera;

    // 摄像机相对物体y轴的偏移量
    public float offset_y = 1.5f;
    // 摄像机相对物体x轴的偏移量
    public float offset_x = 5f;

    // 用于实现相机平滑移动
    private float _timeStartedLerping;
    // 相机一次移动花费的时间(s)
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
    /// 通过按键控制相机对准的物体，从而移动相机
    /// </summary>
    /// <param name="keyCode"></param>
    public void ControlCameraByKey(KeyCode keyCode)
    {
        if (keyCode == KeyCode.Q)
        {
            if(curSN == -1) { return; }
            else if (curSN <= 0)
            {
                // 已经是初始的物体，不再移动相机
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
                Debug.LogError("已经是最后一个动画");
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
    /// 移动相机到nextSN对应物体的视点处
    /// </summary>
    /// <param name="nextSN"></param>
    public void MoveCamera(int nextSN)
    {
        GameObject nextChild = childList[nextSN];
        // 获取物体的Mesh包围盒中心
        Vector3 nextChild_center_position = GetBoundsCenter(nextChild);

        // Debug.Log("center position:" + nextChild_center_position);
        // 根据y和x轴的偏移量计算相机目标位置
        Vector3 target_position = nextChild_center_position + Vector3.right*offset_x + Vector3.up*offset_y;
        // Debug.Log("target position:" + target_position);

        // 物体中心坐标-相机目标位置 = 相机的方向向量
        Vector3 target_forward = nextChild_center_position - target_position;
        Quaternion q_forward = Quaternion.LookRotation(target_forward);

        // 移动的开始时间
        _timeStartedLerping = Time.time; 

        StartCoroutine(LerpMoveCamera(target_position, q_forward));
        

        
    }

    // 通过协程进行相机的平滑移动的转向
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
        Debug.Log("相机此次移动结束:" + mainCamera.transform.position + ";" + target_position);
    }


    Vector3 GetBoundsCenter(GameObject target)
    {
        //获取目标物体下所有网格渲染
        MeshRenderer[] meshRenderers = target.GetComponentsInChildren<MeshRenderer>(true);
        if (meshRenderers.Length == 0)
        {
            Debug.LogError("子物体没有网格");
            return new Vector3();
        }
        //将所有的网格渲染的边界进行合并
        Bounds centerBounds = meshRenderers[0].bounds;
        for (int i = 1; i < meshRenderers.Length; i++)
        {
            centerBounds.Encapsulate(meshRenderers[i].bounds);
        }
        return centerBounds.center;
    }
}

