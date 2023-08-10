using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceController : MonoBehaviour
{
    private AssemblyInfo assemblyInfo;
    private List<GameObject> deviceList;
    private List<GameObject> waikeList;

    public int curSN = -1;
    private int deviceSN = 0;
    private int colorNum = 0;

    private List<Color> colorList;


    private void Awake()
    {
        Debug.Log("DeviceController Awake");

        // 从主信息脚本获取变量
        assemblyInfo = GetComponent<AssemblyInfo>();
        deviceList = assemblyInfo.deviceList;
        waikeList = assemblyInfo.waikeList;

        // 添加材质
        //colorList = new List<Color>();
        //InitialColorList(colorList); // 初始化colorlist
        //foreach(GameObject device in deviceList)
        //{
        //    AddMaterial(device);
        //}

        // 为设备列表的每个设备创建包围盒
        //CreateBoxColiderForTargetList(deviceList);

        CreateMeshCenterForTargetList(deviceList);
        CreateMeshCenterForTargetList(waikeList);

    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("DeviceController Start");
        SetAllDeviceDisActive();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 将设备列表中的所有物体设置为未激活
    /// </summary>
    public void SetAllDeviceDisActive()
    {
        for(int i = 0; i < deviceList.Count; i++)
        {
            deviceList[i].SetActive(false);
        }
    }


    /// <summary>
    /// 初始化子物体材质，递归函数
    /// </summary>
    /// <param name="parentDevice"></param>
    void AddMaterial(GameObject parentDevice)
    {
        // 如果没有子物体，返回        
        if (parentDevice.transform.childCount == 0) { return; }
        if (parentDevice.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>())
        {
            // Debug.Log("这是一个设备:" + parentDevice.name);
            deviceSN++;
        }

        // 有子物体，遍历子物体
        for(int i = 0; i < parentDevice.transform.childCount; i++)
        {
            GameObject curChild = parentDevice.transform.GetChild(i).gameObject;
            MeshRenderer meshRenderer = curChild.GetComponent<MeshRenderer>();
            // 子物体有面片
            if (meshRenderer)
            {
                // Debug.Log("子物体有面片:"+curChild.name);
                Material material = new Material(Shader.Find("Mixed Reality Toolkit/Standard"))
                {
                    color = colorList[(deviceSN-1)%colorNum]
                };
                // Debug.Log("color:" + colorList[i % colorNum].ToString());
                material.SetFloat("_Metallic", 1f);
                meshRenderer.material = material;
            }
            else // 子物体无面片
            {
                AddMaterial(curChild);
            }   
        }

    }

    /// <summary>
    /// 颜色列表初始化
    /// </summary>
    /// <param name="colorList"></param>
    void InitialColorList(List<Color> colorList)
    {
        colorList.Add(ColorPretreatment(1, 168, 80)); // 深绿
        colorList.Add(ColorPretreatment(48, 48, 148)); // 深蓝
        colorList.Add(ColorPretreatment(250, 149, 30)); // 淡橙色

        //colorList.Add(ColorPretreatment(255, 243, 0)); // 黄色
        colorList.Add(ColorPretreatment(0, 176, 239)); // 淡蓝色
        colorList.Add(ColorPretreatment(237,29,38)); // 大红色

        colorList.Add(ColorPretreatment(140, 196, 61)); // 淡绿色
        colorList.Add(ColorPretreatment(2, 116, 186)); // 湖蓝
        colorList.Add(ColorPretreatment(244,103,32)); // 橙色

        colorList.Add(ColorPretreatment(1, 170,156)); // 蓝绿色
        colorList.Add(ColorPretreatment(148, 37, 145)); // 紫色
        colorList.Add(ColorPretreatment(255, 197, 12)); // 淡淡橙色

        colorNum = colorList.Count;


    }

    /// <summary>
    /// 将0-255的色彩值转化进0-1的区间
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public Color ColorPretreatment(float r,float g,float b)
    {
        return new Color(r / 255, g / 255, b / 255);
    }

    public Bounds getBounds(GameObject target)
    {
        //获取目标物体下所有网格渲染
        MeshRenderer[] meshRenderers = target.GetComponentsInChildren<MeshRenderer>(true);
        if (meshRenderers.Length == 0)
        {
            Debug.LogError(target.name+" 子物体没有网格");
            return new Bounds();
        }
        //将所有的网格渲染的边界进行合并
        Bounds centerBounds = meshRenderers[0].bounds;
        for (int i = 1; i < meshRenderers.Length; i++)
        {
            centerBounds.Encapsulate(meshRenderers[i].bounds);
        }
        return centerBounds;
    }

    private void CreateBoxColiderForTargetList(List<GameObject> targetList)
    {
        foreach(GameObject target in targetList)
        {
            Bounds tempBounds = getBounds(target);

            var boxColider = target.AddComponent<BoxCollider>();
            

            boxColider.center = tempBounds.center;
            boxColider.size = tempBounds.size;
        }
    }

    private void CreateMeshCenterForTargetList(List<GameObject> targetList)
    {
        foreach (GameObject target in targetList)
        {
            Bounds tempBounds = getBounds(target);

            GameObject meshcenter = new GameObject("MeshCenter");
            meshcenter.transform.position = tempBounds.center;
            
            MeshCenterInfo meshCenterInfoScript = meshcenter.AddComponent<MeshCenterInfo>();
            meshCenterInfoScript.componentBounds = tempBounds;

            meshcenter.transform.parent = target.transform;

        }
    }



}
