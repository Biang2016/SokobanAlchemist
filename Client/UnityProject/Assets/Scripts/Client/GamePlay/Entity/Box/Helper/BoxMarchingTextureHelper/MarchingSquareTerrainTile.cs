using System.Collections.Generic;
using BiangLibrary.ObjectPool;
using UnityEngine;

public class MarchingSquareTerrainTile : PoolObject
{
    [SerializeField]
    private MeshRenderer MeshRenderer;

    internal MarchingTextureCase MarchingTextureCase;

    private MaterialPropertyBlock mpb;

    public void Initialize(MarchingSquareData data)
    {
        if (mpb == null) mpb = new MaterialPropertyBlock();
        MarchingTextureCase = data.GetMarchingTextureCase();
        int index_x = ((int) MarchingTextureCase) % 4;
        int index_y = ((int) MarchingTextureCase) / 4;
        int textureIndex = 0;
        for (int i = 0; i < ConfigManager.TERRAIN_TYPE_COUNT; i++)
        {
            if (i < (int) data.BasicTerrain)
            {
                textureIndex += ConfigManager.TERRAIN_TYPE_COUNT - i;
            }
            else
            {
                textureIndex += (int) data.TransitTerrain - (int) data.BasicTerrain;
                break;
            }
        }

        mpb.SetFloat("_UV_Index_X", index_x);
        mpb.SetFloat("_UV_Index_Y", index_y);
        mpb.SetInt("_TextureIndex", textureIndex);
        MeshRenderer.SetPropertyBlock(mpb);
    }

    public struct MarchingSquareData
    {
        // BasicTerrain <= TransitTerrain
        public TerrainType BasicTerrain;
        public TerrainType TransitTerrain;

        public TerrainType Terrain_LB;
        public TerrainType Terrain_RB;
        public TerrainType Terrain_RT;
        public TerrainType Terrain_LT;

        public bool ForceEmpty;

        public MarchingSquareData(TerrainType terrainLB, TerrainType terrainRB, TerrainType terrainRT, TerrainType terrainLT) : this()
        {
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

        private static HashSet<TerrainType> temp_TerrainTypeSet = new HashSet<TerrainType>();

        public void InitData()
        {
            ForceEmpty = false;
            temp_TerrainTypeSet.Clear();
            temp_TerrainTypeSet.Add(Terrain_LB);
            temp_TerrainTypeSet.Add(Terrain_RB);
            temp_TerrainTypeSet.Add(Terrain_RT);
            temp_TerrainTypeSet.Add(Terrain_LT);
            if (temp_TerrainTypeSet.Count > 2) // 三种以上混合则强制为Earth过度
            {
                ForceEmpty = true;
                BasicTerrain = TerrainType.Earth;
                TransitTerrain = TerrainType.Earth;
            }
            else if (temp_TerrainTypeSet.Count == 2)
            {
                int index = 0;
                foreach (TerrainType tt in temp_TerrainTypeSet)
                {
                    if (index == 0)
                    {
                        BasicTerrain = tt;
                    }
                    else
                    {
                        TransitTerrain = tt;
                    }

                    index++;
                }

                if ((int) BasicTerrain > (int) TransitTerrain)
                {
                    TerrainType swap = BasicTerrain;
                    BasicTerrain = TransitTerrain;
                    TransitTerrain = swap;
                }

                //if (ConfigManager.TerrainMarchingTextureDict[(int) BasicTerrain, (int) TransitTerrain] == null) // 两种混合，如果配置里面无此类贴图，则强制为Earth过度
                //{
                //    ForceEmpty = true;
                //    BasicTerrain = TerrainType.Earth;
                //    TransitTerrain = TerrainType.Earth;
                //}
            }
            else
            {
                BasicTerrain = Terrain_LB;
                TransitTerrain = Terrain_LB;
            }
        }
    }
}