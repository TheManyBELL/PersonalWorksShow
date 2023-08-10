using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;


public class AssemblyProgressPanelCotrol : MonoBehaviour
{
    public GameObject AssemblyProgressPanel;

    public TMP_Text component_sn;
    public TMP_Text component_name;
    public TMP_Text component_state;

    private List<string> sn_list;
    private List<string> name_list;
    private List<string> state_list;

    private int start_index = 1;
    private int end_index = 0;
    private int info_max_column = 10;

    // Start is called before the first frame update
    void Start()
    {
        sn_list = new List<string>();
        name_list = new List<string>();
        state_list = new List<string>();

        sn_list.Add("序号\n");
        name_list.Add("组件名\n");
        state_list.Add("状态\n");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPressProgressButton()
    {
        AssemblyProgressPanel.SetActive(!AssemblyProgressPanel.activeSelf);
    }

    public void OnPressPinButton()
    {
        Follow follow = AssemblyProgressPanel.GetComponent<Follow>();
        follow.enabled = !follow.enabled;
    }

    public void AddNewProgressInfo(int curSN, string name)
    {
        sn_list.Add(curSN.ToString() + "\n");
        name_list.Add(name +"\n");
        state_list.Add("<color=yellow>装配中</color>\n");

        if (end_index > 0)
        {
            state_list[end_index] = "已完成\n";
        }

        end_index++;
        if (end_index >= info_max_column)
        {
            start_index++;
        }

        string sn_string = sn_list[0];
        string name_string = name_list[0];
        string state_string = state_list[0];

        for(int i = start_index; i <= end_index; i++)
        {
            sn_string += sn_list[i];
            name_string += name_list[i];
            state_string += state_list[i];
        }

        component_sn.text = sn_string;
        component_name.text = name_string;
        component_state.text = state_string;
        
    }
}
