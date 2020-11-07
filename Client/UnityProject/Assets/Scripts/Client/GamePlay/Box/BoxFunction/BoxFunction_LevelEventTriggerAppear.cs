using System;
using System.Collections.Generic;
using BiangStudio.GameDataFormat.Grid;
using UnityEngine.Events;

[Serializable]
public class BoxFunction_LevelEventTriggerAppear : BoxFunction_InvokeOnLevelEventID
{
    protected override string BoxFunctionDisplayName => "关卡事件触发后才出现";

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
        public BoxFunction_LevelEventTriggerAppear BoxFunction_LevelEventTriggerAppear;
        public Box.WorldSpecialBoxData WorldSpecialBoxData; // 复用这个数据结构，仅世界下生效

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.BoxTypeIndex = BoxTypeIndex;
            data.BoxFunction_LevelEventTriggerAppear = (BoxFunction_LevelEventTriggerAppear)BoxFunction_LevelEventTriggerAppear.Clone(); // 此处慎重Clone，因为GenerateBoxAction没有深拷贝
            data.WorldSpecialBoxData = WorldSpecialBoxData?.Clone();
        }
    }
}