using System.Collections.Generic;
using UnityEngine;

public class PlayerStatHUD : MonoBehaviour
{
    void Awake()
    {
        SkillSlotDict.Add(PlayerControllerHelper.KeyBind.J_West, FireElementBottle);
        SkillSlotDict.Add(PlayerControllerHelper.KeyBind.K_North, IceElementBottle);
        SkillSlotDict.Add(PlayerControllerHelper.KeyBind.L_East, LightningElementBottle);
        SkillSlotDict.Add(PlayerControllerHelper.KeyBind.Num1, SkillSlot_Num1);
        SkillSlotDict.Add(PlayerControllerHelper.KeyBind.Num2, SkillSlot_Num2);
        SkillSlotDict.Add(PlayerControllerHelper.KeyBind.Num3, SkillSlot_Num3);
        SkillSlotDict.Add(PlayerControllerHelper.KeyBind.Num4, SkillSlot_Num4);
    }

    public void Initialize(ActorBattleHelper helper)
    {
        SetAllComponentShown(true);
        EntityStatPropSet asps = helper.Actor.EntityStatPropSet;

        HealthBottle.RefreshValue(asps.HealthDurability.Value, asps.HealthDurability.MinValue, asps.HealthDurability.MaxValue);
        asps.HealthDurability.m_NotifyActionSet.OnChanged += HealthBottle.RefreshValue;

        ActionPointBottle.RefreshValue(asps.ActionPoint.Value, asps.ActionPoint.MinValue, asps.ActionPoint.MaxValue);
        asps.ActionPoint.m_NotifyActionSet.OnChanged += ActionPointBottle.RefreshValue;

        GoldBottle.Initialize();
        GoldBottle.RefreshValue(asps.Gold.Value, asps.Gold.MinValue, asps.Gold.MaxValue);
        asps.Gold.m_NotifyActionSet.OnChanged += GoldBottle.RefreshValue;

        FireElementBottle.Initialize();
        FireElementBottle.RefreshValue(asps.FireElementFragment.Value, asps.FireElementFragment.MinValue, asps.FireElementFragment.MaxValue);
        asps.FireElementFragment.m_NotifyActionSet.OnChanged += FireElementBottle.RefreshValue;

        IceElementBottle.Initialize();
        IceElementBottle.RefreshValue(asps.IceElementFragment.Value, asps.IceElementFragment.MinValue, asps.IceElementFragment.MaxValue);
        asps.IceElementFragment.m_NotifyActionSet.OnChanged += IceElementBottle.RefreshValue;

        LightningElementBottle.Initialize();
        LightningElementBottle.RefreshValue(asps.LightningElementFragment.Value, asps.LightningElementFragment.MinValue, asps.LightningElementFragment.MaxValue);
        asps.LightningElementFragment.m_NotifyActionSet.OnChanged += LightningElementBottle.RefreshValue;

        foreach (KeyValuePair<PlayerControllerHelper.KeyBind, ISkillBind> kv in SkillSlotDict)
        {
            kv.Value.BindSkill(null);
        }
    }

    [SerializeField]
    private GameObject BGBar;

    public void SetComponentShown(HUDComponent component, bool shown)
    {
        switch (component)
        {
            case HUDComponent.HealthBottle:
            {
                HealthBottle.gameObject.SetActive(shown);
                break;
            }
            case HUDComponent.ActionPointBottle:
            {
                ActionPointBottle.gameObject.SetActive(shown);
                break;
            }
            case HUDComponent.BGBar:
            {
                BGBar.SetActive(shown);
                break;
            }
            case HUDComponent.GoldBottle:
            {
                GoldBottle.gameObject.SetActive(shown);
                break;
            }
            case HUDComponent.FireElementBottle:
            {
                FireElementBottle.gameObject.SetActive(shown);
                break;
            }
            case HUDComponent.IceElementBottle:
            {
                IceElementBottle.gameObject.SetActive(shown);
                break;
            }
            case HUDComponent.LightningElementBottle:
            {
                LightningElementBottle.gameObject.SetActive(shown);
                break;
            }
            case HUDComponent.SkillSlots:
            {
                SkillSlots.gameObject.SetActive(shown);
                break;
            }
        }
    }

    public void SetAllComponentShown(bool shown)
    {
        HealthBottle.gameObject.SetActive(shown);
        ActionPointBottle.gameObject.SetActive(shown);
        BGBar.SetActive(shown);
        GoldBottle.gameObject.SetActive(shown);
        FireElementBottle.gameObject.SetActive(shown);
        IceElementBottle.gameObject.SetActive(shown);
        LightningElementBottle.gameObject.SetActive(shown);
        SkillSlots.gameObject.SetActive(shown);
    }

    public enum HUDComponent
    {
        HealthBottle,
        ActionPointBottle,
        BGBar,
        GoldBottle,
        FireElementBottle,
        IceElementBottle,
        LightningElementBottle,
        SkillSlots
    }

    public void ShowEveryElements()
    {
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

    public Dictionary<PlayerControllerHelper.KeyBind, ISkillBind> SkillSlotDict = new Dictionary<PlayerControllerHelper.KeyBind, ISkillBind>();

    public HashSet<PlayerControllerHelper.KeyBind> JKL_KeyBinds = new HashSet<PlayerControllerHelper.KeyBind>
    {
        PlayerControllerHelper.KeyBind.J_West, PlayerControllerHelper.KeyBind.K_North, PlayerControllerHelper.KeyBind.L_East
    };

    public GameObject SkillSlots;
    public SkillSlot SkillSlot_Num1;
    public SkillSlot SkillSlot_Num2;
    public SkillSlot SkillSlot_Num3;
    public SkillSlot SkillSlot_Num4;

    #endregion
}