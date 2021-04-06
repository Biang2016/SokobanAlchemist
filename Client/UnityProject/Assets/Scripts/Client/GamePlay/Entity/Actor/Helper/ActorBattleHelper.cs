using System.Collections;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay.UI;
using UnityEngine;
using UnityEngine.Events;

public class ActorBattleHelper : ActorMonoHelper
{
    internal Box LastAttackBox;

    [SerializeField]
    private BoxCollider BoxCollider;

    public Transform HealthBarPivot;
    public InGameHealthBar InGameHealthBar;

    public override void OnHelperRecycled()
    {
        BoxCollider.enabled = false;
        InGameHealthBar?.PoolRecycle();
        InGameHealthBar = null;
        IsDestroying = false;
        base.OnHelperRecycled();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        IsDestroying = false;
    }

    public void Initialize()
    {
        BoxCollider.enabled = true;
        Transform trans = UIManager.Instance.ShowUIForms<InGameUIPanel>().transform;
        InGameHealthBar = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.InGameHealthBar].AllocateGameObject<InGameHealthBar>(trans);
        InGameHealthBar.Initialize(this, 100, 30);
    }

    void FixedUpdate()
    {
    }

    #region Life & Health

    public UnityAction<int> OnDamaged;

    public void ShowDamageNumFX(int damage)
    {
        if (damage == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, damage, BattleTipType.Damage, 0, 0));
        OnDamaged?.Invoke(damage);
        FX injureFX = FXManager.Instance.PlayFX(Actor.InjureFX, Actor.transform.position);
    }

    public UnityAction<int> OnHealed;

    public void ShowHealNumFX(int addHealth)
    {
        if (addHealth == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, addHealth, BattleTipType.Heal, 0, 0));
        OnHealed?.Invoke(addHealth);

        FX healFX = FXManager.Instance.PlayFX(Actor.HealFX, Actor.transform.position);
    }

    #endregion

    #region Die

    public bool IsDestroying = false;

    public void DestroyActor(UnityAction callBack = null, bool forModuleRecycle = false)
    {
        if (IsDestroying) return;
        IsDestroying = true;
        if (!forModuleRecycle)
        {
            foreach (EntityPassiveSkill ps in Actor.EntityPassiveSkills)
            {
                ps.OnBeforeDestroyEntity();
            }
        }

        if (Actor is EnemyActor && WorldManager.Instance.CurrentWorld is OpenWorld openWorld)
        {
            GridPos3D actorModuleGP = WorldManager.Instance.CurrentWorld.GetModuleGPByWorldGP(Actor.WorldGP);
            WorldModule module = WorldManager.Instance.CurrentWorld.WorldModuleMatrix[actorModuleGP.x, actorModuleGP.y, actorModuleGP.z];
            BornPointData mBornPointData = WorldManager.Instance.CurrentWorld.WorldData.WorldBornPointGroupData_Runtime.GetBornPointDataByGUID(Actor.BornPointDataGUID);
            if (module is OpenWorldModule)
            {
                if (forModuleRecycle)
                {
                    // Enemy没死，但迁移到了其他模组，需要将出生点数据转移
                    if (mBornPointData == null)
                    {
                        Debug.LogError("角色未死亡但BornPointData已经遗失");
                    }
                    else
                    {
                        mBornPointData.ActorOrientation = Actor.EntityOrientation;
                        Actor.SwitchEntityOrientation(GridPosR.Orientation.Up); // 需要将Actor的朝向转回正，才能读取到原WorldGP
                        GridPos3D actorFaceUpWorldGP = Actor.WorldGP;
                        Actor.SwitchEntityOrientation(mBornPointData.ActorOrientation); // 复原朝向
                        GridPos3D oriModuleGP = openWorld.GetModuleGPByWorldGP(mBornPointData.WorldGP);
                        GridPos3D destModuleGP = openWorld.GetModuleGPByWorldGP(actorFaceUpWorldGP);
                        openWorld.WorldData.WorldBornPointGroupData_Runtime.ChangeBornPointDataBetweenModules_ForOpenWorldModule(mBornPointData.GUID, oriModuleGP, destModuleGP);
                        mBornPointData.WorldGP = actorFaceUpWorldGP;
                        mBornPointData.LocalGP = mBornPointData.WorldGP - destModuleGP * WorldModule.MODULE_SIZE;
                    }
                }
                else
                {
                    // Enemy死亡，移除出生点数据
                    openWorld.WorldData.WorldBornPointGroupData_Runtime.UnRegisterBornPointData_OpenWorldEnemyDied(mBornPointData.GUID);
                }
            }
        }

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Co_DelayDestroyActor(callBack, forModuleRecycle));
        }
    }

    IEnumerator Co_DelayDestroyActor(UnityAction callBack, bool forModuleRecycle = false)
    {
        yield return new WaitForSeconds(0.1f);
        foreach (EntityPassiveSkill ps in Actor.EntityPassiveSkills)
        {
            ps.OnDestroyEntity();
        }

        if (Actor.IsPlayerCamp)
        {
            BattleManager.Instance.LoseGame();
        }
        else
        {
            if (LastAttackBox != null && LastAttackBox.LastInteractActor.IsNotNullAndAlive())
            {
                if (LastAttackBox.LastInteractActor.IsPlayerCamp)
                {
                    // todo 玩家击杀
                }
            }

            if (Actor.ActorFrozenHelper.FrozenBox)
            {
                Actor.ActorFrozenHelper.FrozenBox.DestroyBox(null, forModuleRecycle);
                Actor.ActorFrozenHelper.FrozenBox = null;
            }

            if (!forModuleRecycle)
            {
                FX hit = FXManager.Instance.PlayFX(Actor.DieFX, transform.position);
            }

            Actor.PoolRecycle();
        }

        callBack?.Invoke();
    }

    #endregion

    #region Money

    public UnityAction<int> OnGainGold;

    public void ShowGainGoldNumFX(int gold)
    {
        if (gold == 0) return;
        ClientGameManager.Instance.BattleMessenger.Broadcast((uint) ENUM_BattleEvent.Battle_ActorNumeralTip, new NumeralUIBattleTipData(Actor.Camp, Actor.transform.position, gold, BattleTipType.Gold, 0, 0));
        OnGainGold?.Invoke(gold);
    }

    #endregion
}