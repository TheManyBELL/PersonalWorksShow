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

        // ������Ϣ�ű���ȡ����
        assemblyInfo = GetComponent<AssemblyInfo>();
        deviceList = assemblyInfo.deviceList;
        waikeList = assemblyInfo.waikeList;

        // ��Ӳ���
        //colorList = new List<Color>();
        //InitialColorList(colorList); // ��ʼ��colorlist
        //foreach(GameObject device in deviceList)
        //{
        //    AddMaterial(device);
        //}

        // Ϊ�豸�б��ÿ���豸������Χ��
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
    /// ���豸�б��е�������������Ϊδ����
    /// </summary>
    public void SetAllDeviceDisActive()
    {
        for(int i = 0; i < deviceList.Count; i++)
        {
            deviceList[i].SetActive(false);
        }
    }


    /// <summary>
    /// ��ʼ����������ʣ��ݹ麯��
    /// </summary>
    /// <param name="parentDevice"></param>
    void AddMaterial(GameObject parentDevice)
    {
        // ���û�������壬����        
        if (parentDevice.transform.childCount == 0) { return; }
        if (parentDevice.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>())
        {
            // Debug.Log("����һ���豸:" + parentDevice.name);
            deviceSN++;
        }

        // �������壬����������
        for(int i = 0; i < parentDevice.transform.childCount; i++)
        {
            GameObject curChild = parentDevice.transform.GetChild(i).gameObject;
            MeshRenderer meshRenderer = curChild.GetComponent<MeshRenderer>();
            // ����������Ƭ
            if (meshRenderer)
            {
                // Debug.Log("����������Ƭ:"+curChild.name);
                Material material = new Material(Shader.Find("Mixed Reality Toolkit/Standard"))
                {
                    color = colorList[(deviceSN-1)%colorNum]
                };
                // Debug.Log("color:" + colorList[i % colorNum].ToString());
                material.SetFloat("_Metallic", 1f);
                meshRenderer.material = material;
            }
            else // ����������Ƭ
            {
                AddMaterial(curChild);
            }   
        }

    }

    /// <summary>
    /// ��ɫ�б��ʼ��
    /// </summary>
    /// <param name="colorList"></param>
    void InitialColorList(List<Color> colorList)
    {
        colorList.Add(ColorPretreatment(1, 168, 80)); // ����
        colorList.Add(ColorPretreatment(48, 48, 148)); // ����
        colorList.Add(ColorPretreatment(250, 149, 30)); // ����ɫ

        //colorList.Add(ColorPretreatment(255, 243, 0)); // ��ɫ
        colorList.Add(ColorPretreatment(0, 176, 239)); // ����ɫ
        colorList.Add(ColorPretreatment(237,29,38)); // ���ɫ

        colorList.Add(ColorPretreatment(140, 196, 61)); // ����ɫ
        colorList.Add(ColorPretreatment(2, 116, 186)); // ����
        colorList.Add(ColorPretreatment(244,103,32)); // ��ɫ

        colorList.Add(ColorPretreatment(1, 170,156)); // ����ɫ
        colorList.Add(ColorPretreatment(148, 37, 145)); // ��ɫ
        colorList.Add(ColorPretreatment(255, 197, 12)); // ������ɫ

        colorNum = colorList.Count;


    }

    /// <summary>
    /// ��0-255��ɫ��ֵת����0-1������
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
        //��ȡĿ������������������Ⱦ
        MeshRenderer[] meshRenderers = target.GetComponentsInChildren<MeshRenderer>(true);
        if (meshRenderers.Length == 0)
        {
            Debug.LogError(target.name+" ������û������");
            return new Bounds();
        }
        //�����е�������Ⱦ�ı߽���кϲ�
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
