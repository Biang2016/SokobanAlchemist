using System;
using System.Collections.Generic;
using System.Linq;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class BoxFunctionBase
{
    internal Box Box;

    private IEnumerable<string> GetAllBoxTypeNames()
    {
        ConfigManager.LoadAllConfigs();
        List<string> res = ConfigManager.BoxTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "None");
        return res;
    }

    public virtual void OnRegisterLevelEventID()
    {
    }

    public virtual void OnUnRegisterLevelEventID()
    {
    }

    public virtual void OnBeingLift(Actor actor)
    {
    }

    public virtual void OnFlyingCollisionEnterDestroy(Collision collision)
    {
    }

    public virtual void OnBeingKickedCollisionEnterDestroy(Collision collision)
    {
    }

    public virtual void OnBoxThornTrapTriggerEnter(Collider collider)
    {
    }

    public virtual void OnBoxThornTrapTriggerStay(Collider collider)
    {
    }

    public virtual void OnBoxThornTrapTriggerExit(Collider collider)
    {
    }
}

[Serializable]
[LabelText("关卡事件触发")]
public abstract class BoxFunction_InvokeOnLevelEventID : BoxFunctionBase
{
    [LabelText("监听关卡事件ID")]
    public int ListenLevelEventID;

    public override void OnRegisterLevelEventID()
    {
        ClientGameManager.Instance.BattleMessenger.AddListener<int>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventID, OnEvent);
    }

    public override void OnUnRegisterLevelEventID()
    {
        ClientGameManager.Instance.BattleMessenger.RemoveListener<int>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventID, OnEvent);
    }

    private void OnEvent(int eventID)
    {
        if (ListenLevelEventID == eventID)
        {
            OnEventExecute();
        }
    }

    protected abstract void OnEventExecute();
}

[Serializable]
[LabelText("更改箱子类型")]
public class BoxFunction_ChangeBoxType : BoxFunction_InvokeOnLevelEventID
{
    [LabelText("更改箱子类型为")]
    [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DropdownTitle = "选择箱子类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string ChangeBoxTypeTo;

    protected override void OnEventExecute()
    {
        if (Box.State == Box.States.Static)
        {
            WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(Box.GridPos3D);
            if (module != null)
            {
                GridPos3D localGP = Box.LocalGridPos3D;
                WorldManager.Instance.CurrentWorld.DeleteBox(Box);
                ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(ChangeBoxTypeTo);
                module.GenerateBox(boxTypeIndex, localGP);
            }
        }
    }
}

[Serializable]
[LabelText("举起箱子掉落踢技能")]
public class BoxFunction_LiftDropSkill : BoxFunctionBase
{
    [GUIColor(0, 1.0f, 0)]
    [LabelText("举起箱子掉落踢技能")]
    [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DropdownTitle = "选择箱子类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string LiftGetLiftBoxAbility;

    public override void OnBeingLift(Actor actor)
    {
        ushort dropLiftAbilityIndex = ConfigManager.GetBoxTypeIndex(LiftGetLiftBoxAbility);
        if (dropLiftAbilityIndex != 0)
        {
            Box.PlayCollideFX();
            actor.ActorSkillHelper.DisableInteract(InteractSkillType.Kick, actor.ActorSkillHelper.PlayerCurrentGetKickAbility);
            actor.ActorSkillHelper.PlayerCurrentGetKickAbility = dropLiftAbilityIndex;
            actor.ActorSkillHelper.EnableInteract(InteractSkillType.Kick, dropLiftAbilityIndex);
        }
    }
}

[Serializable]
[LabelText("举起箱子更换皮肤")]
public class BoxFunction_LiftDropSkin : BoxFunctionBase
{
    [GUIColor(0, 1.0f, 0)]
    [LabelText("举起箱子更换皮肤")]
    public Material DieDropMaterial;

    public override void OnBeingLift(Actor actor)
    {
        base.OnBeingLift(actor);
        actor.ActorSkinHelper.SwitchSkin(DieDropMaterial);
    }
}

[Serializable]
[LabelText("举箱子回复生命")]
public class BoxFunction_LiftGainHealth : BoxFunctionBase
{
    [LabelText("举箱子回复生命")]
    public int HealLifeCountWhenLifted;

    public override void OnBeingLift(Actor actor)
    {
        base.OnBeingLift(actor);
        if (Box.Healable)
        {
            actor.ActorBattleHelper.AddLife(HealLifeCountWhenLifted);
        }
    }
}

[Serializable]
[LabelText("爆炸推力")]
public class BoxFunction_ExplodePushForce : BoxFunctionBase
{
    [LabelText("碰撞爆炸推力半径")]
    public int ExplodePushRadius = 3;

