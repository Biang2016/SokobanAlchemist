﻿using System;
using System.Collections.Generic;
using System.Linq;
using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class BoxFunctionBase : IClone<BoxFunctionBase>
{
    internal Box Box;

    [LabelText("特例类型")]
    [EnumToggleButtons]
    public BoxFunctionBaseSpecialCaseType SpecialCaseType = BoxFunctionBaseSpecialCaseType.None;

    public enum BoxFunctionBaseSpecialCaseType
    {
        [LabelText("无")]
        None,

        [LabelText("模组特例")]
        Module,

        [LabelText("模组特例")]
        World,
    }

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    private IEnumerable<string> GetAllEnemyNames => ConfigManager.GetAllEnemyNames();

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

    public BoxFunctionBase Clone()
    {
        Type type = GetType();
        BoxFunctionBase newBF = (BoxFunctionBase) Activator.CreateInstance(type);
        ChildClone(newBF);
        return newBF;
    }

    protected virtual void ChildClone(BoxFunctionBase newBF)
    {
    }

    public virtual void ApplyData(BoxFunctionBase srcData)
    {
    }
}

[Serializable]
[LabelText("隐藏")]
public class BoxFunction_Hide : BoxFunctionBase
{
    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_Hide bf = ((BoxFunction_Hide) newBF);
    }

    public override void ApplyData(BoxFunctionBase srcData)
    {
        base.ApplyData(srcData);
        BoxFunction_Hide bf = ((BoxFunction_Hide) srcData);
    }
}

[Serializable]
[LabelText("形状和朝向")]
public class BoxFunction_ShapeAndOrientation : BoxFunctionBase
{
    [LabelText("形状")]
    public BoxShapeType BoxShapeType = BoxShapeType.Box;

    [LabelText("朝向")]
    public GridPosR.Orientation Orientation = GridPosR.Orientation.Up;

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_ShapeAndOrientation bf = ((BoxFunction_ShapeAndOrientation) newBF);
        bf.BoxShapeType = BoxShapeType;
        bf.Orientation = Orientation;
    }

    public override void ApplyData(BoxFunctionBase srcData)
    {
        base.ApplyData(srcData);
        BoxFunction_ShapeAndOrientation bf = ((BoxFunction_ShapeAndOrientation) srcData);
        BoxShapeType = bf.BoxShapeType;
        Orientation = bf.Orientation;
    }
}

[Serializable]
[LabelText("关卡事件触发")]
public abstract class BoxFunction_InvokeOnLevelEventID : BoxFunctionBase
{
    [BoxGroup("事件监听与触发")]
    [LabelText("多个事件联合触发")]
    public bool MultiEventTrigger = false;

    [BoxGroup("事件监听与触发")]
    [ShowIf("MultiEventTrigger")]
    [LabelText("监听关卡事件花名列表(联合触发)")]
    public List<string> ListenLevelEventAliasList = new List<string>();

    [ShowInInspector]
    [HideInEditorMode]
    [LabelText("联合触发记录")]
    private List<bool> multiTriggerFlags = new List<bool>();

    [BoxGroup("事件监听与触发")]
    [HideIf("MultiEventTrigger")]
    [LabelText("监听关卡事件花名(单个触发)")]
    public string ListenLevelEventAlias;

    [BoxGroup("事件监听与触发")]
    [LabelText("最大触发次数")]
    public int MaxTriggeredTimes = 1;

    [ShowInInspector]
    [HideInEditorMode]
    [LabelText("已触发次数")]
    private int triggeredTimes = 0;

