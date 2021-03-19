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
    public TypeSelectHelper ChangeBoxToEnemyTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.Enemy};

    public void Execute()
    {
        if (Box.State == Box.States.Static)
        {
            WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(Box.WorldGP);
            if (module != null)
            {
                GridPos3D localGP = Box.LocalGP;
                Box.DestroyBox();
                ushort enemyTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.Enemy, ChangeBoxToEnemyTypeName.TypeName);
                if (enemyTypeIndex != 0)
                {
                    BornPointData newBornPointData = new BornPointData();
                    newBornPointData.LocalGP = localGP;
                    newBornPointData.WorldGP = module.LocalGPToWorldGP(localGP);
                    newBornPointData.EnemyType = ChangeBoxToEnemyTypeName.Clone();
                    BattleManager.Instance.CreateActorByBornPointData(newBornPointData);
                }
            }
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_ChangeBoxToEnemy bf = ((BoxPassiveSkillAction_ChangeBoxToEnemy) newAction);
        bf.ChangeBoxToEnemyTypeName = ChangeBoxToEnemyTypeName.Clone();
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_ChangeBoxToEnemy bf = ((BoxPassiveSkillAction_ChangeBoxToEnemy) srcData);
        ChangeBoxToEnemyTypeName.CopyDataFrom(bf.ChangeBoxToEnemyTypeName);
    }
}