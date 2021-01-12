using System;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

[Serializable]
public class BoxPassiveSkill_ChangeBoxType : BoxPassiveSkill_InvokeOnLevelEventID
{
    protected override string Description => "更改箱子类型";

    [BoxName]
    [LabelText("更改箱子类型为")]
    [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DropdownTitle = "选择箱子类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string ChangeBoxTypeTo = "None";

    protected override void OnEventExecute()
    {
        if (Box.State == Box.States.Static)
        {
            WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(Box.WorldGP);
            if (module != null)
            {
                GridPos3D localGP = Box.LocalGP;
                Box.DestroyBox();
                ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(ChangeBoxTypeTo);
                if (boxTypeIndex != 0) module.GenerateBox(boxTypeIndex, localGP);
            }
        }
    }

    protected override void ChildClone(BoxPassiveSkill newBF)
    {
        base.ChildClone(newBF);
        BoxPassiveSkill_ChangeBoxType bf = ((BoxPassiveSkill_ChangeBoxType) newBF);
        bf.ChangeBoxTypeTo = ChangeBoxTypeTo;
    }

    public override void CopyDataFrom(BoxPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkill_ChangeBoxType bf = ((BoxPassiveSkill_ChangeBoxType) srcData);
        ChangeBoxTypeTo = bf.ChangeBoxTypeTo;
    }
}