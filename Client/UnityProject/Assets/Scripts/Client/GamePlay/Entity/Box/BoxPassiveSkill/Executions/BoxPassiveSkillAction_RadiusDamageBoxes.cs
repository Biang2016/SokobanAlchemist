using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxPassiveSkillAction_RadiusDamageBoxes : BoxPassiveSkillAction, BoxPassiveSkillAction.IPureAction
{
    protected override string Description => "对附近Boxes造成伤害";

    //[LabelText("判定半径")]
    public int AddBuffRadius = 2;

    //[BoxGroup("耐久值伤害")]
    //[HideLabel]
    public int DurabilityDamage;

    public  void Execute()
    {
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

    protected override void ChildClone(BoxPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_RadiusDamageBoxes action = ((BoxPassiveSkillAction_RadiusDamageBoxes)newAction);
        action.AddBuffRadius = AddBuffRadius;
        action.DurabilityDamage = DurabilityDamage;
    }

    public override void CopyDataFrom(BoxPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_RadiusDamageBoxes action = ((BoxPassiveSkillAction_RadiusDamageBoxes) srcData);
        AddBuffRadius = action.AddBuffRadius;
        action.DurabilityDamage = DurabilityDamage;
    }
}