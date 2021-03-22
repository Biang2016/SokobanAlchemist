using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace EPOOutline
{
    public enum CutoutDescriptionType
    {
        None,
        Hash
    }

    public enum RendererType
    {
        Unknown,
        MeshRenderer,
        SkinnedMeshRenderer,
        Other
    }

    [System.Serializable]
    public class OutlineTarget
    {
        [SerializeField]
        private float edgeDilateAmount = 5.0f;

        [SerializeField]
        private float frontEdgeDilateAmount = 5.0f;

        [SerializeField]
        private float backEdgeDilateAmount = 5.0f;

        [SerializeField]
        [FormerlySerializedAs("Renderer")]
        public Renderer renderer;

        [SerializeField]
        private RendererType rendererType;

        [SerializeField]
        public int SubmeshIndex;

        [SerializeField]
        public BoundsMode BoundsMode = BoundsMode.Default;

        [SerializeField]
        public Bounds Bounds = new Bounds(Vector3.zero, Vector3.one);

        [SerializeField]
        public CutoutDescriptionType CutoutDescriptionType;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        public float CutoutThreshold = 0.5f;

        [SerializeField]
        public CullMode CullMode;

        [SerializeField]
        private string cutoutTextureName;

        [SerializeField]
        public DilateRenderMode DilateRenderingMode;

        [SerializeField]
        private int cutoutTextureIndex;
        
        private int? cutoutTextureId;

        private bool rendererIsNotNull;

        private bool readyToRender;

        public bool RendererIsNotNull
        {
            get
            {
                return rendererIsNotNull;
            }
        }

        public Renderer Renderer
        {
            get
            {
                return renderer;
            }
        }

        public RendererType RendererType
        {
            get
            {
                return rendererType;
            }
        }

        public bool ReadyToRender
        {
            get
            {
                return readyToRender;
            }
        }

        public bool UsesCutout
        {
            get
            {
                return !string.IsNullOrEmpty(cutoutTextureName);
            }
        }

        public int CutoutTextureIndex
        {
            get
            {
                return cutoutTextureIndex;
            }

            set
            {
                cutoutTextureIndex = value;
                if (cutoutTextureIndex < 0)
                {
                    Debug.LogError("Trying to set cutout texture index less than zero");
                    cutoutTextureIndex = 0;
                }
            }
        }

        public bool CanUseEdgeDilateShift
        {
            get
            {
                return !UsesCutout && (renderer is MeshRenderer || renderer is SkinnedMeshRenderer) && (rendererIsNotNull && !renderer.isPartOfStaticBatch && !renderer.gameObject.isStatic);
            }
        }

        public int ShiftedSubmeshIndex
        {
            get
            {
                return SubmeshIndex;
            }
        }

        public int CutoutTextureId
        {
            get
            {
                if (!cutoutTextureId.HasValue)
                    cutoutTextureId = Shader.PropertyToID(cutoutTextureName);

                return cutoutTextureId.Value;
            }
        }

        public string CutoutTextureName
        {
            get
            {
                return cutoutTextureName;
            }

            set
            {
                cutoutTextureName = value;
                cutoutTextureId = null;
            }
        }

        public float EdgeDilateAmount
        {
            get
            {
                return edgeDilateAmount;
            }

            set
            {
                if (value < 0)
                    edgeDilateAmount = 0;
                else
                    edgeDilateAmount = value;
            }
        }

        public float FrontEdgeDilateAmount
        {
            get
            {
                return frontEdgeDilateAmount;
            }

            set
            {
                if (value < 0)
                    frontEdgeDilateAmount = 0;
                else
                    frontEdgeDilateAmount = value;
            }
        }

        public float BackEdgeDilateAmount
        {
            get
            {
                return backEdgeDilateAmount;
            }

            set
            {
                if (value < 0)
                    backEdgeDilateAmount = 0;
                else
                    backEdgeDilateAmount = value;
            }
        }

        public OutlineTarget()
        {

        }

        public OutlineTarget(Renderer renderer, int submesh = 0)
        {
            SubmeshIndex = submesh;
            this.renderer = renderer;
            UpdateRendererType();

            CutoutDescriptionType = CutoutDescriptionType.None;
            CutoutThreshold = 0.5f;
            cutoutTextureId = null;
            cutoutTextureName = string.Empty;
            CullMode = renderer is SpriteRenderer ? CullMode.Off : CullMode.Back;
            DilateRenderingMode = DilateRenderMode.PostProcessing;
            frontEdgeDilateAmount = 5.0f;
            backEdgeDilateAmount = 5.0f;
            edgeDilateAmount = 5.0f;
        }

        public OutlineTarget(Renderer renderer, string cutoutTextureName, float cutoutThreshold = 0.5f)
        {
            SubmeshIndex = 0;
            this.renderer = renderer;

            UpdateRendererType();

            CutoutDescriptionType = CutoutDescriptionType.Hash;
            cutoutTextureId = Shader.PropertyToID(cutoutTextureName);
            CutoutThreshold = cutoutThreshold;
            this.cutoutTextureName = cutoutTextureName;
            CullMode = renderer is SpriteRenderer ? CullMode.Off : CullMode.Back;
            DilateRenderingMode = DilateRenderMode.PostProcessing;
            frontEdgeDilateAmount = 5.0f;
            backEdgeDilateAmount = 5.0f;
            edgeDilateAmount = 5.0f;
        }

        public void UpdateRendererType()
        {
            if (rendererType != RendererType.Unknown)
                return;

            if (renderer is SkinnedMeshRenderer)
                rendererType = RendererType.SkinnedMeshRenderer;
            else if (renderer is MeshRenderer)
                rendererType = RendererType.MeshRenderer;
            else
                rendererType = RendererType.Other;
        }

        public void UpdateReadyToRender()
        {
            rendererIsNotNull = renderer != null;
            readyToRender = rendererIsNotNull && renderer.enabled && renderer.gameObject.activeInHierarchy;
        }
    }
}