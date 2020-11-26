using System;
using System.Collections.Generic;
using BiangStudio;
using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;

[Serializable]
public class ActorActiveSkill_SummonDropBox : ActorActiveSkill_AreaCast
{
    protected override string Description => "天降箱子";

    [BoxNameList]
    [LabelText("箱子类型概率")]
    public List<BoxNameWithProbability> DropBoxList = new List<BoxNameWithProbability>();

    [LabelText("箱子起落高度")]
    public int DropFromHeightFromFloor = 1;

    protected override void Cast()
    {
        base.Cast();
        foreach (GridPos3D gp in RealSkillEffectGPs)
        {
            BoxNameWithProbability randomResult = CommonUtils.GetRandomWithProbabilityFromList(DropBoxList);
            if (randomResult != null)
            {
                ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(randomResult.BoxTypeName);
                if (boxTypeIndex != 0)
                {
                    if (WorldManager.Instance.CurrentWorld.DropBoxOnTopLayer(boxTypeIndex, GridPos3D.Down, gp + GridPos3D.Up * DropFromHeightFromFloor, DropFromHeightFromFloor + 3, out Box dropBox))
                    {
                        dropBox.LastTouchActor = Actor;
                    }
                }
            }
        }
    }

    protected override void ChildClone(ActorActiveSkill cloneData)
    {
        base.ChildClone(cloneData);
        ActorActiveSkill_SummonDropBox newAAS = (ActorActiveSkill_SummonDropBox) cloneData;
        newAAS.DropBoxList = DropBoxList.Clone();
        newAAS.DropFromHeightFromFloor = DropFromHeightFromFloor;
    }

    public override void CopyDataFrom(ActorActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_SummonDropBox srcAAS = (ActorActiveSkill_SummonDropBox) srcData;
        DropBoxList = srcAAS.DropBoxList.Clone();
        DropFromHeightFromFloor = srcAAS.DropFromHeightFromFloor;
    }
}