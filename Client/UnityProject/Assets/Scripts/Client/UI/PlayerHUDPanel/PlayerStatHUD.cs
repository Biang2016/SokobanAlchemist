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

    public Animator ActionPointSliderAnim;
    public Slider ActionPointSlider;
    public RectTransform ActionPointSliderRectTransform;
    private float actionPointRatio;

    public TextMeshProUGUI GoldText;

    public void Initialize(ActorBattleHelper helper)
    {
        EntityStatPropSet asps = helper.Actor.EntityStatPropSet;
        SetHealth(asps.HealthDurability.Value, asps.HealthDurability.MinValue, asps.HealthDurability.MaxValue);
        SetActionPoint(asps.ActionPoint.Value, asps.ActionPoint.MinValue, asps.ActionPoint.MaxValue);
        SetActionPointBar(asps.MaxActionPoint.GetModifiedValue, asps.MaxActionPoint.GetModifiedValue);
        SetGold(asps.Gold.Value);

        asps.HealthDurability.OnChanged += SetHealth;
        asps.ActionPoint.OnChanged += SetActionPoint;
        asps.ActionPoint.OnMaxValueChanged += OnActionChangeNotice;
        asps.MaxActionPoint.OnValueChanged += SetActionPointBar;
        asps.Gold.OnChanged += (value, min, max) => SetGold(value);
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

    private float actionPoint_SmoothDampVelocity;

    public void SetActionPoint(int current, int min, int max)
    {
        actionPointRatio = (float) current / max;
    }

    public void SetActionPointBar(int before, int after)
    {
        ActionPointSliderRectTransform.sizeDelta = new Vector2(after / 50f * 600f, ActionPointSliderRectTransform.sizeDelta.y);
    }

    public void OnActionLowWarning()
    {
        ActionPointSliderAnim.SetTrigger("JumpRed");
    }

    private void OnActionChangeNotice(int before, int after)
    {
        if ((after - before) > 1)
        {
            ActionPointSliderAnim.SetTrigger("JumpYellow");
        }
    }

    void Update()
    {
        ActionPointSlider.value = Mathf.SmoothDamp(ActionPointSlider.value, actionPointRatio, ref actionPoint_SmoothDampVelocity, 0.1f, 1, Time.deltaTime);
    }

    #endregion

    #region Gold

    public void SetGold(int current)
    {
        GoldText.text = current.ToString();
    }

    #endregion
}