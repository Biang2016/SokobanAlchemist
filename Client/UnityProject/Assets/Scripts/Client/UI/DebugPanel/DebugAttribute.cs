using System;

[AttributeUsage(AttributeTargets.Method)]
public class DebugButtonAttribute : Attribute
{
    public DebugButtonAttribute(string buttonName)
    {
        ButtonName = buttonName;
    }

    public string ButtonName { get; }
}

[AttributeUsage(AttributeTargets.Method)]
public class DebugSliderAttribute : Attribute
{
    public DebugSliderAttribute(string sliderName, float defaultValue, float min, float max)
    {
        SliderName = sliderName;
        DefaultValue = defaultValue;
        Min = min;
        Max = max;
    }

    public string SliderName { get; }
    public float DefaultValue { get; }
    public float Min { get; }
    public float Max { get; }
}