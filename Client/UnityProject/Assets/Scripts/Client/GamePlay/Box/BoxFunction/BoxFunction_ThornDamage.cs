using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxFunction_ThornDamage : BoxFunctionBase
{
    protected override string BoxFunctionDisplayName => "荆棘伤害";

    [LabelText("每次伤害")]
    public int Damage = 0;

    [LabelText("伤害间隔时间/s")]
    public float DamageInterval = 1f;

    public override void OnBoxThornTrapTriggerEnter(Collider collider)
    {
        base.OnBoxThornTrapTriggerEnter(collider);
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

    public override void OnBoxThornTrapTriggerStay(Collider collider)
    {
        base.OnBoxThornTrapTriggerStay(collider);
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

    public override void OnBoxThornTrapTriggerExit(Collider collider)
    {
        base.OnBoxThornTrapTriggerExit(collider);
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

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_ThornDamage bf = ((BoxFunction_ThornDamage) newBF);
        bf.Damage = Damage;
        bf.DamageInterval = DamageInterval;
    }

    public override void CopyDataFrom(BoxFunctionBase srcData)
    {
        base.CopyDataFrom(srcData);
        BoxFunction_ThornDamage bf = ((BoxFunction_ThornDamage) srcData);
        Damage = bf.Damage;
        DamageInterval = bf.DamageInterval;
    }
}