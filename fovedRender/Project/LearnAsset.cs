using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "ScriptShader/LearnSRP")]
public class LearnAsset : RenderPipelineAsset
{

    protected override RenderPipeline CreatePipeline()
    {
        return new LearnSRP();
    }
}