using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

public class WorldData : IClone<WorldData>
{
    public ushort WorldTypeIndex;
    public string WorldTypeName;

    public WorldFeature WorldFeature;
    public string DefaultWorldActorBornPointAlias;

    /// <summary>
    /// 世界制作规范，世界最大范围为16x16x8个模组
    /// </summary>
    public ushort[,,] ModuleMatrix = new ushort[World.WORLD_SIZE, World.WORLD_HEIGHT, World.WORLD_SIZE];

    public WorldBornPointGroupData_Runtime WorldBornPointGroupData_Runtime = new WorldBornPointGroupData_Runtime();

    public List<GridPos3D> WorldModuleGPOrder = new List<GridPos3D>();

    public bool UseSpecialPlayerEnterESPS = false;
    public EntityStatPropSet Raw_PlayerEnterESPS = new EntityStatPropSet(); // 干数据

    public FieldCamera.CameraConfigData CameraConfigData = new FieldCamera.CameraConfigData();

    public TypeSelectHelper SkyBoxType = new TypeSelectHelper {TypeDefineType = TypeDefineType.SkyBox};
    public TypeSelectHelper PostProcessingProfileType = new TypeSelectHelper {TypeDefineType = TypeDefineType.PostProcessingProfile};

    public WorldData Clone()
    {
        WorldData data = new WorldData();
        data.WorldTypeIndex = WorldTypeIndex;
        data.WorldTypeName = WorldTypeName;
        data.WorldFeature = WorldFeature;
        data.DefaultWorldActorBornPointAlias = DefaultWorldActorBornPointAlias;
        for (int x = 0; x < World.WORLD_SIZE; x++)
        {
            for (int y = 0; y < World.WORLD_HEIGHT; y++)
            {
                for (int z = 0; z < World.WORLD_SIZE; z++)
                {
                    data.ModuleMatrix[x, y, z] = ModuleMatrix[x, y, z];
                }
            }
        }

        data.WorldModuleGPOrder = WorldModuleGPOrder.Clone<GridPos3D, GridPos3D>();
        data.UseSpecialPlayerEnterESPS = UseSpecialPlayerEnterESPS;
        Raw_PlayerEnterESPS.ApplyDataTo(data.Raw_PlayerEnterESPS);
        CameraConfigData.ApplyTo(data.CameraConfigData, true);
        data.SkyBoxType = SkyBoxType.Clone();
        data.PostProcessingProfileType = PostProcessingProfileType.Clone();
        return data;
    }
}

[Flags]
public enum WorldFeature
{
    None = 0,

    [LabelText("玩家无敌")]
    PlayerImmune = 1 << 0,

    [LabelText("PVP")]
    PVP = 1 << 1,

    [LabelText("开放世界")]
    OpenWorld = 1 << 2,
}