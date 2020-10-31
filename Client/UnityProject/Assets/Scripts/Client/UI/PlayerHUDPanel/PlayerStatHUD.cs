using UnityEngine;
using UnityEngine.UI;

public class PlayerStatHUD : MonoBehaviour
{
    public Image HealthSliderFillImage;
    public Slider HealthSlider;
    public Animator HealthSliderHandleAnim;
    public Text HealthText;
    public Animator HealthTextAnim;
    public Gradient HealthBarColorGradient;

    public Slider ActionPointSlider;
    public RectTransform ActionPointSliderRectTransform;
    private float actionPointRatio;

    public void Initialize(ActorBattleHelper helper)
    {
        ActorStatPropSet asps = helper.Actor.ActorStatPropSet;
        SetHealth(asps.Health.Value, asps.Health.MinValue, asps.Health.MaxValue);
        SetLife(asps.Life.Value, asps.Life.MinValue, asps.Life.MaxValue);
        SetActionPoint(asps.ActionPoint.Value, asps.ActionPoint.MinValue, asps.ActionPoint.MaxValue);
        SetActionPointBar(asps.MaxActionPoint.GetModifiedValue, asps.MaxActionPoint.GetModifiedValue);
        asps.Health.OnChanged += SetHealth;
        asps.Life.OnChanged += SetLife;
        asps.ActionPoint.OnChanged += SetActionPoint;
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

    public void SetLife(int current, int min, int max)
    {
        HealthSliderFillImage.color = HealthBarColorGradient.Evaluate((float) current / max);
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

    void Update()
    {
        ActionPointSlider.value = Mathf.SmoothDamp(ActionPointSlider.value, actionPointRatio, ref actionPoint_SmoothDampVelocity, 0.1f, 1, Time.deltaTime);
    }
}