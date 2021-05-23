using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntitySkillAction_SummonSmashDownBox : EntitySkillAction, EntitySkillAction.IWorldGPAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "天降箱子";

    [LabelText("箱子类型概率")]
    public List<BoxNameWithProbability> DropBoxList = new List<BoxNameWithProbability>();

    [LabelText("箱子起落高度")]
    public int DropFromHeightFromFloor = 1;

    public void ExecuteOnWorldGP(GridPos3D worldGP)
    {
        BoxNameWithProbability randomResult = CommonUtils.GetRandomWithProbabilityFromList(DropBoxList);
        if (randomResult != null)
        {
            ushort boxTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.Box, randomResult.BoxTypeName.TypeName);
            if (boxTypeIndex != 0)
            {
                EntityData entityData = new EntityData(boxTypeIndex, randomResult.BoxOrientation);
                entityData.InitStaticLayoutGUID = Entity.CurrentEntityData.InitStaticLayoutGUID;
                entityData.InitWorldModuleGUID = Entity.CurrentEntityData.InitWorldModuleGUID;
                if (WorldManager.Instance.CurrentWorld.DropBoxOnTopLayer(entityData, GridPos3D.Down, worldGP + GridPos3D.Up * DropFromHeightFromFloor, DropFromHeightFromFloor + 3, out Box dropBox))
                {
                    dropBox.LastInteractEntity = Entity;
                }
            }
        }
    }

    protected override void ChildClone(EntitySkillAction cloneData)
    {
        base.ChildClone(cloneData);
        EntitySkillAction_SummonSmashDownBox newEAS = (EntitySkillAction_SummonSmashDownBox) cloneData;
        newEAS.DropBoxList = DropBoxList.Clone<BoxNameWithProbability, BoxNameWithProbability>();
        newEAS.DropFromHeightFromFloor = DropFromHeightFromFloor;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_SummonSmashDownBox srcEAS = (EntitySkillAction_SummonSmashDownBox) srcData;
        if (DropBoxList.Count != srcEAS.DropBoxList.Count)
        {
            Debug.LogError("EntityActiveSkill_SummonDropBox CopyDataFrom DropBoxList数量不一致");
        }
        else
        {
            for (int i = 0; i < DropBoxList.Count; i++)
            {
                DropBoxList[i].CopyDataFrom(srcEAS.DropBoxList[i]);
            }
        }

        DropFromHeightFromFloor = srcEAS.DropFromHeightFromFloor;
    }
}