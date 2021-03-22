using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace EPOOutline
{
    public enum DilateQuality
    {
        Base,
        High,
        Ultra
    }

    public enum RenderingMode
    {
        LDR,
        HDR
    }

    public enum OutlineRenderingStrategy
    {
        Default,
        PerObject
    }

    public enum RenderStage
    {
        BeforeTransparents,
        AfterTransparents
    }

    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class Outliner : MonoBehaviour
    {
#if UNITY_EDITOR
        private static GameObject lastSelectedOutliner;

        private static List<Outliner> outliners = new List<Outliner>();
#endif

        private static List<Outlinable> temporaryOutlinables = new List<Outlinable>(); 

        private OutlineParameters parameters = new OutlineParameters();

#if UNITY_EDITOR
        private OutlineParameters editorPreviewParameters = new OutlineParameters();
#endif

        private Camera targetCamera;

        [SerializeField]
        private RenderStage stage = RenderStage.AfterTransparents;

        [SerializeField]
        private OutlineRenderingStrategy renderingStrategy = OutlineRenderingStrategy.Default;

        [SerializeField]
        private string renderTargetName = string.Empty;

        [SerializeField]
        private RenderingMode renderingMode;

        [SerializeField]
        private long outlineLayerMask = -1;

        [SerializeField]
        private bool scaleIndepented = true;

        [SerializeField]
        [Range(0.15f, 1.0f)]
        private float primaryRendererScale = 0.75f;

        [SerializeField]
        [Range(0.0f, 2.0f)]
        private float blurShift = 1.0f;

        [SerializeField]
        [Range(0.0f, 2.0f)]
        private float dilateShift = 1.0f;

        [SerializeField]
        [FormerlySerializedAs("dilateIterrations")]
        private int dilateIterations = 1;

        [SerializeField]
        private DilateQuality dilateQuality;

        [SerializeField]
        [FormerlySerializedAs("blurIterrations")]
        private int blurIterations = 1;

        [SerializeField]
        private BlurType blurType = BlurType.Box;

        [SerializeField]
        [Range(0.05f, 1.0f)]
        private float infoRendererScale = 0.75f;

        private CameraEvent Event
        {
            get
            {
                return stage == RenderStage.BeforeTransparents ? CameraEvent.AfterForwardOpaque : CameraEvent.BeforeImageEffects;
            }
        }

        public OutlineRenderingStrategy RenderingStrategy
        {
            get
            {
                return renderingStrategy;
            }

            set
            {
                renderingStrategy = value;
            }
        }

        public RenderStage RenderStage
        {
            get
            {
                return stage;
            }

            set
            {
                stage = value;
            }
        }

        public DilateQuality DilateQuality
        {
            get
            {
                return dilateQuality;
            }

            set
            {
                dilateQuality = value;
            }
        }

        public bool HasCutomRenderTarget
        {
            get
            {
                return !string.IsNullOrEmpty(renderTargetName);
            }
        }

        private RenderingMode RenderingMode
        {
            get
            {
                return renderingMode;
            }

            set
            {
                renderingMode = value;
            }
        }

        public float BlurShift
        {
            get
            {
                return blurShift;
            }

            set
            {
                blurShift = Mathf.Clamp(value, 0, 2.0f);
            }
        }

        public float DilateShift
        {
            get
            {
                return dilateShift;
            }

            set
            {
                dilateShift = Mathf.Clamp(value, 0, 2.0f);
            }
        }

        public long OutlineLayerMask
        {
            get
            {
                return outlineLayerMask;
            }

            set
            {
                outlineLayerMask = value;
            }
        }

        public bool ScaleIndependent
        {
            get
            {
                return scaleIndepented;
            }

            set
            {
                scaleIndepented = value;
            }
        }

        public float InfoRendererScale
        {
            get
            {
                return infoRendererScale;
            }

            set
            {
                infoRendererScale = Mathf.Clamp01(value);
            }
        }

        public float PrimaryRendererScale
        {
            get
            {
                return primaryRendererScale;
            }

            set
            {
                primaryRendererScale = Mathf.Clamp01(value);
            }
        }

        [Obsolete("Fixed incorrect spelling. Use BlurIterations instead")]
        public int BlurIterrations
        {
            get
            {
                return BlurIterations;
            }

            set
            {
                BlurIterations = value;
            }
        }

        public int BlurIterations
        {
            get
            {
                return blurIterations;
            }

            set
            {
                blurIterations = value > 0 ? value : 0;
            }
        }

        public BlurType BlurType
        {
            get
            {
                return blurType;
            }

            set
            {
                blurType = value;
            }
        }

        [Obsolete("Fixed incorrect spelling. Use DilateIterations instead")]
        public int DilateIterration
        {
            get
            {
                return DilateIterations;
            }

            set
            {
                DilateIterations = value;
            }
        }

        public int DilateIterations
        {
            get
            {
                return dilateIterations;
            }

            set
            {
                dilateIterations = value > 0 ? value : 0;
            }
        }

        public RenderTargetIdentifier GetRenderTarget(OutlineParameters parameters)
        {
            return TargetsHolder.Instance.GetTarget(parameters, renderTargetName);
        }

        private void OnValidate()
        {
            if (blurIterations < 0)
                blurIterations = 0;

            if (dilateIterations < 0)
                dilateIterations = 0;
        }

        private void OnEnable()
        {
            if (targetCamera == null)
                targetCamera = GetComponent<Camera>();

            targetCamera.forceIntoRenderTexture = targetCamera.stereoTargetEye == StereoTargetEyeMask.None || !UnityEngine.XR.XRSettings.enabled;

#if UNITY_EDITOR
            outliners.Add(this);
#endif

            parameters.CheckInitialization();
            parameters.Buffer.name = "Outline";

#if UNITY_EDITOR
            editorPreviewParameters.CheckInitialization();

            editorPreviewParameters.Buffer.name = "Editor outline";
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            GameObject.DestroyImmediate(editorPreviewParameters.BlitMesh);
            if (editorPreviewParameters.Buffer != null)
                editorPreviewParameters.Buffer.Dispose();
#endif

            GameObject.DestroyImmediate(parameters.BlitMesh);

            if (parameters.Buffer != null)
                parameters.Buffer.Dispose();
        }

        private void OnDisable()
        {
            if (targetCamera != null)
                UpdateBuffer(targetCamera, parameters.Buffer, true);

#if UNITY_EDITOR
            RemoveFromAllSceneViews();

            outliners.Remove(this);
#endif

#if UNITY_EDITOR
            foreach (var view in UnityEditor.SceneView.sceneViews)
            {
                var viewToUpdate = (UnityEditor.SceneView)view;

                viewToUpdate.camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, editorPreviewParameters.Buffer);
                viewToUpdate.camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, editorPreviewParameters.Buffer);
            }
#endif
        }

        private void UpdateBuffer(Camera targetCamera, CommandBuffer buffer, bool removeOnly)
        {
            targetCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, buffer);
            targetCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, buffer);
            if (removeOnly)
                return;

            targetCamera.AddCommandBuffer(Event, buffer);
        }

        private void OnPreRender()
        {
            if (GraphicsSettings.renderPipelineAsset != null)
                return;

            parameters.OutlinablesToRender.Clear();
            SetupOutline(targetCamera, parameters, false);
        }

        private void SetupOutline(Camera cameraToUse, OutlineParameters parametersToUse, bool isEditor)
        {
            UpdateBuffer(cameraToUse, parametersToUse.Buffer, false);
            UpdateParameters(parametersToUse, cameraToUse, isEditor);

            parametersToUse.Buffer.Clear();
            if (renderingStrategy == OutlineRenderingStrategy.Default)
            {
                OutlineEffect.SetupOutline(parametersToUse);
                parametersToUse.BlitMesh = null;
                parametersToUse.MeshPool.ReleaseAllMeshes();
            }
            else
            {
                temporaryOutlinables.Clear();
                temporaryOutlinables.AddRange(parametersToUse.OutlinablesToRender);

                parametersToUse.OutlinablesToRender.Clear();
                parametersToUse.OutlinablesToRender.Add(null);

                foreach (var outlinable in temporaryOutlinables)
                {
                    parametersToUse.OutlinablesToRender[0] = outlinable;
                    OutlineEffect.SetupOutline(parametersToUse);
                    parametersToUse.BlitMesh = null;
                }

                parametersToUse.MeshPool.ReleaseAllMeshes();
            }
        }

