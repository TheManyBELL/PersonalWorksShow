using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssemblyInfo : MonoBehaviour
{
    /// <summary>
    /// ��������
    /// </summary>
    public GameObject parentDevice;

    /// <summary>
    /// �����ƽű��������˻�ģ����
    /// </summary>
    public MainController mainController;

    // �����ƽű��� ���װ������ �б�
    public List<GameObject> componentAssemblySequence;
    // ��ǰװ������� ���װ������ �е��±�
    public int curSN = -1;

    // �����ƽű��� ��װ���豸 �б�
    public List<GameObject> deviceList;

    // �����ƽű��� ��װ������ �б�
    public List<GameObject> waikeList;

    private void Awake()
    {
        //componentAssemblySequence = mainController.componentAssemblySequence;
        componentAssemblySequence = mainController.assemblySequenceForDemo;
        deviceList = mainController.deviceList;
        waikeList = mainController.waikeList;
    }

}
