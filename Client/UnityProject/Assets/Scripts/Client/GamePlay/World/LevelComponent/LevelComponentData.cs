using System;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

public class LevelComponentData : IClone<LevelComponentData>
{
    [ReadOnly]
    [HideInEditorMode]
    public GridPos3D WorldGP;

    [ReadOnly]
    [HideInEditorMode]
    public GridPos3D LocalGP;

    public LevelComponentData Clone()
    {
        Type type = GetType();
        LevelComponentData data = (LevelComponentData) Activator.CreateInstance(type);
        data.WorldGP = WorldGP;
        data.LocalGP = LocalGP;
        ChildClone(data);
        return data;
    }

    protected virtual void ChildClone(LevelComponentData newData)
    {
    }
}