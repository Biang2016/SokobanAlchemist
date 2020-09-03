using UnityEngine;
using UnityEngine.UI;

public class HealthSlider : MonoBehaviour
{
    public Image SliderFillImage;
    public Slider Slider;
    public Animator SliderHandelAnim;
    public Text LifeText;
    public Animator LifeTextAnim;
    public Color[] LifeColors;

    public void Initialize(ActorBattleHelper helper)
    {
        SetHealthSliderValue(helper.Health, helper.MaxHealth);
        SetLife(helper.Life, helper.TotalLife);
        helper.OnHealthChanged = SetHealthSliderValue;
        helper.OnLifeChanged = SetLife;
    }

    public void SetHealthSliderValue(int left, int total)
    {
        if (total == 0)
        {
            Slider.value = 0f;
        }
        else
        {
            Slider.value = (float) left / total;
            SliderHandelAnim.SetTrigger("Jump");
        }
    }

    public void SetLife(int leftLife, int totalLife)
    {
        LifeTextAnim.SetTrigger("Jump");
        LifeText.text = leftLife.ToString();
        SliderFillImage.color = LifeColors[leftLife];
    }
}