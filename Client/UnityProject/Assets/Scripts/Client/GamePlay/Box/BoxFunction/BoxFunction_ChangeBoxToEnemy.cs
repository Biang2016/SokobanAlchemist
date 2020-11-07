using System;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;

[Serializable]
public class BoxFunction_ChangeBoxToEnemy : BoxFunction_InvokeOnLevelEventID
{
    protected override string BoxFunctionDisplayName => "更改箱子为敌人";

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

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_ChangeBoxToEnemy bf = ((BoxFunction_ChangeBoxToEnemy) newBF);
        bf.ChangeBoxToEnemyType = ChangeBoxToEnemyType;
    }

    public override void CopyDataFrom(BoxFunctionBase srcData)
    {
        base.CopyDataFrom(srcData);
        BoxFunction_ChangeBoxToEnemy bf = ((BoxFunction_ChangeBoxToEnemy) srcData);
    }
}