using System;
using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;

public class LevelComponentData : IClone<LevelComponentData>
{
    [ReadOnly]
    [HideInEditorMode]
    [LabelText("从属")]
    public LevelComponentBelongsTo LevelComponentBelongsTo;

    [ReadOnly]
    public GridPos3D WorldGP;

    [ReadOnly]
    public GridPos3D LocalGP;

    public LevelComponentData Clone()
    {
        Type type = GetType();
        LevelComponentData data = (LevelComponentData) Activator.CreateInstance(type);
        data.LevelComponentBelongsTo = LevelComponentBelongsTo;
        data.WorldGP = WorldGP;
        data.LocalGP = LocalGP;
        ChildClone(data);
        return data;
    }

    protected virtual void ChildClone(LevelComponentData newData)
    {
    }
}

public enum LevelComponentBelongsTo
{
    World,
    WorldModule,
}