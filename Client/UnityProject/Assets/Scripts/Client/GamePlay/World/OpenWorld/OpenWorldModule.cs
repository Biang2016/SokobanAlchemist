using System.Collections;
using BiangLibrary.GameDataFormat.Grid;

public class OpenWorldModule : WorldModule
{
    public override IEnumerator Initialize(WorldModuleData worldModuleData, GridPos3D moduleGP, World world, int loadEntityNumPerFrame)
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

        int loadEntityCount = 0;
        foreach (TypeDefineType entityType in worldModuleData.EntityDataMatrixKeys)
        {
            for (int x = 0; x < MODULE_SIZE; x++)
            {
                for (int y = 0; y < MODULE_SIZE; y++)
                {
                    for (int z = 0; z < MODULE_SIZE; z++)
                    {
                        GridPos3D localGP = new GridPos3D(x, y, z);
                        EntityData entityData = worldModuleData[entityType, localGP];
                        Entity entity = GenerateEntity(entityData, localGP, false, true);
                        if (entity != null)
                        {
                            loadEntityCount++;
                            if (loadEntityCount >= loadEntityNumPerFrame)
                            {
                                loadEntityCount = 0;
                                yield return null;
                            }
                        }
                    }
                }
            }
        }
    }
}