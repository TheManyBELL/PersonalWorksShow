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

    // 自动装配进度显示
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
                Debug.Log("没有动画.." + assemblyInfo.componentAssemblySequence[curAnimSN].name);
                assemblyInfo.componentAssemblySequence[curAnimSN].SetActive(true);
                animationStateList[curAnimSN] = true;

                SetAssemblyProgressBar(curAnimSN+1); // 用当前进度更新
                assemblyProgress.AddNewProgressInfo(curAnimSN + 1, assemblyInfo.componentAssemblySequence[curAnimSN].name);

                curAnimSN++;
                assemblyInfo.curSN = curAnimSN; // 修改进度
            }
            else
            {
                if (animationStateList[curAnimSN] == false)
                {
                    assemblyInfo.componentAssemblySequence[curAnimSN].SetActive(true);
                    animationGenerator.PlayAnimation();
                    Debug.Log("开始播放..." + assemblyInfo.componentAssemblySequence[curAnimSN].name);
                    animationStateList[curAnimSN] = true;

                    SetAssemblyProgressBar(curAnimSN + 1); // 用当前进度更新
                    assemblyProgress.AddNewProgressInfo(curAnimSN + 1, assemblyInfo.componentAssemblySequence[curAnimSN].name);

                }
                else if (animationGenerator.generatedAnimList[0].isPlaying == false)
                {
                    animationGenerator.StopAnimation();
                    Debug.Log("播放结束..." + assemblyInfo.componentAssemblySequence[curAnimSN].name);
                    curAnimSN++;

                    assemblyInfo.curSN = curAnimSN; // 修改进度

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
