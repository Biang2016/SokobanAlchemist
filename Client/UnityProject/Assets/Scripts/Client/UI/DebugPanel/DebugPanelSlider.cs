using BiangLibrary.ObjectPool;
using UnityEngine.Events;
using UnityEngine.UI;

public class DebugPanelSlider : DebugPanelComponent
{
    public Slider Slider;
    public Text Text;
    public Text ValueText;

    public override void OnRecycled()
    {
        Slider.onValueChanged.RemoveAllListeners();
        base.OnRecycled();
    }

    public void Initialize(string sliderName, float defaultValue, float min, float max, UnityAction<float> action)
    {
        name = "slider_" + sliderName;
        Text.text = sliderName;
        ValueText.text = defaultValue.ToString("F1");
        Slider.onValueChanged.RemoveAllListeners();
        Slider.minValue = min;
        Slider.maxValue = max;
        Slider.value = defaultValue;
        Slider.onValueChanged.AddListener((value) => { ValueText.text = value.ToString("F1"); });
        if (action != null) Slider.onValueChanged.AddListener(action);
    }
}