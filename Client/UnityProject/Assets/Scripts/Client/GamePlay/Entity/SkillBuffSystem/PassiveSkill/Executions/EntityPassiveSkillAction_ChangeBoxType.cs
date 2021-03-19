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

    [LabelText("更改箱子为")]
    public TypeSelectHelper ChangeBoxTypeToBoxName = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};

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

        ushort boxTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.Box, ChangeBoxTypeToBoxName.TypeName);
        if (ChangeForEveryGrid)
        {
            List<GridPos3D> occupations = Box.GetEntityOccupationGPs_Rotated();
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
                    WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(worldGP);
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
        action.ChangeBoxTypeToBoxName = ChangeBoxTypeToBoxName.Clone();
        action.BoxOrientation = BoxOrientation;
        action.ChangeForEveryGrid = ChangeForEveryGrid;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_ChangeBoxType action = ((BoxPassiveSkillAction_ChangeBoxType) srcData);
        ChangeBoxTypeToBoxName.CopyDataFrom(action.ChangeBoxTypeToBoxName);
        BoxOrientation = action.BoxOrientation;
        ChangeForEveryGrid = action.ChangeForEveryGrid;
    }
}