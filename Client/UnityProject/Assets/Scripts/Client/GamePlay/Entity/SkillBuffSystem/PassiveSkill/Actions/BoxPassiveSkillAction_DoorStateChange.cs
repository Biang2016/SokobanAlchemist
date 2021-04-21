using System;
using Sirenix.OdinInspector;

[Serializable]
public class BoxPassiveSkillAction_DoorStateChange : BoxPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "更改门开关状态";

    [LabelText("切换开关门状态")]
    public bool ToggleDoor;

    [HideIf("ToggleDoor")]
    [LabelText("设定为开True或关False")]
    public bool ChangeDoorStateTo;

    public void Execute()
    {
        if (Box.DoorBoxHelper != null)
        {
            if (ToggleDoor)
            {
                Box.DoorBoxHelper.Open = !Box.DoorBoxHelper.Open;
            }
            else
            {
                Box.DoorBoxHelper.Open = ChangeDoorStateTo;
            }
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_DoorStateChange action = ((BoxPassiveSkillAction_DoorStateChange) newAction);
        action.ToggleDoor = ToggleDoor;
        action.ChangeDoorStateTo = ChangeDoorStateTo;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_DoorStateChange action = ((BoxPassiveSkillAction_DoorStateChange) srcData);
        ToggleDoor = action.ToggleDoor;
        ChangeDoorStateTo = action.ChangeDoorStateTo;
    }
}