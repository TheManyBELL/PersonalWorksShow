using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public class LearnSRP : RenderPipeline
{
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        Camera camera = cameras[0];
        context.SetupCameraProperties(camera);

        CommandBuffer cmd = new CommandBuffer();
        cmd.name = "gbuffer";

        // ����
        cmd.ClearRenderTarget(true, true, Color.red);
        context.ExecuteCommandBuffer(cmd);

        // �޳�
        camera.TryGetCullingParameters(out var cullingParameters);
        var cullingResults = context.Cull(ref cullingParameters);

        // config settings
        ShaderTagId shaderTagId = new ShaderTagId("gbuffer");   // ʹ�� LightMode Ϊ gbuffer �� shader
        SortingSettings sortingSettings = new SortingSettings(camera);
        DrawingSettings drawingSettings = new DrawingSettings(shaderTagId, sortingSettings);
        FilteringSettings filteringSettings = FilteringSettings.defaultValue;

        // ����
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        // skybox and Gizmos
        context.DrawSkybox(camera);
        if (Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }

        // �ύ��������
        context.Submit();
    }
}