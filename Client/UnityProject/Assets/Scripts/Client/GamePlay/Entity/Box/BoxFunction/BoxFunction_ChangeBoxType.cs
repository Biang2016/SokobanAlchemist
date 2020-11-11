using System;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;

[Serializable]
public class BoxFunction_ChangeBoxType : BoxFunction_InvokeOnLevelEventID
{
    protected override string BoxFunctionDisplayName => "更改箱子类型";

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
                Box.DeleteSelf();
                ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(ChangeBoxTypeTo);
                if (boxTypeIndex != 0) module.GenerateBox(boxTypeIndex, localGP);
            }
        }
    }

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_ChangeBoxType bf = ((BoxFunction_ChangeBoxType) newBF);
        bf.ChangeBoxTypeTo = ChangeBoxTypeTo;
    }

    public override void CopyDataFrom(BoxFunctionBase srcData)
    {
        base.CopyDataFrom(srcData);
        BoxFunction_ChangeBoxType bf = ((BoxFunction_ChangeBoxType) srcData);
        ChangeBoxTypeTo = bf.ChangeBoxTypeTo;
    }
}