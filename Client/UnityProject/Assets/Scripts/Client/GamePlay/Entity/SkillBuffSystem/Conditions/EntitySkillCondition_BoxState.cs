using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillCondition_BoxState : EntitySkillCondition, EntitySkillCondition.IPureCondition
{
    [LabelText("合法的Box状态")]
    public List<Box.States> ValidBoxStates = new List<Box.States>();

    public bool OnCheckCondition()
    {
        if (Entity is Box box)
        {
            foreach (Box.States validBoxState in ValidBoxStates)
            {
                if (box.State == validBoxState)
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected override void ChildClone(EntitySkillCondition cloneData)
    {
        EntitySkillCondition_BoxState newCondition = (EntitySkillCondition_BoxState) cloneData;
        newCondition.ValidBoxStates = ValidBoxStates.Clone<Box.States,Box.States>();
    }

    public override void CopyDataFrom(EntitySkillCondition srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillCondition_BoxState srcCondition = (EntitySkillCondition_BoxState) srcData;
        ValidBoxStates = srcCondition.ValidBoxStates.Clone<Box.States, Box.States>();
    }
}