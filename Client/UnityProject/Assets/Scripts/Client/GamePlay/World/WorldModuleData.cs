using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;

public class WorldModuleData : IClone<WorldModuleData>
{
    #region ConfigData

    public WorldModuleType WorldModuleType;

    /// <summary>
    /// 世界模组制作规范，一个模组容量为16x16x16
    /// 模组上下层叠，底部模组Y为0，顶部Y为15
    /// </summary>
    public byte[,,] BoxMatrix = new byte[WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE, WorldModule.MODULE_SIZE];

    public byte Y;

    public WorldModuleData Clone()
    {
        WorldModuleData data = new WorldModuleData();
        data.WorldModuleType = WorldModuleType;
        for (int x = 0; x < BoxMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < BoxMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < BoxMatrix.GetLength(2); z++)
                {
                    data.BoxMatrix[x, y, z] = BoxMatrix[x, y, z];
                }
            }
        }

        data.Y = Y;
        return data;
    }

    #endregion
}