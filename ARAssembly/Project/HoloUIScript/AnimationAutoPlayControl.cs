using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using TMPro;

public class AnimationAutoPlayControl : MonoBehaviour
{
    public GameObject AnimationAutoPlayPanel;
    private AssemblyInfo assemblyInfo;

    private bool isAutoPlaying = false;
    private int curAnimSN = 0;
    private List<bool> animationStateList;

    // �Զ�װ�������ʾ
    public TMP_Text component_sn;
    public TMP_Text total_number;

    private AssemblyProgressPanelCotrol assemblyProgress;




    // Start is called before the first frame update
    void Start()
    {
        assemblyInfo = GetComponent<AssemblyInfo>();
        animationStateList = new List<bool>();
        for (int i = 0; i < assemblyInfo.componentAssemblySequence.Count; i++)
        {
            animationStateList.Add(false);
        }
        curAnimSN = assemblyInfo.curSN;
        InitialAssemblyProgressBar(curAnimSN+1, assemblyInfo.componentAssemblySequence.Count);

        assemblyProgress = GetComponent<AssemblyProgressPanelCotrol>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isAutoPlaying)
        {
            curAnimSN = assemblyInfo.curSN;
            AnimationGenerator animationGenerator = assemblyInfo.componentAssemblySequence[curAnimSN].GetComponent<AnimationGenerator>();
            
            if (animationGenerator == null)
            {
                Debug.Log("û�ж���.." + assemblyInfo.componentAssemblySequence[curAnimSN].name);
                assemblyInfo.componentAssemblySequence[curAnimSN].SetActive(true);
                animationStateList[curAnimSN] = true;

                SetAssemblyProgressBar(curAnimSN+1); // �õ�ǰ���ȸ���
                assemblyProgress.AddNewProgressInfo(curAnimSN + 1, assemblyInfo.componentAssemblySequence[curAnimSN].name);

                curAnimSN++;
                assemblyInfo.curSN = curAnimSN; // �޸Ľ���
            }
            else
            {
                if (animationStateList[curAnimSN] == false)
                {
                    assemblyInfo.componentAssemblySequence[curAnimSN].SetActive(true);
                    animationGenerator.PlayAnimation();
                    Debug.Log("��ʼ����..." + assemblyInfo.componentAssemblySequence[curAnimSN].name);
                    animationStateList[curAnimSN] = true;

                    SetAssemblyProgressBar(curAnimSN + 1); // �õ�ǰ���ȸ���
                    assemblyProgress.AddNewProgressInfo(curAnimSN + 1, assemblyInfo.componentAssemblySequence[curAnimSN].name);

                }
                else if (animationGenerator.generatedAnimList[0].isPlaying == false)
                {
                    animationGenerator.StopAnimation();
                    Debug.Log("���Ž���..." + assemblyInfo.componentAssemblySequence[curAnimSN].name);
                    curAnimSN++;

                    assemblyInfo.curSN = curAnimSN; // �޸Ľ���

                }
            }
            if (curAnimSN == assemblyInfo.componentAssemblySequence.Count)
            {
                isAutoPlaying = false;
            }
            
        }
    }

    public void OnPressPinButton() {
        Follow follow = AnimationAutoPlayPanel.GetComponent<Follow>();
        follow.enabled = !follow.enabled;
    }

    public void OnPressPlayButton()
    {
        if(isAutoPlaying == false)
        {
            assemblyInfo.curSN++;
            
            isAutoPlaying = true;
        }
    }

    public void OnPressPauseButton()
    {
        if(isAutoPlaying == true)
        {
            isAutoPlaying = false;
        }
    }

    public void OnPressOpenPanelButton()
    {
        AnimationAutoPlayPanel.SetActive(!AnimationAutoPlayPanel.activeSelf);
    }

    public void InitialAssemblyProgressBar(int curSN,int totalNumber)
    {
        component_sn.text = curSN.ToString();
        total_number.text = totalNumber.ToString();
    }

    public void SetAssemblyProgressBar(int curSN)
    {
        component_sn.text = curSN.ToString();
    }
   
}
