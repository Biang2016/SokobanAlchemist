using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillAction_TransportPlayer : BoxSkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "传送玩家";

    [LabelText("世界类型概率")]
    public List<WorldNameWithProbability> WorldProbList = new List<WorldNameWithProbability>();

    public void Execute()
    {
        WorldNameWithProbability randomResult = CommonUtils.GetRandomFromList(WorldProbList);
        ClientGameManager.Instance.ChangeWorld(randomResult.WorldTypeName.TypeName, true);
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_TransportPlayer action = ((EntitySkillAction_TransportPlayer) newAction);
        action.WorldProbList = WorldProbList.Clone<WorldNameWithProbability, WorldNameWithProbability>();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_TransportPlayer action = ((EntitySkillAction_TransportPlayer) srcData);
        WorldProbList = action.WorldProbList.Clone<WorldNameWithProbability, WorldNameWithProbability>();
    }
}