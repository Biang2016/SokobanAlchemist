using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityActiveSkill_SummonDropBox : EntityActiveSkill_AreaCast
{
    protected override string Description => "天降箱子";

    [LabelText("箱子类型概率")]
    public List<BoxNameWithProbability> DropBoxList = new List<BoxNameWithProbability>();

    [LabelText("箱子起落高度")]
    public int DropFromHeightFromFloor = 1;

    protected override IEnumerator Cast(float castDuration)
    {
        foreach (GridPos3D gp in RealSkillEffectGPs)
        {
            BoxNameWithProbability randomResult = CommonUtils.GetRandomWithProbabilityFromList(DropBoxList);
            if (randomResult != null)
            {
                ushort boxTypeIndex = ConfigManager.GetTypeIndex( TypeDefineType.Box,randomResult.BoxTypeName.TypeName);
                if (boxTypeIndex != 0)
                {
                    if (WorldManager.Instance.CurrentWorld.DropBoxOnTopLayer(boxTypeIndex, randomResult.BoxOrientation, GridPos3D.Down, gp + GridPos3D.Up * DropFromHeightFromFloor, DropFromHeightFromFloor + 3, out Box dropBox))
                    {
                        if (Entity is Actor actor)
                        {
                            dropBox.LastInteractActorGUID = actor.GUID;
                        }
                    }
                }
            }
        }

        yield return base.Cast(castDuration);
    }

    protected override void ChildClone(EntityActiveSkill cloneData)
    {
        base.ChildClone(cloneData);
        EntityActiveSkill_SummonDropBox newEAS = (EntityActiveSkill_SummonDropBox) cloneData;
        newEAS.DropBoxList = DropBoxList.Clone();
        newEAS.DropFromHeightFromFloor = DropFromHeightFromFloor;
    }

    public override void CopyDataFrom(EntityActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        EntityActiveSkill_SummonDropBox srcEAS = (EntityActiveSkill_SummonDropBox) srcData;
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