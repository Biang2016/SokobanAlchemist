using System;
using Sirenix.OdinInspector;

[Serializable]
public class BoxPassiveSkillAction_DoorStateChange : BoxPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "更改门开关状态";

    [BoxName]
    [LabelText("开True关False")]
    public bool ChangeDoorStateTo;

    public void Execute()
    {
        if (Box.DoorBoxHelper != null)
        {
            Box.DoorBoxHelper.Open = ChangeDoorStateTo;
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_DoorStateChange action = ((BoxPassiveSkillAction_DoorStateChange) newAction);
        action.ChangeDoorStateTo = ChangeDoorStateTo;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_DoorStateChange action = ((BoxPassiveSkillAction_DoorStateChange) srcData);
    }
}