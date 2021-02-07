using System;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine.Events;

[Serializable]
public class BoxPassiveSkill_LevelEventTriggerAppear : EntityPassiveSkill_Conditional
{
    protected override string Description => "关卡事件触发后才出现(仅箱子使用)";

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
        public GridPosR.Orientation BoxOrientation;

        public BoxPassiveSkill_LevelEventTriggerAppear BoxPassiveSkill_LevelEventTriggerAppear;

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.BoxTypeIndex = BoxTypeIndex;
            data.BoxOrientation = BoxOrientation;
            data.BoxPassiveSkill_LevelEventTriggerAppear = (BoxPassiveSkill_LevelEventTriggerAppear) BoxPassiveSkill_LevelEventTriggerAppear.Clone(); // 此处慎重Clone，因为GenerateBoxAction没有深拷贝
        }
    }

    public override void CopyDataFrom(EntityPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
    }
}