    public override void OnFlyingCollisionEnterDestroy(Collision collision)
    {
        base.OnFlyingCollisionEnterDestroy(collision);
        ExplodePushBox(Box, Box.transform.position, ExplodePushRadius);
    }

    public override void OnBeingKickedCollisionEnterDestroy(Collision collision)
    {
        base.OnBeingKickedCollisionEnterDestroy(collision);
        ExplodePushBox(Box, Box.transform.position, ExplodePushRadius);
    }

    private void ExplodePushBox(Box m_Box, Vector3 center, int radius)
    {
        List<Box> boxes = new List<Box>();
        GridPos3D curGP = m_Box.transform.position.ToGridPos3D();
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                GridPos3D targetGP = curGP + new GridPos3D(x, 0, z);
                Box box = WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(targetGP, out WorldModule module, out GridPos3D gp);
                if (box != null)
                {
                    boxes.Add(box);
                }
            }
        }

        Collider[] colliders = Physics.OverlapSphere(center, radius, LayerManager.Instance.LayerMask_HitBox_Box);
        foreach (Collider collider in colliders)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box != null && box.Interactable && box != m_Box)
            {
                if (!boxes.Contains(box))
                {
                    if (box.State == Box.States.BeingKicked || box.State == Box.States.BeingPushed || box.State == Box.States.PushingCanceling || box.State == Box.States.PushingCanceling)
                    {
                        Vector3 diff = box.transform.position - center;
                        if (diff.x > diff.z)
                        {
                            diff.z = 0;
                        }
                        else if (diff.z > diff.x)
                        {
                            diff.x = 0;
                        }

                        diff.y = 0;
                        box.Kick(diff, 15f, m_Box.LastTouchActor);
                    }
                }
            }
        }
    }
}

[Serializable]
[LabelText("爆炸施加Buff")]
public class BoxFunction_ExplodeAddActorBuff : BoxFunctionBase
{
    [LabelText("爆炸施加ActorBuff")]
    public ActorBuff ActorBuff;

    [LabelText("生效于相对阵营")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [LabelText("判定半径")]
    public int AddBuffRadius = 2;

    [LabelText("永久Buff")]
    public bool PermanentBuff;

    [LabelText("Buff持续时间")]
    [HideIf("PermanentBuff")]
    public float Duration;

    public override void OnFlyingCollisionEnterDestroy(Collision collision)
    {
        base.OnFlyingCollisionEnterDestroy(collision);
        ExplodeAddBuff(collision.contacts[0].point);
    }

    public override void OnBeingKickedCollisionEnterDestroy(Collision collision)
    {
        base.OnBeingKickedCollisionEnterDestroy(collision);
        ExplodeAddBuff(collision.contacts[0].point);
    }

    private void ExplodeAddBuff(Vector3 center)
    {
        Collider[] colliders = Physics.OverlapSphere(center, AddBuffRadius, LayerManager.Instance.LayerMask_HitBox_Enemy | LayerManager.Instance.Layer_HitBox_Player);
        List<Actor> actorList = new List<Actor>();
        foreach (Collider collider in colliders)
        {
            Actor actor = collider.gameObject.GetComponentInParent<Actor>();
            if (actor != null)
            {
                Actor m_Actor = Box.LastTouchActor;
                if (m_Actor != null)
                {
                    if (EffectiveOnRelativeCamp == RelativeCamp.FriendCamp && !actor.IsSameCampOf(m_Actor))
                    {
                        continue;
                    }
                    else if (EffectiveOnRelativeCamp == RelativeCamp.OpponentCamp && !actor.IsOpponentCampOf(m_Actor))
                    {
                        continue;
                    }
                    else if (EffectiveOnRelativeCamp == RelativeCamp.NeutralCamp && !actor.IsNeutralCampOf(m_Actor))
                    {
                        continue;
                    }
                    else if (EffectiveOnRelativeCamp == RelativeCamp.AllCamp)
                    {
                    }
                    else if (EffectiveOnRelativeCamp == RelativeCamp.None)
                    {
                        continue;
                    }
                }

                if (!actorList.Contains(actor))
                {
                    actorList.Add(actor);
                    if (PermanentBuff)
                    {
                        actor.ActorBuffHelper.AddPermanentBuff(ActorBuff);
                    }
                    else
                    {
                        actor.ActorBuffHelper.AddBuff(ActorBuff, Duration);
                    }
                }
            }
        }
    }
}

[Serializable]
[LabelText("荆棘伤害")]
public class BoxFunction_ThornDamage : BoxFunctionBase
{
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
}

[Serializable]
[LabelText("沿途生成新箱子")]
public class BoxFunction_GenerateNewBoxes : BoxFunctionBase
{
    [LabelText("箱子类型")]
    [ValueDropdown("GetAllBoxTypeNames")]
    private string BoxTypeName;
}