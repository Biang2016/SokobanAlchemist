using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatHUD : MonoBehaviour
{
    void Awake()
    {
        SkillSlotDict.Add(PlayerControllerHelper.KeyBind.Num1, SkillSlot_Num1);
        SkillSlotDict.Add(PlayerControllerHelper.KeyBind.Num2, SkillSlot_Num2);
        SkillSlotDict.Add(PlayerControllerHelper.KeyBind.Num3, SkillSlot_Num3);
        SkillSlotDict.Add(PlayerControllerHelper.KeyBind.Num4, SkillSlot_Num4);
    }

    public void Initialize(ActorBattleHelper helper)
    {
        EntityStatPropSet asps = helper.Actor.EntityStatPropSet;

        HealthBottle.RefreshValue(asps.HealthDurability.Value, asps.HealthDurability.MinValue, asps.HealthDurability.MaxValue);
        asps.HealthDurability.m_NotifyActionSet.OnChanged += HealthBottle.RefreshValue;

        ActionPointBottle.RefreshValue(asps.ActionPoint.Value, asps.ActionPoint.MinValue, asps.ActionPoint.MaxValue);
        asps.ActionPoint.m_NotifyActionSet.OnChanged += ActionPointBottle.RefreshValue;

        GoldBottle.Initialize();
        GoldBottle.RefreshValue(asps.Gold.Value, asps.Gold.MinValue, asps.Gold.MaxValue);
        asps.Gold.m_NotifyActionSet.OnChanged += GoldBottle.RefreshValue;

        FireElementBottle.Initialize(PlayerControllerHelper.KeyBind.J_RightTrigger);
        FireElementBottle.RefreshValue(asps.FireElementFragment.Value, asps.FireElementFragment.MinValue, asps.FireElementFragment.MaxValue);
        asps.FireElementFragment.m_NotifyActionSet.OnChanged += FireElementBottle.RefreshValue;

        IceElementBottle.Initialize(PlayerControllerHelper.KeyBind.K);
        IceElementBottle.RefreshValue(asps.IceElementFragment.Value, asps.IceElementFragment.MinValue, asps.IceElementFragment.MaxValue);
        asps.IceElementFragment.m_NotifyActionSet.OnChanged += IceElementBottle.RefreshValue;

        LightningElementBottle.Initialize(PlayerControllerHelper.KeyBind.L);
        LightningElementBottle.RefreshValue(asps.LightningElementFragment.Value, asps.LightningElementFragment.MinValue, asps.LightningElementFragment.MaxValue);
        asps.LightningElementFragment.m_NotifyActionSet.OnChanged += LightningElementBottle.RefreshValue;

        SkillSlot_Num1.Initialize(null);
        SkillSlot_Num2.Initialize(null);
        SkillSlot_Num3.Initialize(null);
        SkillSlot_Num4.Initialize(null);
    }

    #region Health & ActionPoint

    public SphereBottle HealthBottle;
    public SphereBottle ActionPointBottle;

    #endregion

    #region 财产

    public ElementBottle GoldBottle;
    public ElementBottle FireElementBottle;
    public ElementBottle IceElementBottle;
    public ElementBottle LightningElementBottle;

    #endregion

    #region SkillSlots

    public Dictionary<PlayerControllerHelper.KeyBind, SkillSlot> SkillSlotDict = new Dictionary<PlayerControllerHelper.KeyBind, SkillSlot>();

    public SkillSlot SkillSlot_Num1;
    public SkillSlot SkillSlot_Num2;
    public SkillSlot SkillSlot_Num3;
    public SkillSlot SkillSlot_Num4;

    #endregion
}