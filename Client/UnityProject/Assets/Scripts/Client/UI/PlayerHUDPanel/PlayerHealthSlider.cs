using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthSlider : MonoBehaviour
{
    public Image SliderFillImage;
    public Slider Slider;
    public Animator SliderHandelAnim;
    public Text LifeText;
    public Animator LifeTextAnim;
    public Gradient LifeColorGradient;

    public void Initialize(ActorBattleHelper helper)
    {
        ActorStatPropSet asps = helper.Actor.ActorStatPropSet;
        SetHealthSliderValue(asps.Health.Value, asps.Health.MinValue, asps.Health.MaxValue);
        SetLife(asps.Life.Value, asps.Life.MinValue, asps.Life.MaxValue);
        asps.Health.OnChanged += SetHealthSliderValue;
        asps.Life.OnChanged += SetLife;
    }

    public void SetHealthSliderValue(int currentHealth, int minHealth, int maxHealth)
    {
        if (maxHealth == 0)
        {
            Slider.value = 0f;
        }
        else
        {
            Slider.value = (float) currentHealth / maxHealth;
            SliderHandelAnim.SetTrigger("Jump");
        }
    }

    public void SetLife(int currentLife, int minLife, int maxLife)
    {
        LifeTextAnim.SetTrigger("Jump");
        LifeText.text = currentLife.ToString();
        SliderFillImage.color = LifeColorGradient.Evaluate((float) currentLife / maxLife);
    }
}