using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class BoxMarchingTextureHelper : BoxMonoHelper
{
    public Transform BoxMarchingTextureTileContainer;
    public BoxMarchingTextureTile[,] BoxMarchingTextureTileMatrix = new BoxMarchingTextureTile[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
        for (int y = 0; y < WorldModule.MODULE_SIZE; y++)
        {
            BoxMarchingTextureTileMatrix[x, y]?.PoolRecycle();
            BoxMarchingTextureTileMatrix[x, y] = null;
        }
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }

    private BoxMarchingTextureTile.MarchingSquareData[,] marchingSquareDataMatrix = new BoxMarchingTextureTile.MarchingSquareData[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];

    public void Initialize()
    {
        if (WorldManager.Instance.CurrentWorld is OpenWorld openWorld)
        {
            TerrainType GetTerrainType(int x, int z)
            {
                TerrainType terrainType = TerrainType.Earth;
                if (x < 0 || x >= openWorld.WorldMap_TerrainType.GetLength(0) || z < 0 || z >= openWorld.WorldMap_TerrainType.GetLength(1)) return TerrainType.Earth;
                terrainType = openWorld.WorldMap_TerrainType[x, z];
                return terrainType;
            }

            BoxMarchingTextureTile.MarchingSquareData CalculateMarchingSquareData(int x, int z)
            {
                TerrainType tt_LB = GetTerrainType(x, z);
                TerrainType tt_RB = GetTerrainType(x + 1, z);
                TerrainType tt_RT = GetTerrainType(x + 1, z + 1);
                TerrainType tt_LT = GetTerrainType(x, z + 1);
                BoxMarchingTextureTile.MarchingSquareData data = new BoxMarchingTextureTile.MarchingSquareData(tt_LB, tt_RB, tt_RT, tt_LT);
                return data;
            }

            if (Box.WorldModule != null)
            {
                for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
                for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                {
                    BoxMarchingTextureTile tt = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxMarchingTextureTile].AllocateGameObject<BoxMarchingTextureTile>(BoxMarchingTextureTileContainer);
                    BoxMarchingTextureTileMatrix[x, z] = tt;
                    GridPos3D worldGP = Box.WorldModule.LocalGPToWorldGP(new GridPos3D(x, WorldModule.MODULE_SIZE - 1, z));
                    BoxMarchingTextureTile.MarchingSquareData data = CalculateMarchingSquareData(worldGP.x, worldGP.z);
                    marchingSquareDataMatrix[x, z] = data;
                }

                for (int i = 0; i < 3; i++)
                {
                    for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
                    for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                    {
                        BoxMarchingTextureTile.MarchingSquareData thisData = marchingSquareDataMatrix[x, z];
                        for (int delta_x = -1; delta_x <= 1; delta_x++)
                        for (int delta_z = -1; delta_z <= 1; delta_z++)
                        {
                            if (delta_x == 0 && delta_z == 0) continue;
                            BoxMarchingTextureTile.MarchingSquareData surroundingTTData;
                            if (x + delta_x >= 0 && x + delta_x < WorldModule.MODULE_SIZE && z + delta_z >= 0 && z + delta_z < WorldModule.MODULE_SIZE)
                            {
                                surroundingTTData = marchingSquareDataMatrix[x + delta_x, z + delta_z];
                            }
                            else
                            {
                                GridPos3D worldGP = Box.WorldModule.LocalGPToWorldGP(new GridPos3D(x + delta_x, WorldModule.MODULE_SIZE - 1, z + delta_z));
                                surroundingTTData = CalculateMarchingSquareData(worldGP.x, worldGP.z);
                            }

                            if (surroundingTTData.ForceEmpty)
                            {
                                if (delta_x == -1 && delta_z == -1) thisData.Terrain_LB = TerrainType.Earth;
                                if (delta_x == -1 && delta_z == 0)
                                {
                                    thisData.Terrain_LB = TerrainType.Earth;
                                    thisData.Terrain_LT = TerrainType.Earth;
                                }

                                if (delta_x == -1 && delta_z == 1) thisData.Terrain_LT = TerrainType.Earth;

                                if (delta_x == 0 && delta_z == -1)
                                {
                                    thisData.Terrain_LB = TerrainType.Earth;
                                    thisData.Terrain_RB = TerrainType.Earth;
                                }

                                if (delta_x == 0 && delta_z == 1)
                                {
                                    thisData.Terrain_LT = TerrainType.Earth;
                                    thisData.Terrain_RT = TerrainType.Earth;
                                }

                                if (delta_x == 1 && delta_z == -1) thisData.Terrain_RB = TerrainType.Earth;
                                if (delta_x == 1 && delta_z == 0)
                                {
                                    thisData.Terrain_RB = TerrainType.Earth;
                                    thisData.Terrain_RT = TerrainType.Earth;
                                }

                                if (delta_x == 1 && delta_z == 1) thisData.Terrain_RT = TerrainType.Earth;
                            }
                        }

                        thisData.InitData();
                        marchingSquareDataMatrix[x, z] = thisData;
                    }
                }

                for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
                for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
                {
                    BoxMarchingTextureTile.MarchingSquareData data = marchingSquareDataMatrix[x, z];
                    BoxMarchingTextureTile tt = BoxMarchingTextureTileMatrix[x, z];
                    tt.Initialize(data);
                    tt.transform.localPosition = new Vector3(x, 0, z);
                }
            }
        }
    }
}