using EPOOutline;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if HDRP_OUTLINE
using UnityEngine.Rendering.HighDefinition;

[VolumeComponentMenu("EPO/Outline")]
public class OutlineCustomPass : CustomPass
{
    private List<Outlinable> tempOutlinables = new List<Outlinable>();

    private Queue<OutlineParameters> pool = new Queue<OutlineParameters>();
    
    private Queue<OutlineParameters> parametersInUse = new Queue<OutlineParameters>();

    private List<Outliner> outliners = new List<Outliner>();

    [SerializeField]
    [HideInInspector]
    private Camera lastSelectedCamera = null;

    protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult)
    {
        var camera = hdCamera.camera;

        if (outliners == null)
            outliners = new List<Outliner>();

        outliners.Clear();
        
#if UNITY_EDITOR
        if (camera.cameraType == CameraType.SceneView)
        {
            if (lastSelectedCamera == null)
            {
                foreach (var obj in UnityEditor.Selection.gameObjects)
                {
                    lastSelectedCamera = obj.GetComponent<Camera>();
                    if (lastSelectedCamera != null)
                        lastSelectedCamera.GetComponents(outliners);

                    if (outliners.Count != 0)
                        break;
                    else
                        lastSelectedCamera = null;
                }
            }

            outliners.Clear();
            if (lastSelectedCamera != null)
                lastSelectedCamera.GetComponents(outliners);
        }
        else
#endif
            camera.GetComponents(outliners);

        foreach (var outliner in outliners)
            UpdateOutliner(renderContext, camera, outliner, hdCamera);

        if (parametersInUse != null)
        {
            foreach (var param in parametersInUse)
                pool.Enqueue(param);

            parametersInUse.Clear();
        }
    }

    private void UpdateOutliner(ScriptableRenderContext renderContext, Camera camera, Outliner outlineEffect, HDCamera hdCamera)
    {
        if (outlineEffect == null || !outlineEffect.enabled)
            return;

        if (pool == null)
            pool = new Queue<OutlineParameters>();

        if (parametersInUse == null)
            parametersInUse = new Queue<OutlineParameters>();

        if (pool.Count == 0)
            pool.Enqueue(new OutlineParameters());

        var parameters = pool.Dequeue();
        parametersInUse.Enqueue(parameters);

        parameters.CheckInitialization();

        outlineEffect.UpdateSharedParameters(parameters, camera, camera.cameraType == CameraType.SceneView);
        Outlinable.GetAllActiveOutlinables(parameters.Camera, parameters.OutlinablesToRender);
        RendererFilteringUtility.Filter(parameters.Camera, parameters);

        parameters.Buffer.EnableShaderKeyword("EPO_HDRP");

        parameters.Buffer.SetGlobalMatrix("_Mat", hdCamera.mainViewConstants.nonJitteredViewProjMatrix);

        if (outlineEffect.RenderingStrategy == OutlineRenderingStrategy.Default)
        {
            outlineEffect.UpdateSharedParameters(parameters, camera, camera.cameraType == CameraType.SceneView);

            RTHandle colorTarget;
            RTHandle depthTarget;
            GetCameraBuffers(out colorTarget, out depthTarget);

            parameters.Target = colorTarget;
            parameters.DepthTarget = depthTarget;

            parameters.TargetWidth = colorTarget.rt.width;
            parameters.TargetHeight = colorTarget.rt.height;
            parameters.Antialiasing = colorTarget.rt.antiAliasing;

            parameters.Prepare();

            OutlineEffect.SetupOutline(parameters);

            renderContext.ExecuteCommandBuffer(parameters.Buffer);

            parameters.Buffer.Clear();
        }
        else
        {
            if (tempOutlinables == null)
                tempOutlinables = new List<Outlinable>();

            tempOutlinables.Clear();
            tempOutlinables.AddRange(parameters.OutlinablesToRender);

            foreach (var outlinable in tempOutlinables)
            {
                outlineEffect.UpdateSharedParameters(parameters, camera, camera.cameraType == CameraType.SceneView);

                RTHandle colorTarget;
                RTHandle depthTarget;
                GetCameraBuffers(out colorTarget, out depthTarget);

                parameters.Target = colorTarget;
                parameters.DepthTarget = depthTarget;

                parameters.TargetWidth = colorTarget.rt.width;
                parameters.TargetHeight = colorTarget.rt.height;
                parameters.Antialiasing = colorTarget.rt.antiAliasing;

                parameters.OutlinablesToRender.Clear();
                parameters.OutlinablesToRender.Add(outlinable);

                parameters.BlitMesh = null;

                parameters.Prepare();

                OutlineEffect.SetupOutline(parameters);
                renderContext.ExecuteCommandBuffer(parameters.Buffer);

                parameters.Buffer.Clear();
            }

            parameters.MeshPool.ReleaseAllMeshes();
        }
        
        parameters.Buffer.Clear();
    }
}
#endif