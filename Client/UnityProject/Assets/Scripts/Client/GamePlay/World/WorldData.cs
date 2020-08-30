using System.Collections.Generic;
using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;

public class WorldData : IClone<WorldData>
{
    #region ConfigData

    public WorldType WorldType;

    /// <summary>
    /// 世界制作规范，世界最大范围为16x16x8个模组
    /// </summary>
    public byte[,,] ModuleMatrix = new byte[World.WORLD_SIZE, World.WORLD_SIZE, World.WORLD_HEIGHT];

    public byte Y;
    public WorldActorData WorldActorData = new WorldActorData();

    public WorldData Clone()
    {
        WorldData data = new WorldData();
        data.WorldType = WorldType;
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

        data.Y = Y;
        data.WorldActorData = WorldActorData.Clone();
        return data;
    }

    #endregion
}