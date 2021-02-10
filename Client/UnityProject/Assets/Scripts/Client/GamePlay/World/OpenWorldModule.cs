using System.Collections;
using BiangLibrary.GameDataFormat.Grid;

public class OpenWorldModule : WorldModule
{
    public override IEnumerator Initialize(WorldModuleData worldModuleData, GridPos3D moduleGP, World world, int loadBoxNumPerFrame)
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

        for (int x = 0; x < MODULE_SIZE; x++)
        {
            for (int y = 0; y < MODULE_SIZE; y++)
            {
                for (int z = 0; z < MODULE_SIZE; z++)
                {
                    if (generateBox(x, y, z, worldModuleData.BoxOrientationMatrix[x, y, z]))
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

        bool generateBox(int x, int y, int z, GridPosR.Orientation orientation)
        {
            ushort boxTypeIndex = worldModuleData.BoxMatrix[x, y, z];
            if (boxTypeIndex != 0)
            {
                GenerateBox(boxTypeIndex, LocalGPToWorldGP(new GridPos3D(x, y, z)), orientation, false, true, null);
                return true;
            }

            return false;
        }
    }
}