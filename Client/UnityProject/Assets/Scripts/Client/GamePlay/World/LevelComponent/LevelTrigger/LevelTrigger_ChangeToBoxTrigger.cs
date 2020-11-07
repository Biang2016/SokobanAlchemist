using System;
using BiangStudio;
using Sirenix.OdinInspector;
using UnityEngine;

public class LevelTrigger_ChangeToBoxTrigger : LevelTriggerBase
{
    [LabelText("配置")]
    public Data childData = new Data();

    public override LevelTriggerBase.Data TriggerData
    {
        get { return childData; }
        set { childData = (Data) value; }
    }

    [Serializable]
    public new class Data : LevelTriggerBase.Data
    {
        [BoxName]
        [LabelText("指定箱子类型")]
        [ValueDropdown("GetAllBoxTypeNames", DropdownTitle = "选择箱子类型")]
        public string ChangeToBoxTypeName = "None";

        protected override void ChildClone(LevelComponentData newData)
        {
            base.ChildClone(newData);
            Data data = ((Data) newData);
            data.ChangeToBoxTypeName = ChangeToBoxTypeName;
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Color color = "#0081FF".HTMLColorToColor();
        Gizmos.color = color;
        transform.DrawSpecialTip(Vector3.zero, color, Color.white, "BLT");
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        Gizmos.DrawSphere(transform.position + Vector3.right * 0.25f + Vector3.forward * 0.25f, 0.15f);
    }
}