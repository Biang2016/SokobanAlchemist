using System.Collections;
using BiangLibrary.GameDataFormat.Grid;

public class OpenWorldModule : WorldModule
{
    // 大世界生成的子任务即：使用一个随机数种子，以及若干调节参数，能够生成一个确定的关卡模组，16x16x16，先完成这个任务

    public override IEnumerator Initialize(WorldModuleData worldModuleData, GridPos3D moduleGP, World world, int loadBoxNumPerFrame)
    {
        ModuleGP = moduleGP;
        World = world;
        WorldModuleData = worldModuleData; // 这个Data数据是空的，此函数的目的就是填满这个Data数据

        int loadBoxCount = 0;
        for (int x = 0; x < worldModuleData.BoxMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < worldModuleData.BoxMatrix.GetLength(1); y++)
            {
                for (int z = 0; z < worldModuleData.BoxMatrix.GetLength(2); z++)
                {
                    if (y == 0) // 暂时只生成底层
                    {
                        ushort boxTypeIndex = worldModuleData.BoxMatrix[x, y, z];
                        GridPosR.Orientation boxOrientation = GridPosR.Orientation.Up;
                        if (boxTypeIndex != 0)
                        {
                            GenerateBox(boxTypeIndex, LocalGPToWorldGP(new GridPos3D(x, y, z)), boxOrientation, false, true, null);
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
        }
    }
}