using System;
using Sirenix.OdinInspector;

[Serializable]
public class BoxSkillAction_ChangeMergeEntityData : BoxSkillAction, EntitySkillAction.IPureAction, EntitySkillAction.IEntityAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "修改Merge产物";

    [LabelText("覆写Key")]
    public string OverrideKey = ""; // 相同OverrideKey的MergeEntityData在关卡设计上应配为完全相同的，使其不会反复覆写。Merge逻辑执行时只随机取用一个.不同的Key的Data会将Extra数据全部叠加在一起

    [LabelText("覆写Merge产物的EntityData")]
    public EntityData MergeEntityData = new EntityData(); // 如果几个Merge的Box指向的MergeEntityData的箱子种类不同，则报错

    [LabelText("True:对目标Entity生效; False:对本Entity生效")]
    public bool ExertOnTarget;

    public void Execute()
    {
        if (ExertOnTarget) return;
        Box.BoxMergeConfig.Temp_NextMergeEntityData = MergeEntityData.Clone();
        Box.BoxMergeConfig.Temp_NextMergeEntityDataOverrideKey = OverrideKey;
    }

    public void ExecuteOnEntity(Entity entity)
    {
        if (!ExertOnTarget) return;
        if (entity is Box box)
        {
            box.BoxMergeConfig.Temp_NextMergeEntityData = MergeEntityData.Clone();
            box.BoxMergeConfig.Temp_NextMergeEntityDataOverrideKey = OverrideKey;
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxSkillAction_ChangeMergeEntityData action = ((BoxSkillAction_ChangeMergeEntityData) newAction);
        action.OverrideKey = OverrideKey;
        action.MergeEntityData = MergeEntityData.Clone();
        action.ExertOnTarget = ExertOnTarget;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxSkillAction_ChangeMergeEntityData action = ((BoxSkillAction_ChangeMergeEntityData) srcData);
        OverrideKey = action.OverrideKey;
        MergeEntityData = action.MergeEntityData.Clone();
        ExertOnTarget = action.ExertOnTarget;
    }
}