#if UNITY_EDITOR
        private void RemoveFromAllSceneViews()
        {
            foreach (var view in UnityEditor.SceneView.sceneViews)
            {
                var viewToUpdate = (UnityEditor.SceneView)view;
                var eventTransferer = viewToUpdate.camera.GetComponent<OnPreRenderEventTransferer>();
                if (eventTransferer != null)
                    eventTransferer.OnPreRenderEvent -= UpdateEditorCamera;

                viewToUpdate.camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, editorPreviewParameters.Buffer);
                viewToUpdate.camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, editorPreviewParameters.Buffer);
            }
        }

        private void LateUpdate()
        {
            if (lastSelectedOutliner == null && outliners.Count > 0)
                lastSelectedOutliner = outliners[0].gameObject;

            var isSelected = Array.Find(UnityEditor.Selection.gameObjects, x => x == gameObject) ?? lastSelectedOutliner != null;
            if (isSelected)
                lastSelectedOutliner = gameObject;

            foreach (var view in UnityEditor.SceneView.sceneViews)
            {
                var viewToUpdate = (UnityEditor.SceneView)view;
                var eventTransferer = viewToUpdate.camera.GetComponent<OnPreRenderEventTransferer>();
                if (eventTransferer != null)
                    eventTransferer.OnPreRenderEvent -= UpdateEditorCamera;

                UpdateBuffer(viewToUpdate.camera, editorPreviewParameters.Buffer, true);
            }

            if (!isSelected)
                return;

            foreach (var view in UnityEditor.SceneView.sceneViews)
            {
                var viewToUpdate = (UnityEditor.SceneView)view;
                if (!viewToUpdate.sceneViewState.showImageEffects)
                    continue;

                var eventTransferer = viewToUpdate.camera.GetComponent<OnPreRenderEventTransferer>();
                if (eventTransferer == null)
                    eventTransferer = viewToUpdate.camera.gameObject.AddComponent<OnPreRenderEventTransferer>();

                eventTransferer.OnPreRenderEvent += UpdateEditorCamera;
            }
        }

        private void UpdateEditorCamera(Camera camera)
        {
            SetupOutline(camera, editorPreviewParameters, true);
        }
