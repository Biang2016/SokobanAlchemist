using System.Collections;
using System.Collections.Generic;
using BiangLibrary.ObjectPool;
using UnityEngine;

public class BoxMarchingTextureTile : PoolObject
{
    [SerializeField]
    private MeshRenderer MeshRenderer;

    internal MarchingTextureCase MarchingTextureCase;

    private MaterialPropertyBlock mpb;

    public void Initialize(MarchingSquareData data)
    {
        TerrainType terrain_0 = TerrainType.Earth;
        if (mpb == null) mpb = new MaterialPropertyBlock();
        MarchingTextureCase = data.GetMarchingTextureCase();
        Dictionary<MarchingTextureCase, Texture> dict = ConfigManager.TerrainMarchingTextureDict[data.TransitTerrain];
        if (dict.TryGetValue(MarchingTextureCase, out Texture texture))
        {
            MeshRenderer.GetPropertyBlock(mpb);
            mpb.SetTexture("_Albedo", texture);
            MeshRenderer.SetPropertyBlock(mpb);
        }
    }

    public struct MarchingSquareData
    {
        public TerrainType BasicTerrain;
        public TerrainType TransitTerrain;

        public TerrainType Terrain_LB;
        public TerrainType Terrain_RB;
        public TerrainType Terrain_RT;
        public TerrainType Terrain_LT;

        public bool ForceEmpty;

        public MarchingSquareData(TerrainType basicTerrain, TerrainType terrainLB, TerrainType terrainRB, TerrainType terrainRT, TerrainType terrainLT) : this()
        {
            BasicTerrain = basicTerrain;
            Terrain_LB = terrainLB;
            Terrain_RB = terrainRB;
            Terrain_RT = terrainRT;
            Terrain_LT = terrainLT;
            InitData();
        }

        public bool LB => Terrain_LB == TransitTerrain;
        public bool RB => Terrain_RB == TransitTerrain;
        public bool RT => Terrain_RT == TransitTerrain;
        public bool LT => Terrain_LT == TransitTerrain;

        public MarchingTextureCase GetMarchingTextureCase()
        {
            return (MarchingTextureCase) (((LB ? 1 : 0) << 0) + ((RB ? 1 : 0) << 1) + ((RT ? 1 : 0) << 2) + ((LT ? 1 : 0) << 3));
        }

        public bool InitData()
        {
            ForceEmpty = false;
            TransitTerrain = TerrainType.Earth;
            if (Terrain_LB != TerrainType.Earth)
            {
                if (TransitTerrain != TerrainType.Earth && TransitTerrain != Terrain_LB) ForceEmpty = true;
                TransitTerrain = Terrain_LB;
            }

            if (Terrain_RB != TerrainType.Earth)
            {
                if (TransitTerrain != TerrainType.Earth && TransitTerrain != Terrain_RB) ForceEmpty = true;
                TransitTerrain = Terrain_RB;
            }

            if (Terrain_RT != TerrainType.Earth)
            {
                if (TransitTerrain != TerrainType.Earth && TransitTerrain != Terrain_RT) ForceEmpty = true;
                TransitTerrain = Terrain_RT;
            }

            if (Terrain_LT != TerrainType.Earth)
            {
                if (TransitTerrain != TerrainType.Earth && TransitTerrain != Terrain_LT) ForceEmpty = true;
                TransitTerrain = Terrain_LT;
            }

            if (ForceEmpty) TransitTerrain = TerrainType.Earth;
            return ForceEmpty;
        }
    }
}