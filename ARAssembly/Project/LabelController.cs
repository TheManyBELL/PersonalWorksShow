using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabelController : MonoBehaviour
{
    private GameObject parentDevice;
    public GameObject labelPrefab;

    private List<GameObject> labelObjectList;
    private int curSN = -1;

    private GameObject currentLabelObject = null;
    private GameObject currentChildObject = null;

    private GameObject mainCamera;

    public float offset_y = 0.1f; // 标签相对物体的y轴偏移量

    // 脚本对象
    private AssemblyInfo assemblyInfo;


    // Start is called before the first frame update
    private void Awake()
    {
        Debug.Log("LabelController Awake");

        assemblyInfo = GetComponent<AssemblyInfo>();
        parentDevice = assemblyInfo.parentDevice;
        labelObjectList = new List<GameObject>();

        mainCamera = GameObject.FindWithTag("MainCamera");
    }

    void Start()
    {
        Debug.Log("LabelController Start");


        CreateLabel();
    }

    // Update is called once per frame
    void Update()
    {
        if (curSN == -1) { return; }
        currentLabelObject = labelObjectList[curSN];
        currentChildObject = parentDevice.transform.GetChild(curSN).transform.gameObject;

        Vector3 newPosition = getBoundsCenter(currentChildObject);
        newPosition.y += offset_y;
        currentLabelObject.transform.position = newPosition;

        var rotation = Quaternion.LookRotation(mainCamera.transform.TransformVector(Vector3.forward), mainCamera.transform.TransformVector(Vector3.up));
        rotation = new Quaternion(0, rotation.y, 0, rotation.w);
        currentLabelObject.transform.rotation = rotation;

    }

    public void CreateLabel()
    {
        //遍历子物体，为每个子物体生成标签对象
        //生成标签父物体，每个子物体的标签对象放在下面
        GameObject parentLabel = new GameObject(parentDevice.name + "_label");
        for (int i = 0; i < parentDevice.transform.childCount; i++)
        {
            GameObject childObject = parentDevice.transform.GetChild(i).gameObject;
            GameObject childLabel = Instantiate(labelPrefab);
            labelObjectList.Add(childLabel);
            childLabel.name = childObject.name + "_label";
            childLabel.transform.parent = parentLabel.transform;

            Text labelText = childLabel.transform.GetChild(0).GetChild(0).GetComponent<Text>();
            labelText.text = childObject.name;

            childLabel.SetActive(false);
        }

    }

    public void ControlLabelByKey(KeyCode keyCode)
    {
        if (keyCode == KeyCode.Q)
        {
            if (curSN <= -1)
            {
                Debug.LogError("已经没有标签!");
            }
            else if (curSN == 0)
            {
                GameObject curLabel = labelObjectList[curSN];
                curLabel.SetActive(false);

                curSN--;
            }
            else
            {
                GameObject curLabel = labelObjectList[curSN];
                curLabel.SetActive(false);

                curSN--;

                GameObject nextLabel = labelObjectList[curSN];
                nextLabel.SetActive(true);
            }
        }

        if(keyCode == KeyCode.E)
        {
            if (curSN >= labelObjectList.Count-1)
            {
                Debug.LogError("已经是最后一个标签!");
            }
            if(curSN == -1)
            {
                curSN++;
                GameObject nextLabel = labelObjectList[curSN];
                nextLabel.SetActive(true);
            }
            else if(curSN <labelObjectList.Count-1)
            {
                GameObject curLabel = labelObjectList[curSN];
                curLabel.SetActive(false);

                curSN++;
                GameObject nextLabel = labelObjectList[curSN];
                nextLabel.SetActive(true);

            }
        }
    }

    Vector3 getBoundsCenter(GameObject target)
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
