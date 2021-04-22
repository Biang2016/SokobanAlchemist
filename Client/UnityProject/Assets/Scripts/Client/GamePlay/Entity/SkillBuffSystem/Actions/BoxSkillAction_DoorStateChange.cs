using System;
using Sirenix.OdinInspector;

[Serializable]
public class BoxSkillAction_DoorStateChange : BoxSkillAction, EntitySkillAction.IPureAction
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

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxSkillAction_DoorStateChange action = ((BoxSkillAction_DoorStateChange) newAction);
        action.ToggleDoor = ToggleDoor;
        action.ChangeDoorStateTo = ChangeDoorStateTo;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxSkillAction_DoorStateChange action = ((BoxSkillAction_DoorStateChange) srcData);
        ToggleDoor = action.ToggleDoor;
        ChangeDoorStateTo = action.ChangeDoorStateTo;
    }
}