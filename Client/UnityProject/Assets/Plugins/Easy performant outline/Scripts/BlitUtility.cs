using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using UnityEngine.Profiling;
using Unity.Collections;

namespace EPOOutline
{
    public static class BlitUtility
    {
        private static readonly int MainTexHash = Shader.PropertyToID("_MainTex");

        private static Vector4[] normals = new Vector4[]
            {
                new Vector4(-1, -1, -1),
                new Vector4(1, -1, -1),
                new Vector4(1, 1, -1),
                new Vector4(-1, 1, -1),
                new Vector4(-1, 1, 1),
                new Vector4(1, 1, 1),
                new Vector4(1, -1, 1),
                new Vector4(-1, -1, 1)
            };

        private static ushort[] triangles = 
            {
                0, 2, 1,
	            0, 3, 2,
                2, 3, 4,
	            2, 4, 5,
                1, 2, 5,
	            1, 5, 6,
                0, 7, 4,
	            0, 4, 3,
                5, 4, 7,
	            5, 7, 6,
                0, 6, 7,
	            0, 1, 6
            };

        private static Vector4[] tempVertecies =
            {
                new Vector4(-1, -1, -1, 1),
                new Vector4(1, -1, -1, 1),
                new Vector4(1, 1, -1, 1),
                new Vector4(-1, 1, -1, 1),
                new Vector4(-1, 1, 1, 1),
                new Vector4(1, 1, 1, 1),
                new Vector4(1, -1, 1, 1),
                new Vector4(-1, -1, 1, 1)
            };

        private static VertexAttributeDescriptor[] vertexParams =
                new VertexAttributeDescriptor[]
                    {
                        new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
                        new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32),
                        new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
                    };

