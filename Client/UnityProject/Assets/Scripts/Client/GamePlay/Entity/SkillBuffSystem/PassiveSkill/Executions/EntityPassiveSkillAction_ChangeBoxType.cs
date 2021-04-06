using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

[Serializable]
public class BoxPassiveSkillAction_ChangeBoxType : BoxPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "更改箱子类型(仅箱子使用)";

    [LabelText("@\"更改箱子为(Box/Enemy)\t\"+ChangeBoxTypeToEntityName")]
    [FormerlySerializedAs("ChangeBoxTypeToBoxName")]
    [ValidateInput("CheckType", "只能在Box或者Enemy中选择")]
    public TypeSelectHelper ChangeBoxTypeToEntityName = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};

    private bool CheckType(TypeSelectHelper value)
    {
        return value.TypeDefineType == TypeDefineType.Box || value.TypeDefineType == TypeDefineType.Enemy;
    }

    [LabelText("生成的朝向")]
    public GridPosR.Orientation ResultOrientation;

    [LabelText("每一格都生成一个")]
    public bool ChangeForEveryGrid = false;

    [LabelText("对象额外数据")]
    public EntityExtraSerializeData RawEntityExtraSerializeData = new EntityExtraSerializeData(); // 干数据，禁修改

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

        ushort entityTypeIndex = ConfigManager.GetTypeIndex(ChangeBoxTypeToEntityName.TypeDefineType, ChangeBoxTypeToEntityName.TypeName);
        EntityExtraSerializeData entityExtraSerializeData = RawEntityExtraSerializeData.Clone();
        if (ChangeForEveryGrid)
        {
            List<GridPos3D> occupations = Box.GetEntityOccupationGPs_Rotated();
            Box.DestroyBox(delegate
            {
                if (entityTypeIndex != 0)
                {
                    foreach (GridPos3D gridPos in occupations)
                    {
                        GridPos3D gridWorldGP = worldGP + gridPos;
                        WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(gridWorldGP);
                        if (module != null)
                        {
                            if (ChangeBoxTypeToEntityName.TypeDefineType == TypeDefineType.Box)
                            {
                                module.GenerateBox(entityTypeIndex, gridWorldGP, ResultOrientation, boxExtraSerializeDataFromModule: entityExtraSerializeData);
                            }
                            else if (ChangeBoxTypeToEntityName.TypeDefineType == TypeDefineType.Enemy)
                            {
                                BornPointData newBornPointData = new BornPointData();
                                newBornPointData.LocalGP = module.WorldGPToLocalGP(gridWorldGP);
                                newBornPointData.WorldGP = gridWorldGP;
                                newBornPointData.EnemyType = ChangeBoxTypeToEntityName.Clone();
                                newBornPointData.RawEntityExtraSerializeData = entityExtraSerializeData;
                                BattleManager.Instance.CreateActorByBornPointData(newBornPointData, false);
                            }
                        }
                    }
                }
            });
        }
        else
        {
            Box.DestroyBox(delegate
            {
                if (entityTypeIndex != 0)
                {
                    WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(worldGP);
                    if (module != null)
                    {
                        if (ChangeBoxTypeToEntityName.TypeDefineType == TypeDefineType.Box)
                        {
                            module.GenerateBox(entityTypeIndex, worldGP, ResultOrientation, boxExtraSerializeDataFromModule: entityExtraSerializeData);
                        }
                        else if (ChangeBoxTypeToEntityName.TypeDefineType == TypeDefineType.Enemy)
                        {
                            BornPointData newBornPointData = new BornPointData();
                            newBornPointData.LocalGP = module.WorldGPToLocalGP(worldGP);
                            newBornPointData.WorldGP = worldGP;
                            newBornPointData.EnemyType = ChangeBoxTypeToEntityName.Clone();
                            newBornPointData.RawEntityExtraSerializeData = entityExtraSerializeData;
                            BattleManager.Instance.CreateActorByBornPointData(newBornPointData, false);
                        }
                    }
                }
            });
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_ChangeBoxType action = ((BoxPassiveSkillAction_ChangeBoxType) newAction);
        action.ChangeBoxTypeToEntityName = ChangeBoxTypeToEntityName.Clone();
        action.ResultOrientation = ResultOrientation;
        action.ChangeForEveryGrid = ChangeForEveryGrid;
        action.RawEntityExtraSerializeData = RawEntityExtraSerializeData.Clone();
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_ChangeBoxType action = ((BoxPassiveSkillAction_ChangeBoxType) srcData);
        ChangeBoxTypeToEntityName.CopyDataFrom(action.ChangeBoxTypeToEntityName);
        ResultOrientation = action.ResultOrientation;
        ChangeForEveryGrid = action.ChangeForEveryGrid;
        RawEntityExtraSerializeData = action.RawEntityExtraSerializeData.Clone();
    }
}