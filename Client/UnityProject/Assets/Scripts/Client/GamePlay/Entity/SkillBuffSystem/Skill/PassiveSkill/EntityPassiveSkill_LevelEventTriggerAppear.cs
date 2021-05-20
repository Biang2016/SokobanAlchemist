using System;
using UnityEngine.Events;

[Serializable]
public class EntityPassiveSkill_LevelEventTriggerAppear : EntityPassiveSkill_Conditional
{
    protected override string Description => "关卡事件触发后才出现(仅箱子使用)";

    internal UnityAction GenerateEntityAction; // 不进行深拷贝

    protected override void OnEventExecute()
    {
        GenerateEntityAction?.Invoke();
    }

    public void ClearAndUnRegister()
    {
        GenerateEntityAction = null;
        OnUnRegisterLevelEventID();
    }

    [Serializable]
    public class Data : LevelComponentData
    {
        public EntityData EntityData = new EntityData();
        public EntityPassiveSkill_LevelEventTriggerAppear EntityPassiveSkill_LevelEventTriggerAppear;

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.EntityData = EntityData.Clone();
            data.EntityPassiveSkill_LevelEventTriggerAppear = (EntityPassiveSkill_LevelEventTriggerAppear) EntityPassiveSkill_LevelEventTriggerAppear.Clone(); // 此处慎重Clone，因为GenerateEntityAction没有深拷贝
        }
    }
}