        private static Vertex[] vertices = new Vertex[4096];
        private static TriangleIndex[] indecies = new TriangleIndex[4096 * 3];

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct Vertex
        {
            public Vector4 Position;
            public Vector3 Normal;
            public Vector2 AdditionalSize;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct TriangleIndex
        {
            public ushort Index;
        }

        private static void UpdateBounds(Renderer renderer, OutlineTarget target)
        {
            if (target.RendererType == RendererType.MeshRenderer)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter.sharedMesh != null)
                    meshFilter.sharedMesh.RecalculateBounds();
            }
            else if (target.RendererType == RendererType.SkinnedMeshRenderer)
            {
                var skinedMeshRenderer = renderer as SkinnedMeshRenderer;
                if (skinedMeshRenderer.sharedMesh != null)
                    skinedMeshRenderer.sharedMesh.RecalculateBounds();
            }
        }

        public static void SetupMesh(OutlineParameters parameters, float baseShift)
        {
            if (parameters.BlitMesh == null)
                parameters.BlitMesh = parameters.MeshPool.AllocateMesh();

            const int numberOfVertices = 8;

            var currentIndex = 0;
            var triangleIndex = 0;
            var expectedCount = 0;
            foreach (var outlinable in parameters.OutlinablesToRender)
            {
                if (outlinable.DrawingMode != OutlinableDrawingMode.Normal)
                    continue;

                foreach (var target in outlinable.OutlineTargets)
                {
                    var renderer = target.Renderer;
                    if (!target.ReadyToRender)
                        continue;

                    expectedCount += numberOfVertices;
                }
            }

            if (vertices.Length < expectedCount)
            {
                Array.Resize(ref vertices, expectedCount * 2);
                Array.Resize(ref indecies, vertices.Length * 3);
            }

            foreach (var outlinable in parameters.OutlinablesToRender)
            {
                if (outlinable.DrawingMode != OutlinableDrawingMode.Normal)
                    continue;

                var frontParameters = outlinable.RenderStyle == RenderStyle.FrontBack ? outlinable.FrontParameters : outlinable.OutlineParameters;
                var backParameters = outlinable.RenderStyle == RenderStyle.FrontBack ? outlinable.BackParameters : outlinable.OutlineParameters;

                var useDilateDueToSettings = parameters.UseInfoBuffer && (frontParameters.DilateShift > 0.01f || backParameters.DilateShift > 0.01f) || !parameters.UseInfoBuffer;
                var useBlurDueToSettings = parameters.UseInfoBuffer && (frontParameters.BlurShift > 0.01f || backParameters.BlurShift > 0.01f) || !parameters.UseInfoBuffer;

                foreach (var target in outlinable.OutlineTargets)
                {
                    var renderer = target.Renderer;
                    if (!target.ReadyToRender)
                        continue;

                    var pretransformedBounds = false;
                    var bounds = new Bounds();
                    if (target.BoundsMode == BoundsMode.Manual)
                    {
                        bounds = target.Bounds;
                        var size = bounds.size;
                        var rendererScale = renderer.transform.localScale;
                        size.x /= rendererScale.x;
                        size.y /= rendererScale.y;
                        size.z /= rendererScale.z;
                        bounds.size = size;
                    }
                    else
                    {
                        if (target.BoundsMode == BoundsMode.ForceRecalculate)
                            UpdateBounds(target.Renderer, target);
                        
                        var meshRenderer = renderer as MeshRenderer;
                        var index = target.RendererType != RendererType.MeshRenderer || !target.RendererIsNotNull ? 0 : meshRenderer.subMeshStartIndex + target.SubmeshIndex;
                        var filter = target.RendererType != RendererType.MeshRenderer || !target.RendererIsNotNull ? null : meshRenderer.GetComponent<MeshFilter>();
                        var mesh = filter == null ? null : filter.sharedMesh;

                        if (mesh != null && mesh.subMeshCount > index)
                            bounds = mesh.GetSubMesh(index).bounds;
                        else if (target.RendererIsNotNull)
                        {
                            pretransformedBounds = true;
                            bounds = renderer.bounds;
                        }
                    }

                    var scale = 0.5f;
                    Vector4 boundsSize = bounds.size * scale;
                    boundsSize.w = 1;

                    var boundsCenter = renderer is SpriteRenderer ? Vector4.zero : (Vector4)bounds.center;

                    var additionalScaleToSet = Vector2.zero;
                    if (target.CanUseEdgeDilateShift && target.DilateRenderingMode == DilateRenderMode.EdgeShift)
                        additionalScaleToSet.x = Mathf.Max(target.BackEdgeDilateAmount, target.FrontEdgeDilateAmount);

                    Matrix4x4 transformMatrix = Matrix4x4.identity;
                    Matrix4x4 normalTransformMatrix = Matrix4x4.identity;
                    if (!pretransformedBounds && (target.BoundsMode == BoundsMode.Manual || target.RendererIsNotNull && !renderer.isPartOfStaticBatch))
                    {
                        transformMatrix = target.renderer.transform.localToWorldMatrix;
                        normalTransformMatrix = Matrix4x4.Rotate(renderer.transform.rotation);
                    }

                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)currentIndex };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 2) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 1) };

                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)currentIndex };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 3) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 2) };

                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 2) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 3) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 4) };

                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 2) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 4) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 5) };

                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 1) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 2) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 5) };

                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 1) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 5) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 6) };

                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)currentIndex };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 7) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 4) };

                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)currentIndex };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 4) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 3) };

                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 5) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 4) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 7) };

                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 5) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 7) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 6) };

                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)currentIndex };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 6) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 7) };

                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)currentIndex };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 1) };
                    indecies[triangleIndex++] = new TriangleIndex() { Index = (ushort)(currentIndex + 6) };

                    for (var index = 0; index < numberOfVertices; index++)
                    {
                        var normal = normalTransformMatrix * normals[index];
                        var normal3 = new Vector3(normal.x, normal.y, normal.z);

                        var vert = tempVertecies[index];
                        var scaledVert = new Vector4(vert.x * boundsSize.x, vert.y * boundsSize.y, vert.z * boundsSize.z, 1);

                        vertices[currentIndex++] = new Vertex()
                            {
                                Position = transformMatrix * (boundsCenter + scaledVert),
                                Normal = normal3,
                                AdditionalSize = additionalScaleToSet
                            };
                    }
                }
            }

            var flags = MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices;

            parameters.BlitMesh.SetVertexBufferParams(currentIndex, attributes: vertexParams);
            parameters.BlitMesh.SetVertexBufferData(vertices, 0, 0, currentIndex, 0, flags);
            parameters.BlitMesh.SetIndexBufferParams(triangleIndex, IndexFormat.UInt16);
            parameters.BlitMesh.SetIndexBufferData(indecies, 0, 0, triangleIndex, flags);

            parameters.BlitMesh.subMeshCount = 1;
            parameters.BlitMesh.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndex, MeshTopology.Triangles), flags);
        }

        public static void Blit(OutlineParameters parameters, RenderTargetIdentifier source, RenderTargetIdentifier destination, RenderTargetIdentifier destinationDepth, Material material, CommandBuffer targetBuffer, int pass = -1)
        {
            var buffer = targetBuffer == null ? parameters.Buffer : targetBuffer;
            buffer.SetRenderTarget(destination, destinationDepth);

            buffer.SetGlobalTexture(MainTexHash, source);

            buffer.DrawMesh(parameters.BlitMesh, Matrix4x4.identity, material, 0, pass);
        }

        public static void Draw(OutlineParameters parameters, RenderTargetIdentifier target, RenderTargetIdentifier depth, Material material)
        {
            parameters.Buffer.SetRenderTarget(target, depth);

            parameters.Buffer.DrawMesh(parameters.BlitMesh, Matrix4x4.identity, material, 0, -1);
        }
    }
}