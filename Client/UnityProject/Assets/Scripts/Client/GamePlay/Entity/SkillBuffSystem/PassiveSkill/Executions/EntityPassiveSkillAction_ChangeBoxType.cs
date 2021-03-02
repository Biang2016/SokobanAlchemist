using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

[Serializable]
public class BoxPassiveSkillAction_ChangeBoxType : BoxPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "更改箱子类型(仅箱子使用)";

    [BoxName]
    [LabelText("更改箱子类型为")]
    [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DropdownTitle = "选择箱子类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string ChangeBoxTypeTo = "None";

    [LabelText("更改的箱子的朝向")]
    public GridPosR.Orientation BoxOrientation;

    [LabelText("每一格都生成一个箱子")]
    public bool ChangeForEveryGrid = false;

    public void Execute()
    {
        GridPos3D worldGP = GridPos3D.Zero;
        if (Box.State == Box.States.Static)
        {
            worldGP = Box.WorldGP;
        }
        else
        {
            worldGP = Box.transform.position.ToGridPos3D();
        }

        ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(ChangeBoxTypeTo);
        if (ChangeForEveryGrid)
        {
            List<GridPos3D> occupations = Box.GetBoxOccupationGPs_Rotated();
            Box.DestroyBox(delegate
            {
                if (boxTypeIndex != 0)
                {
                    foreach (GridPos3D gridPos in occupations)
                    {
                        GridPos3D gridWorldGP = worldGP + gridPos;
                        WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(gridWorldGP);
                        if (module != null)
                        {
                            module.GenerateBox(boxTypeIndex, gridWorldGP, BoxOrientation);
                        }
                    }
                }
            });
        }
        else
        {
            Box.DestroyBox(delegate
            {
                if (boxTypeIndex != 0)
                {
                    WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(Box.WorldGP);
                    if (module != null)
                    {
                        module.GenerateBox(boxTypeIndex, worldGP, BoxOrientation);
                    }
                }
            });
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_ChangeBoxType action = ((BoxPassiveSkillAction_ChangeBoxType) newAction);
        action.ChangeBoxTypeTo = ChangeBoxTypeTo;
        action.BoxOrientation = BoxOrientation;
        action.ChangeForEveryGrid = ChangeForEveryGrid;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_ChangeBoxType action = ((BoxPassiveSkillAction_ChangeBoxType) srcData);
        ChangeBoxTypeTo = action.ChangeBoxTypeTo;
        BoxOrientation = action.BoxOrientation;
        ChangeForEveryGrid = action.ChangeForEveryGrid;
    }
}