using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillCondition_TerrainState : EntitySkillCondition, EntitySkillCondition.IPureCondition
{
    [LabelText("合法的TerrainType")]
    public List<TerrainType> ValidTerrainTypes = new List<TerrainType>();

    public bool OnCheckCondition()
    {
        TerrainType currentTerrainType = WorldManager.Instance.CurrentWorld.GetTerrainType(Entity.WorldGP);
        foreach (TerrainType validTerrainType in ValidTerrainTypes)
        {
            if (currentTerrainType == validTerrainType)
            {
                return true;
            }
        }

        return false;
    }

    protected override void ChildClone(EntitySkillCondition cloneData)
    {
        EntitySkillCondition_TerrainState newCondition = (EntitySkillCondition_TerrainState) cloneData;
        newCondition.ValidTerrainTypes = ValidTerrainTypes.Clone<TerrainType, TerrainType>();
    }

    public override void CopyDataFrom(EntitySkillCondition srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillCondition_TerrainState srcCondition = (EntitySkillCondition_TerrainState) srcData;
        ValidTerrainTypes = srcCondition.ValidTerrainTypes.Clone<TerrainType, TerrainType>();
    }
}