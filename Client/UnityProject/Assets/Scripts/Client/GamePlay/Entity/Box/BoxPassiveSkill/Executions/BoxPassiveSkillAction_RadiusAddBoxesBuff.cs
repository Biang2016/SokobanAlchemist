using System;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxPassiveSkillAction_RadiusAddBoxesBuff : BoxPassiveSkillAction, BoxPassiveSkillAction.IPureAction
{
    protected override string Description => "箱子撞击爆炸AOE给Box施加Buff";

    [LabelText("判定半径")]
    public int AddBuffRadius = 2;

    [SerializeReference]
    [HideLabel]
    public BoxBuff BoxBuff;

    public void Execute()
    {
        HashSet<uint> boxList = new HashSet<uint>();
        foreach (GridPos3D offset in Box.GetBoxOccupationGPs())
        {
            Vector3 boxIndicatorPos = Box.transform.position + offset;
            Collider[] colliders = Physics.OverlapSphere(boxIndicatorPos, AddBuffRadius, LayerManager.Instance.LayerMask_BoxIndicator);
            foreach (Collider collider in colliders)
            {
                Box targetBox = collider.gameObject.GetComponentInParent<Box>();
                if (targetBox != null && !boxList.Contains(targetBox.GUID))
                {
                    boxList.Add(targetBox.GUID);
                    if (!targetBox.BoxBuffHelper.AddBuff(BoxBuff.Clone()))
                    {
                        Debug.Log($"Failed to AddBuff: {BoxBuff.GetType().Name} to {targetBox.name}");
                    }
                }
            }
        }
    }

    protected override void ChildClone(BoxPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_RadiusAddBoxesBuff action = ((BoxPassiveSkillAction_RadiusAddBoxesBuff) newAction);
        action.BoxBuff = (BoxBuff) BoxBuff.Clone();
        action.AddBuffRadius = AddBuffRadius;
    }

    public override void CopyDataFrom(BoxPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_RadiusAddBoxesBuff action = ((BoxPassiveSkillAction_RadiusAddBoxesBuff) srcData);
        BoxBuff = (BoxBuff) action.BoxBuff.Clone();
        AddBuffRadius = action.AddBuffRadius;
    }
}