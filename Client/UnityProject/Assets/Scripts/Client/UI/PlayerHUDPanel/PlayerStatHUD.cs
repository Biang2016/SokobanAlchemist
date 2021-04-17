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

        SetActionPoint(asps.ActionPoint.Value, asps.ActionPoint.MinValue, asps.ActionPoint.MaxValue);
        asps.ActionPoint.m_NotifyActionSet.OnChanged += SetActionPoint;

        SetGold(asps.Gold.Value);
        asps.Gold.m_NotifyActionSet.OnChanged += (value, min, max) => SetGold(value);

        SetFireElementFragment(asps.FireElementFragment.Value, asps.FireElementFragment.MinValue, asps.FireElementFragment.MaxValue);
        asps.FireElementFragment.m_NotifyActionSet.OnChanged += SetFireElementFragment;

        SetIceElementFragment(asps.IceElementFragment.Value, asps.IceElementFragment.MinValue, asps.IceElementFragment.MaxValue);
        asps.IceElementFragment.m_NotifyActionSet.OnChanged += SetIceElementFragment;

        SetLightningElementFragment(asps.LightningElementFragment.Value, asps.LightningElementFragment.MinValue, asps.LightningElementFragment.MaxValue);
        asps.LightningElementFragment.m_NotifyActionSet.OnChanged += SetLightningElementFragment;
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

    public Transform ActionPointsContainer;
    private List<ActionPointIndicator> ActionPointIndicators = new List<ActionPointIndicator>();

    public void SetActionPoint(int current, int min, int max)
    {
        if (ActionPointIndicators.Count > max)
        {
            while (ActionPointIndicators.Count != max)
            {
                ActionPointIndicators[ActionPointIndicators.Count - 1].PoolRecycle();
                ActionPointIndicators.RemoveAt(ActionPointIndicators.Count - 1);
            }
        }
        else if (ActionPointIndicators.Count < max)
        {
            while (ActionPointIndicators.Count != max)
            {
                ActionPointIndicator indicator = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.ActionPointIndicator].AllocateGameObject<ActionPointIndicator>(ActionPointsContainer);
                indicator.Available = false;
                ActionPointIndicators.Add(indicator);
            }
        }

        for (int i = 0; i < ActionPointIndicators.Count; i++)
        {
            ActionPointIndicators[i].Available = i < current;
        }
    }

    public void OnActionLowWarning()
    {
        foreach (ActionPointIndicator indicator in ActionPointIndicators)
        {
            indicator.JumpRed();
        }
    }

    #endregion

    #region 财产

    public TextMeshProUGUI GoldText;

    public void SetGold(int current)
    {
        GoldText.text = current.ToString();
    }

    public TextMeshProUGUI FireElementFragmentText;
    public Slider FireElementFragmentSlider;

    public void SetFireElementFragment(int current, int min, int max)
    {
        FireElementFragmentText.text = current.ToString();
        if (max == 0)
        {
            FireElementFragmentSlider.value = 0f;
        }
        else
        {
            FireElementFragmentSlider.value = (float) current / max;
        }
    }

    public TextMeshProUGUI IceElementFragmentTextText;
    public Slider IceElementFragmentSlider;

    public void SetIceElementFragment(int current, int min, int max)
    {
        IceElementFragmentTextText.text = current.ToString();
        if (max == 0)
        {
            IceElementFragmentSlider.value = 0f;
        }
        else
        {
            IceElementFragmentSlider.value = (float) current / max;
        }
    }

    public TextMeshProUGUI LightningElementFragmentText;
    public Slider LightningElementFragmentSlider;

    public void SetLightningElementFragment(int current, int min, int max)
    {
        LightningElementFragmentText.text = current.ToString();
        if (max == 0)
        {
            LightningElementFragmentSlider.value = 0f;
        }
        else
        {
            LightningElementFragmentSlider.value = (float) current / max;
        }
    }

    #endregion
}