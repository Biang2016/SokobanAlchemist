using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxPassiveSkillAction_ThornDamage : BoxPassiveSkillAction, BoxPassiveSkillAction.ITriggerEnterAction, BoxPassiveSkillAction.ITriggerStayAction, BoxPassiveSkillAction.ITriggerExitAction
{
    protected override string Description => "荆棘伤害";

    [LabelText("每次伤害")]
    public int Damage = 0;

    [LabelText("伤害间隔时间/s")]
    public float DamageInterval = 1f;

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Actor actor = collider.GetComponentInParent<Actor>();
            if (actor != null && actor.IsEnemy)
            {
                if (!Box.BoxThornTrapTriggerHelper.ActorStayTimeDict.ContainsKey(actor.GUID))
                {
                    actor.ActorBattleHelper.Damage(null, Damage);
                    Box.BoxThornTrapTriggerHelper.ActorStayTimeDict.Add(actor.GUID, 0);
                }
            }
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Actor actor = collider.GetComponentInParent<Actor>();
            if (actor != null && actor.IsEnemy)
            {
                if (Box.BoxThornTrapTriggerHelper.ActorStayTimeDict.TryGetValue(actor.GUID, out float duration))
                {
                    if (duration > DamageInterval)
                    {
                        actor.ActorBattleHelper.Damage(null, Damage);
                        Box.BoxThornTrapTriggerHelper.ActorStayTimeDict[actor.GUID] = 0;
                    }
                    else
                    {
                        Box.BoxThornTrapTriggerHelper.ActorStayTimeDict[actor.GUID] += Time.fixedDeltaTime;
                    }
                }
            }
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Actor actor = collider.GetComponentInParent<Actor>();
            if (actor != null && actor.IsEnemy)
            {
                if (Box.BoxThornTrapTriggerHelper.ActorStayTimeDict.ContainsKey(actor.GUID))
                {
                    Box.BoxThornTrapTriggerHelper.ActorStayTimeDict.Remove(actor.GUID);
                }
            }
        }
    }

    protected override void ChildClone(BoxPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        BoxPassiveSkillAction_ThornDamage action = ((BoxPassiveSkillAction_ThornDamage) newAction);
        action.Damage = Damage;
        action.DamageInterval = DamageInterval;
    }

    public override void CopyDataFrom(BoxPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        BoxPassiveSkillAction_ThornDamage action = ((BoxPassiveSkillAction_ThornDamage) srcData);
        Damage = action.Damage;
        DamageInterval = action.DamageInterval;
    }
}