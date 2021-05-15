using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class EntitySkillAction_DropBox : EntitySkillAction, EntitySkillAction.IPureAction
{
    [LabelText("箱子类型概率")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<DropItemProbability> DropBoxList = new List<DropItemProbability>();

    public override void OnRecycled()
    {
    }

    protected override string Description => "掉落箱子";

    private static List<ushort> cached_DropBoxIndexList = new List<ushort>(16);

    public void Execute()
    {
        WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(Entity.WorldGP);
        if (module)
        {
            cached_DropBoxIndexList.Clear();
            foreach (DropItemProbability dropItemProbability in DropBoxList)
            {
                ushort boxTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.Box, dropItemProbability.ItemType.TypeName);
                if (boxTypeIndex == 0) continue;
                int count = CommonUtils.GetRandomFromFloatProbability(RandomType.Uniform, dropItemProbability.ProbabilityMin, dropItemProbability.ProbabilityMax);
                for (int i = 0; i < count; i++)
                {
                    cached_DropBoxIndexList.Add(boxTypeIndex);
                }
            }

            WorldManager.Instance.CurrentWorld.ThrowBoxFormWorldGP(cached_DropBoxIndexList, Entity.EntityGeometryCenter.ToGridPos3D(), Entity.InitWorldModuleGUID, Entity.CurrentEntityData.InitStaticLayoutGUID);
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_DropBox action = ((EntitySkillAction_DropBox) newAction);
        action.DropBoxList = DropBoxList.Clone<DropItemProbability, DropItemProbability>();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_DropBox action = ((EntitySkillAction_DropBox) srcData);
        DropBoxList = action.DropBoxList.Clone<DropItemProbability, DropItemProbability>();
    }
}