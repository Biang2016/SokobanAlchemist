using System;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

[Serializable]
public class BoxPassiveSkillAction_ChangeBoxToEnemy : BoxPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "更改箱子为敌人";

    [LabelText("敌人类型")]
    [ValueDropdown("GetAllEnemyNames", IsUniqueList = true, DropdownTitle = "选择敌人类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string ChangeBoxToEnemyType = "None";

    public void Execute()
    {
        if (Box.State == Box.States.Static)
        {
            WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(Box.WorldGP);
            if (module != null)
            {
                GridPos3D localGP = Box.LocalGP;
                Box.DestroyBox();
                ushort enemyTypeIndex = ConfigManager.GetEnemyTypeIndex(ChangeBoxToEnemyType);
                if (enemyTypeIndex != 0)
                {
                    BornPointData newBornPointData = new BornPointData();
                    newBornPointData.LocalGP = localGP;
                    newBornPointData.ActorType = ChangeBoxToEnemyType;
                    BattleManager.Instance.CreateActorByBornPointData(newBornPointData);
                }
            }
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_ChangeBoxToEnemy bf = ((BoxPassiveSkillAction_ChangeBoxToEnemy) newAction);
        bf.ChangeBoxToEnemyType = ChangeBoxToEnemyType;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_ChangeBoxToEnemy bf = ((BoxPassiveSkillAction_ChangeBoxToEnemy) srcData);
        ChangeBoxToEnemyType = bf.ChangeBoxToEnemyType;
    }
}