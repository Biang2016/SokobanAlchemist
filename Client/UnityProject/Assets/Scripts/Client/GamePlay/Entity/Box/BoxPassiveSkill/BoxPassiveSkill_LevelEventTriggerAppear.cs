using System;
using UnityEngine.Events;
using UnityEngine.Serialization;

[assembly: Sirenix.Serialization.BindTypeNameToType("BoxFunction_LevelEventTriggerAppear", typeof(BoxPassiveSkill_LevelEventTriggerAppear))]

[Serializable]
public class BoxPassiveSkill_LevelEventTriggerAppear : BoxPassiveSkill_InvokeOnLevelEventID
{
    protected override string Description => "关卡事件触发后才出现";

    internal UnityAction GenerateBoxAction; // 不进行深拷贝

    protected override void OnEventExecute()
    {
        GenerateBoxAction?.Invoke();
    }

    public void ClearAndUnRegister()
    {
        GenerateBoxAction = null;
        OnUnRegisterLevelEventID();
    }

    [Serializable]
    public class Data : LevelComponentData
    {
        public ushort BoxTypeIndex;

        [FormerlySerializedAs("BoxFunction_LevelEventTriggerAppear")]
        public BoxPassiveSkill_LevelEventTriggerAppear BoxPassiveSkill_LevelEventTriggerAppear;

        public Box.WorldSpecialBoxData WorldSpecialBoxData; // 复用这个数据结构，仅世界下生效

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.BoxTypeIndex = BoxTypeIndex;
            data.BoxPassiveSkill_LevelEventTriggerAppear = (BoxPassiveSkill_LevelEventTriggerAppear) BoxPassiveSkill_LevelEventTriggerAppear.Clone(); // 此处慎重Clone，因为GenerateBoxAction没有深拷贝
            data.WorldSpecialBoxData = WorldSpecialBoxData?.Clone();
        }
    }
}