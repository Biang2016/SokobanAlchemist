using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatHUD : MonoBehaviour
{
    public void Initialize(ActorBattleHelper helper)
    {
        EntityStatPropSet asps = helper.Actor.EntityStatPropSet;

        SetHealth(asps.HealthDurability.Value, asps.HealthDurability.MinValue, asps.HealthDurability.MaxValue);
        asps.HealthDurability.m_NotifyActionSet.OnChanged += SetHealth;

        ActionPointBar.Initialize(EntityStatType.ActionPoint);
        ActionPointBar.SetStat(asps.ActionPoint.Value, asps.ActionPoint.MinValue, asps.ActionPoint.MaxValue);
        asps.ActionPoint.m_NotifyActionSet.OnChanged += ActionPointBar.SetStat;

        SetGold(asps.Gold.Value);
        asps.Gold.m_NotifyActionSet.OnChanged += (value, min, max) => SetGold(value);

        FireElementFragmentBar.Initialize(EntityStatType.FireElementFragment);
        FireElementFragmentBar.SetStat(asps.FireElementFragment.Value, asps.FireElementFragment.MinValue, asps.FireElementFragment.MaxValue);
        asps.FireElementFragment.m_NotifyActionSet.OnChanged += FireElementFragmentBar.SetStat;

        IceElementFragmentBar.Initialize(EntityStatType.IceElementFragment);
        IceElementFragmentBar.SetStat(asps.IceElementFragment.Value, asps.IceElementFragment.MinValue, asps.IceElementFragment.MaxValue);
        asps.IceElementFragment.m_NotifyActionSet.OnChanged += IceElementFragmentBar.SetStat;

        LightningElementFragmentBar.Initialize(EntityStatType.LightningElementFragment);
        LightningElementFragmentBar.SetStat(asps.LightningElementFragment.Value, asps.LightningElementFragment.MinValue, asps.LightningElementFragment.MaxValue);
        asps.LightningElementFragment.m_NotifyActionSet.OnChanged += LightningElementFragmentBar.SetStat;
    }

    #region Health

    public Gradient HealthSliderFillImageGradient;
    public Image HealthSliderFillImage;
    public Slider HealthSlider;
    public Animator HealthSliderHandleAnim;
    public TextMeshProUGUI HealthText;
    public Animator HealthTextAnim;

    public void SetHealth(int current, int min, int max)
    {
        if (max == 0)
        {
            HealthSlider.value = 0f;
        }
        else
        {
            HealthSlider.value = (float) current / max;
            HealthSliderHandleAnim.SetTrigger("Jump");
        }

        HealthSliderFillImage.color = HealthSliderFillImageGradient.Evaluate((float) current / max);
        HealthTextAnim.SetTrigger("Jump");
        HealthText.text = current.ToString();
    }

    #endregion

    #region ActionPoint

    public DiscreteStatPointBar ActionPointBar;

    #endregion

    #region 财产

    public TextMeshProUGUI GoldText;

    public void SetGold(int current)
    {
        GoldText.text = current.ToString();
    }

    public DiscreteStatPointBar FireElementFragmentBar;
    public DiscreteStatPointBar IceElementFragmentBar;
    public DiscreteStatPointBar LightningElementFragmentBar;

    #endregion
}