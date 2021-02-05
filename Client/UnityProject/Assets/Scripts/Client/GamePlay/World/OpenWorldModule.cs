using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using BiangLibrary.GameDataFormat.Grid;

public class OpenWorldModule : WorldModule
{
    public override IEnumerator Initialize(WorldModuleData worldModuleData, GridPos3D moduleGP, World world, int loadBoxNumPerFrame, GridPosR.Orientation generatorOrder)
    {
        ModuleGP = moduleGP;
        World = world;
        WorldModuleData = worldModuleData; // 这个Data数据是空的，此函数的目的就是填满这个Data数据

        if (WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.DeadZone))
        {
            WorldDeadZoneTrigger = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldDeadZoneTrigger].AllocateGameObject<WorldDeadZoneTrigger>(WorldModuleTriggerRoot);
            WorldDeadZoneTrigger.name = $"{nameof(WorldDeadZoneTrigger)}_{ModuleGP}";
            WorldDeadZoneTrigger.Initialize(moduleGP);
        }

        if (WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.Wall))
        {
            WorldWallCollider = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldWallCollider].AllocateGameObject<WorldWallCollider>(WorldModuleTriggerRoot);
            WorldWallCollider.name = $"{nameof(WorldWallCollider)}_{ModuleGP}";
            WorldWallCollider.Initialize(moduleGP);
        }

        if (WorldModuleData.WorldModuleFeature.HasFlag(WorldModuleFeature.Ground))
        {
            WorldGroundCollider = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldGroundCollider].AllocateGameObject<WorldGroundCollider>(WorldModuleTriggerRoot);
            WorldGroundCollider.name = $"{nameof(WorldGroundCollider)}_{ModuleGP}";
            WorldGroundCollider.Initialize(moduleGP);
        }

        int loadBoxCount = 0;
        switch (generatorOrder)
        {
            case GridPosR.Orientation.Right:
            {
                for (int x = 0; x < worldModuleData.BoxMatrix.GetLength(0); x++)
                {
                    for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
                    {
                        for (int z = 0; z < worldModuleData.BoxMatrix.GetLength(2); z++)
                        {
                            if (generateBox(x, y, z))
                            {
                                loadBoxCount++;
                                if (loadBoxCount >= loadBoxNumPerFrame)
                                {
                                    loadBoxCount = 0;
                                    yield return null;
                                }
                            }
                        }
                    }
                }

                break;
            }
            case GridPosR.Orientation.Left:
            {
                for (int x = worldModuleData.BoxMatrix.GetLength(0) - 1; x >= 0; x--)
                {
                    for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
                    {
                        for (int z = 0; z < worldModuleData.BoxMatrix.GetLength(2); z++)
                        {
                            if (generateBox(x, y, z))
                            {
                                loadBoxCount++;
                                if (loadBoxCount >= loadBoxNumPerFrame)
                                {
                                    loadBoxCount = 0;
                                    yield return null;
                                }
                            }
                        }
                    }
                }

                break;
            }
            case GridPosR.Orientation.Up:
            {
                for (int z = 0; z < worldModuleData.BoxMatrix.GetLength(2); z++)
                {
                    for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
                    {
                        for (int x = 0; x < worldModuleData.BoxMatrix.GetLength(0); x++)
                        {
                            if (generateBox(x, y, z))
                            {
                                loadBoxCount++;
                                if (loadBoxCount >= loadBoxNumPerFrame)
                                {
                                    loadBoxCount = 0;
                                    yield return null;
                                }
                            }
                        }
                    }
                }

                break;
            }
            case GridPosR.Orientation.Down:
            {
                for (int z = worldModuleData.BoxMatrix.GetLength(2) - 1; z >= 0; z--)
                {
                    for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
                    {
                        for (int x = 0; x < worldModuleData.BoxMatrix.GetLength(0); x++)
                        {
                            if (generateBox(x, y, z))
                            {
                                loadBoxCount++;
                                if (loadBoxCount >= loadBoxNumPerFrame)
                                {
                                    loadBoxCount = 0;
                                    yield return null;
                                }
                            }
                        }
                    }
                }

                break;
            }
        }

        bool generateBox(int x, int y, int z)
        {
            ushort boxTypeIndex = worldModuleData.BoxMatrix[x, y, z];
            GridPosR.Orientation boxOrientation = GridPosR.Orientation.Up;
            if (boxTypeIndex != 0)
            {
                GenerateBox(boxTypeIndex, LocalGPToWorldGP(new GridPos3D(x, y, z)), boxOrientation, false, true, null);
                return true;
            }

            return false;
        }
    }
}