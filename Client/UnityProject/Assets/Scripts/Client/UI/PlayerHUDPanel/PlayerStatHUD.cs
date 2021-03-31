using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatHUD : MonoBehaviour
{
    public Gradient HealthSliderFillImageGradient;
    public Image HealthSliderFillImage;
    public Slider HealthSlider;
    public Animator HealthSliderHandleAnim;
    public TextMeshProUGUI HealthText;
    public Animator HealthTextAnim;

    public void Initialize(ActorBattleHelper helper)
    {
        EntityStatPropSet asps = helper.Actor.EntityStatPropSet;
        SetHealth(asps.HealthDurability.Value, asps.HealthDurability.MinValue, asps.HealthDurability.MaxValue);
        SetActionPoint(asps.ActionPoint.Value, asps.ActionPoint.MinValue, asps.ActionPoint.MaxValue);
        SetGold(asps.Gold.Value);

        asps.HealthDurability.m_NotifyActionSet.OnChanged += SetHealth;
        asps.ActionPoint.m_NotifyActionSet.OnChanged += SetActionPoint;
        asps.Gold.m_NotifyActionSet.OnChanged += (value, min, max) => SetGold(value);
    }

    #region Health

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

    #region Gold

    public TextMeshProUGUI GoldText;

    public void SetGold(int current)
    {
        GoldText.text = current.ToString();
    }

    #endregion
}