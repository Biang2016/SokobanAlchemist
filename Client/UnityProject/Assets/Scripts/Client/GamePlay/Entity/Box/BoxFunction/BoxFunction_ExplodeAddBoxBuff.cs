using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxFunction_ExplodeAddBoxBuff : BoxFunctionBase
{
    protected override string BoxFunctionDisplayName => "箱子撞击爆炸AOE给Box施加Buff";

    [LabelText("判定半径")]
    public int AddBuffRadius = 2;

    [BoxGroup("爆炸施加BoxBuff")]
    [HideLabel]
    public BoxBuff BoxBuff;

    public override void OnFlyingCollisionEnter(Collision collision)
    {
        base.OnFlyingCollisionEnter(collision);
        ExplodeAddBuff();
    }

    public override void OnBeingKickedCollisionEnter(Collision collision)
    {
        base.OnBeingKickedCollisionEnter(collision);
        ExplodeAddBuff();
    }

    private void ExplodeAddBuff()
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
                    if (!targetBox.BoxBuffHelper.AddBuff(BoxBuff.Clone()))
                    {
                        Debug.Log($"Failed to AddBuff: {BoxBuff.GetType().Name} to {targetBox.name}");
                    }
                }
            }
        }
    }

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_ExplodeAddBoxBuff bf = ((BoxFunction_ExplodeAddBoxBuff) newBF);
        bf.BoxBuff = (BoxBuff) BoxBuff.Clone();
        bf.AddBuffRadius = AddBuffRadius;
    }

    public override void CopyDataFrom(BoxFunctionBase srcData)
    {
        base.CopyDataFrom(srcData);
        BoxFunction_ExplodeAddBoxBuff bf = ((BoxFunction_ExplodeAddBoxBuff) srcData);
        BoxBuff = (BoxBuff) bf.BoxBuff.Clone();
        AddBuffRadius = bf.AddBuffRadius;
    }
}