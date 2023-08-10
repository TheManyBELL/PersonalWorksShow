using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class HoloLabelGenerator : MonoBehaviour
{
    // ��Ҫ�ɿ�������ʼ��
    private GameObject labelPrefab;

    /// <summary>
    /// ��ǩ����
    /// </summary>
    public Vector3 pivotDirect;
    public float pivotDistance;
    public string labelName = "Label";

    public bool isOnTop = false;
    public bool isOnLeft = false;
    public bool isOnRight = false;
    public bool isOnCenter = false;

    
    void Start()
    {
        
    }

    public void SetLabelPrefab(GameObject prefab)
    {
        labelPrefab = prefab;
    }

    public void SetPivotPosition(Vector3 dir,float dis)
    {
        pivotDirect = dir;
        pivotDistance = dis;
    }

    public GameObject GenerateHoloLabel()
    {
        if (transform.Find(labelName))
        {
            Debug.LogError(this.name + "���������б�ǩ");
            return null;
        }
        GameObject holoLabel = Instantiate(labelPrefab);
        holoLabel.name = labelName;

        // �趨�ı�
        ToolTip toolTip = holoLabel.GetComponent<ToolTip>();
        toolTip.ToolTipText = this.name;

        // �趨ê�㣨ͨ����Χ�У�
        GameObject anchor = holoLabel.transform.GetChild(0).gameObject;

        anchor.transform.position = getAnchorPosition();

        GameObject pivot = holoLabel.transform.GetChild(1).gameObject;
        pivot.transform.position = anchor.transform.position + pivotDirect * pivotDistance;

        // Ȼ�����趨һ�¸����壬���趨���������ƫ�ԭ����
        holoLabel.transform.parent = this.transform;

        holoLabel.SetActive(false);

        return holoLabel;
    }

    private Vector3 getAnchorPosition()
    {
        GameObject meshCenter = this.transform.Find("MeshCenter").gameObject;
        Bounds meshCenterBounds = meshCenter.GetComponent<MeshCenterInfo>().componentBounds;
        Vector3 anchorPosition = new Vector3();
        if (isOnCenter) { anchorPosition = meshCenter.transform.position; }
        else if (isOnTop) { anchorPosition = meshCenter.transform.position + new Vector3(0, meshCenterBounds.size.y / 2f, 0); }
        else if (isOnLeft) { anchorPosition = meshCenter.transform.position + new Vector3(-meshCenterBounds.size.x / 2f, 0, 0); }
        else if (isOnRight) { anchorPosition = meshCenter.transform.position + new Vector3(meshCenterBounds.size.x / 2f, 0, 0); }
        return anchorPosition;
    }
}