    public override void OnRegisterLevelEventID()
    {
        triggeredTimes = 0;
        ClientGameManager.Instance.BattleMessenger.AddListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnEvent);
        multiTriggerFlags.Clear();
        foreach (string alias in ListenLevelEventAliasList)
        {
            multiTriggerFlags.Add(false);
        }
    }

    public override void OnUnRegisterLevelEventID()
    {
        triggeredTimes = 0;
        ClientGameManager.Instance.BattleMessenger.RemoveListener<string>((uint) ENUM_BattleEvent.Battle_TriggerLevelEventAlias, OnEvent);
        multiTriggerFlags.Clear();
    }

    private void OnEvent(string eventAlias)
    {
        if (MultiEventTrigger)
        {
            for (int index = 0; index < ListenLevelEventAliasList.Count; index++)
            {
                string alias = ListenLevelEventAliasList[index];
                if (eventAlias == alias && !multiTriggerFlags[index])
                {
                    multiTriggerFlags[index] = true;
                }
            }

            bool trigger = true;
            foreach (bool flag in multiTriggerFlags)
            {
                if (!flag) trigger = false;
            }

            if (trigger)
            {
                ExecuteFunction();
            }
        }
        else
        {
            if (ListenLevelEventAlias.Equals(eventAlias))
            {
                ExecuteFunction();
            }
        }
    }

    private void ExecuteFunction()
    {
        if (triggeredTimes < MaxTriggeredTimes)
        {
            OnEventExecute();
            triggeredTimes++;
            for (int i = 0; i < multiTriggerFlags.Count; i++)
            {
                multiTriggerFlags[i] = false;
            }
        }
    }

    protected abstract void OnEventExecute();

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_InvokeOnLevelEventID bf = ((BoxFunction_InvokeOnLevelEventID) newBF);
        bf.MultiEventTrigger = MultiEventTrigger;
        bf.ListenLevelEventAliasList = ListenLevelEventAliasList.Clone();
        bf.ListenLevelEventAlias = ListenLevelEventAlias;
        bf.MaxTriggeredTimes = MaxTriggeredTimes;
    }

    public override void ApplyData(BoxFunctionBase srcData)
    {
        base.ApplyData(srcData);
        BoxFunction_InvokeOnLevelEventID bf = ((BoxFunction_InvokeOnLevelEventID) srcData);
        MultiEventTrigger = bf.MultiEventTrigger;
        ListenLevelEventAliasList = bf.ListenLevelEventAliasList.Clone();
        ListenLevelEventAlias = bf.ListenLevelEventAlias;
        MaxTriggeredTimes = bf.MaxTriggeredTimes;
    }
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
            WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(Box.WorldGP);
            if (module != null)
            {
                GridPos3D localGP = Box.LocalGP;
                WorldManager.Instance.CurrentWorld.DeleteBox(Box);
                ushort boxTypeIndex = ConfigManager.GetBoxTypeIndex(ChangeBoxTypeTo);
                if (boxTypeIndex != 0) module.GenerateBox(boxTypeIndex, localGP);
            }
        }
    }

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_ChangeBoxType bf = ((BoxFunction_ChangeBoxType) newBF);
        bf.ChangeBoxTypeTo = ChangeBoxTypeTo;
    }

    public override void ApplyData(BoxFunctionBase srcData)
    {
        base.ApplyData(srcData);
        BoxFunction_ChangeBoxType bf = ((BoxFunction_ChangeBoxType) srcData);
        ChangeBoxTypeTo = bf.ChangeBoxTypeTo;
    }
}

[Serializable]
[LabelText("箱子变敌人")]
public class BoxFunction_ChangeBoxToEnemy : BoxFunction_InvokeOnLevelEventID
{
    [LabelText("更改箱子为敌人")]
    [ValueDropdown("GetAllEnemyNames", IsUniqueList = true, DropdownTitle = "选择敌人类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string ChangeBoxToEnemyType;

    protected override void OnEventExecute()
    {
        if (Box.State == Box.States.Static)
        {
            WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(Box.WorldGP);
            if (module != null)
            {
                GridPos3D localGP = Box.LocalGP;
                WorldManager.Instance.CurrentWorld.DeleteBox(Box);
                ushort enemyTypeIndex = ConfigManager.GetEnemyTypeIndex(ChangeBoxToEnemyType);
                if (enemyTypeIndex != 0)
                {
                    BornPointData newBornPointData = new BornPointData();
                    newBornPointData.LocalGP = localGP;
                    newBornPointData.ActorType = ChangeBoxToEnemyType;
                    BattleManager.Instance.CreateActorByBornPointData(newBornPointData, module);
                }
            }
        }
    }

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_ChangeBoxToEnemy bf = ((BoxFunction_ChangeBoxToEnemy) newBF);
        bf.ChangeBoxToEnemyType = ChangeBoxToEnemyType;
    }

    public override void ApplyData(BoxFunctionBase srcData)
    {
        base.ApplyData(srcData);
        BoxFunction_ChangeBoxToEnemy bf = ((BoxFunction_ChangeBoxToEnemy) srcData);
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

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_LiftDropSkill bf = ((BoxFunction_LiftDropSkill) newBF);
        bf.LiftGetLiftBoxAbility = LiftGetLiftBoxAbility;
    }

    public override void ApplyData(BoxFunctionBase srcData)
    {
        base.ApplyData(srcData);
        BoxFunction_LiftDropSkill bf = ((BoxFunction_LiftDropSkill) srcData);
        LiftGetLiftBoxAbility = bf.LiftGetLiftBoxAbility;
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

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_LiftDropSkin bf = ((BoxFunction_LiftDropSkin) newBF);
        bf.DieDropMaterial = DieDropMaterial;
    }

    public override void ApplyData(BoxFunctionBase srcData)
    {
        base.ApplyData(srcData);
        BoxFunction_LiftDropSkin bf = ((BoxFunction_LiftDropSkin) srcData);
        DieDropMaterial = bf.DieDropMaterial;
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
        actor.ActorBattleHelper.AddLife(HealLifeCountWhenLifted);
    }

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_LiftGainHealth bf = ((BoxFunction_LiftGainHealth) newBF);
        bf.HealLifeCountWhenLifted = HealLifeCountWhenLifted;
    }

