using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControllerNew : MonoBehaviour
{
    public GameObject airborneEquipmentParent;
    private AssemblyInfo assemblyInfo;
    private List<GameObject> deviceList;

    private List<GameObject> waikeList;

    // Start is called before the first frame update
    void Start()
    {
        assemblyInfo = GetComponent<AssemblyInfo>();
        deviceList = assemblyInfo.deviceList;
        waikeList = assemblyInfo.waikeList;

        GenerateDeviceAnimation(deviceList);
        GenerateDeviceAnimation(waikeList);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateDeviceAnimation(List<GameObject> targetList)
    {
        for(int i = 0; i < targetList.Count; i++)
        {
            GameObject device = targetList[i];
            DeviceAnimationGenerator deviceAnimationScrip;
            // 已有动画脚本，跳过
            if (device.GetComponent<DeviceAnimationGenerator>()) {
                deviceAnimationScrip = device.GetComponent<DeviceAnimationGenerator>();
            }
            else
            {
                deviceAnimationScrip = device.AddComponent<DeviceAnimationGenerator>();
            }
            deviceAnimationScrip.InitialBindingInfo();
            deviceAnimationScrip.GenerateAnimation();
        }
    }
}
