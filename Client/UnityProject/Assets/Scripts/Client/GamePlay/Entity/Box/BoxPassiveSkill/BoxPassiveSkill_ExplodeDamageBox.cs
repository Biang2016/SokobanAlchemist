using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxPassiveSkill_ExplodeDamageBox : BoxPassiveSkill
{
    protected override string Description => "箱子撞击爆炸AOE对Box造成伤害";

    [LabelText("判定半径")]
    public int AddBuffRadius = 2;

    [BoxGroup("耐久值伤害")]
    [HideLabel]
    public int DurabilityDamage;

    public override void OnDestroyBox()
    {
        base.OnDestroyBox();
        ExplodeDamage();
    }

    private void ExplodeDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(Box.transform.position, AddBuffRadius, LayerManager.Instance.LayerMask_BoxIndicator);
        List<Box> boxList = new List<Box>();
        foreach (Collider collider in colliders)
        {
            Box targetBox = collider.gameObject.GetComponentInParent<Box>();
            if (targetBox != null)
            {
                if (!boxList.Contains(targetBox))
                {
                    boxList.Add(targetBox);
                    targetBox.BoxStatPropSet.CommonDurability.Value -= DurabilityDamage;
                }
            }
        }
    }

    protected override void ChildClone(BoxPassiveSkill newBF)
    {
        base.ChildClone(newBF);
        BoxPassiveSkill_ExplodeDamageBox bf = ((BoxPassiveSkill_ExplodeDamageBox) newBF);
        bf.AddBuffRadius = AddBuffRadius;
        bf.DurabilityDamage = DurabilityDamage;
    }

    public override void CopyDataFrom(BoxPassiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkill_ExplodeDamageBox bf = ((BoxPassiveSkill_ExplodeDamageBox) srcData);
        AddBuffRadius = bf.AddBuffRadius;
        bf.DurabilityDamage = DurabilityDamage;
    }
}