    public override void ApplyData(BoxFunctionBase srcData)
    {
        base.ApplyData(srcData);
        BoxFunction_LiftGainHealth bf = ((BoxFunction_LiftGainHealth) srcData);
        HealLifeCountWhenLifted = bf.HealLifeCountWhenLifted;
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

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_ExplodePushForce bf = ((BoxFunction_ExplodePushForce) newBF);
        bf.ExplodePushRadius = ExplodePushRadius;
    }

    public override void ApplyData(BoxFunctionBase srcData)
    {
        base.ApplyData(srcData);
        BoxFunction_ExplodePushForce bf = ((BoxFunction_ExplodePushForce) srcData);
        ExplodePushRadius = bf.ExplodePushRadius;
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

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_ExplodeAddActorBuff bf = ((BoxFunction_ExplodeAddActorBuff) newBF);
        bf.ActorBuff = ActorBuff.Clone();
        bf.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
        bf.AddBuffRadius = AddBuffRadius;
        bf.PermanentBuff = PermanentBuff;
        bf.Duration = Duration;
    }

    public override void ApplyData(BoxFunctionBase srcData)
    {
        base.ApplyData(srcData);
        BoxFunction_ExplodeAddActorBuff bf = ((BoxFunction_ExplodeAddActorBuff) srcData);
        ActorBuff = bf.ActorBuff.Clone();
        EffectiveOnRelativeCamp = bf.EffectiveOnRelativeCamp;
        AddBuffRadius = bf.AddBuffRadius;
        PermanentBuff = bf.PermanentBuff;
        Duration = bf.Duration;
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

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_ThornDamage bf = ((BoxFunction_ThornDamage) newBF);
        bf.Damage = Damage;
        bf.DamageInterval = DamageInterval;
    }

    public override void ApplyData(BoxFunctionBase srcData)
    {
        base.ApplyData(srcData);
        BoxFunction_ThornDamage bf = ((BoxFunction_ThornDamage) srcData);
        Damage = bf.Damage;
        DamageInterval = bf.DamageInterval;
    }
}

[Serializable]
[LabelText("沿途生成新箱子")]
public class BoxFunction_GenerateNewBoxes : BoxFunctionBase
{
    [LabelText("箱子类型")]
    [ValueDropdown("GetAllBoxTypeNames")]
    private string BoxTypeName;

    protected override void ChildClone(BoxFunctionBase newBF)
    {
        base.ChildClone(newBF);
        BoxFunction_GenerateNewBoxes bf = ((BoxFunction_GenerateNewBoxes) newBF);
        bf.BoxTypeName = BoxTypeName;
    }

    public override void ApplyData(BoxFunctionBase srcData)
    {
        base.ApplyData(srcData);
        BoxFunction_GenerateNewBoxes bf = ((BoxFunction_GenerateNewBoxes) srcData);
        BoxTypeName = bf.BoxTypeName;
    }
}