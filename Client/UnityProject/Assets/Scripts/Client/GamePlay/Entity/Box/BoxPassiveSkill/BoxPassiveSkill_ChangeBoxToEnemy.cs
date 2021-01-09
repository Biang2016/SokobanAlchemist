using System;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

[Serializable]
public class BoxPassiveSkill_ChangeBoxToEnemy : BoxPassiveSkill_InvokeOnLevelEventID
{
    protected override string Description => "更改箱子为敌人";

    [BoxName]
    [LabelText("敌人类型")]
    [ValueDropdown("GetAllEnemyNames", IsUniqueList = true, DropdownTitle = "选择敌人类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string ChangeBoxToEnemyType = "None";

    protected override void OnEventExecute()
    {
        if (Box.State == Box.States.Static)
        {
            WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(Box.WorldGP);
            if (module != null)
            {
                GridPos3D localGP = Box.LocalGP;
                Box.DeleteSelf();
                ushort enemyTypeIndex = ConfigManager.GetEnemyTypeIndex(ChangeBoxToEnemyType);
                if (enemyTypeIndex != 0)
                {
                    BornPointData newBornPointData = new BornPointData();
                    newBornPointData.LocalGP = localGP;
                    newBornPointData.WorldGP = module.LocalGPToWorldGP(localGP);
                    newBornPointData.ActorType = ChangeBoxToEnemyType;
                    BattleManager.Instance.CreateActorByBornPointData(newBornPointData);
                }
            }
        }
    }

    protected override void ChildClone(BoxPassiveSkill newBF)
    {
        base.ChildClone(newBF);
        BoxPassiveSkill_ChangeBoxToEnemy bf = ((BoxPassiveSkill_ChangeBoxToEnemy) newBF);
        bf.ChangeBoxToEnemyType = ChangeBoxToEnemyType;
    }

    public override void CopyDataFrom(BoxPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkill_ChangeBoxToEnemy bf = ((BoxPassiveSkill_ChangeBoxToEnemy) srcData);
    }
}