using System;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;

public class WorldData : IClone<WorldData>
{
    #region ConfigData

    public string WorldName;

    public WorldFeature WorldFeature;

    /// <summary>
    /// 世界制作规范，世界最大范围为16x16x8个模组
    /// </summary>
    public byte[,,] ModuleMatrix = new byte[World.WORLD_SIZE, World.WORLD_SIZE, World.WORLD_HEIGHT];

    public WorldActorData WorldActorData = new WorldActorData();
    public WorldCameraPOIData WorldCameraPOIData = new WorldCameraPOIData();

    public WorldData Clone()
    {
        WorldData data = new WorldData();
        data.WorldName = WorldName;
        data.WorldFeature = WorldFeature;
        for (int x = 0; x < ModuleMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < ModuleMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < ModuleMatrix.GetLength(2); z++)
                {
                    data.ModuleMatrix[x, y, z] = ModuleMatrix[x, y, z];
                }
            }
        }

        data.WorldActorData = WorldActorData.Clone();
        data.WorldCameraPOIData = WorldCameraPOIData.Clone();
        return data;
    }

    #endregion
}

[Flags]
public enum WorldFeature
{
    None = 0,

    [LabelText("玩家无敌")]
    PlayerImmune = 1 << 0,
}