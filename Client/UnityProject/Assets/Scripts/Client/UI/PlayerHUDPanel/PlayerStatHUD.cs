using UnityEngine;
using UnityEngine.UI;

public class PlayerStatHUD : MonoBehaviour
{
    public Image HealthSliderFillImage;
    public Slider HealthSlider;
    public Animator HealthSliderHandleAnim;
    public Text HealthText;
    public Animator HealthTextAnim;

    public Animator ActionPointSliderAnim;
    public Slider ActionPointSlider;
    public RectTransform ActionPointSliderRectTransform;
    private float actionPointRatio;

    public void Initialize(ActorBattleHelper helper)
    {
        EntityStatPropSet asps = helper.Actor.EntityStatPropSet;
        SetHealth(asps.HealthDurability.Value, asps.HealthDurability.MinValue, asps.HealthDurability.MaxValue);
        SetActionPoint(asps.ActionPoint.Value, asps.ActionPoint.MinValue, asps.ActionPoint.MaxValue);
        SetActionPointBar(asps.MaxActionPoint.GetModifiedValue, asps.MaxActionPoint.GetModifiedValue);
        asps.HealthDurability.OnChanged += SetHealth;
        asps.ActionPoint.OnChanged += SetActionPoint;
        asps.ActionPoint.OnMaxValueChanged += OnActionChangeNotice;
        asps.MaxActionPoint.OnValueChanged += SetActionPointBar;
    }

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

        HealthTextAnim.SetTrigger("Jump");
        HealthText.text = $"{current}/{max}";
    }

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
        ActionPointSliderAnim.SetTrigger("JumpYellow");
    }

    void Update()
    {
        ActionPointSlider.value = Mathf.SmoothDamp(ActionPointSlider.value, actionPointRatio, ref actionPoint_SmoothDampVelocity, 0.1f, 1, Time.deltaTime);
    }
}