using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;

public class World : PoolObject
{
    public WorldData WorldData;
    public List<WorldModule> WorldModules = new List<WorldModule>();

    public void Initialize(WorldData worldData)
    {
        WorldData = worldData;
        for (int x = 0; x < worldData.ModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < worldData.ModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < worldData.ModuleMatrix.GetLength(2); z++)
                {
                    WorldModuleType wmType = (WorldModuleType) worldData.ModuleMatrix[x, y, z];
                    if (wmType != WorldModuleType.None)
                    {
                        WorldModule wm = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.WorldModule].AllocateGameObject<WorldModule>(transform);
                        wm.name = $"WorldModule({x}, {y}, {z})";
                        WorldModules.Add(wm);
                        WorldModuleData data = ConfigManager.Instance.GetWorldModuleDataConfig(wmType);
                        GridPos3D.ApplyGridPosToLocalTrans(new GridPos3D(x, y, z), wm.transform, 16);
                        wm.Initialize(data);
                    }
                }
            }
        }
    }
}

public enum WorldType
{
    None = 0,
    SampleWorld = 1,

    Max = 255
}