#endif

        public void UpdateSharedParameters(OutlineParameters parameters, Camera camera, bool editorCamera)
        {
            parameters.DilateQuality = DilateQuality;
            parameters.ScaleIndependent = scaleIndepented;
            parameters.Camera = camera;
            parameters.IsEditorCamera = editorCamera;
            parameters.PrimaryBufferScale = primaryRendererScale;
            parameters.InfoBufferScale = infoRendererScale;
            parameters.BlurIterations = blurIterations;
            parameters.BlurType = blurType;
            parameters.DilateIterations = dilateIterations;
            parameters.BlurShift = blurShift;
            parameters.DilateShift = dilateShift;
            parameters.UseHDR = camera.allowHDR && (RenderingMode == RenderingMode.HDR);
            parameters.EyeMask = camera.stereoTargetEye;

            parameters.OutlineLayerMask = outlineLayerMask;

            parameters.Prepare();
        }

        private void UpdateParameters(OutlineParameters parameters, Camera camera, bool editorCamera)
        {
            UpdateSharedParameters(parameters, camera, editorCamera);

            parameters.DepthTarget = RenderTargetUtility.ComposeTarget(parameters, BuiltinRenderTextureType.CameraTarget);

            var targetTexture = camera.targetTexture == null ? camera.activeTexture : camera.targetTexture;

            if (UnityEngine.XR.XRSettings.enabled
                && !parameters.IsEditorCamera
                && parameters.EyeMask != StereoTargetEyeMask.None)
            {
                var descriptor = UnityEngine.XR.XRSettings.eyeTextureDesc;
                parameters.TargetWidth = descriptor.width;
                parameters.TargetHeight = descriptor.height;
            }
            else
            {
                parameters.TargetWidth = targetTexture != null ? targetTexture.width : camera.scaledPixelWidth;
                parameters.TargetHeight = targetTexture != null ? targetTexture.height : camera.scaledPixelHeight;
            }

            parameters.Antialiasing = editorCamera ? (targetTexture == null ? 1 : targetTexture.antiAliasing) : CameraUtility.GetMSAA(targetCamera);

            parameters.Target = RenderTargetUtility.ComposeTarget(parameters, HasCutomRenderTarget && !editorCamera ? GetRenderTarget(parameters) :
                BuiltinRenderTextureType.CameraTarget);

            Outlinable.GetAllActiveOutlinables(parameters.Camera, parameters.OutlinablesToRender);
            RendererFilteringUtility.Filter(parameters.Camera, parameters);
        }
    }
}