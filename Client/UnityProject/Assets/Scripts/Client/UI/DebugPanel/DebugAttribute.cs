using System;

[AttributeUsage(AttributeTargets.Method)]
public class DebugControllerAttribute : Attribute
{
    public int Priority = 0;

    public DebugControllerAttribute(int priority)
    {
        Priority = priority;
    }
}

public class DebugButtonAttribute : DebugControllerAttribute
{
    public DebugButtonAttribute(string buttonName, int priority = 0) : base(priority)
    {
        ButtonName = buttonName;
    }

    public DebugButtonAttribute(string buttonName, string methodName, int priority = 0) : base(priority)
    {
        ButtonName = buttonName;
        MethodName = methodName;
    }

    public string ButtonName { get; }
    public string MethodName { get; }
}

public class DebugSliderAttribute : DebugControllerAttribute
{
    public DebugSliderAttribute(string sliderName, float defaultValue, float min, float max, int priority = 0) : base(priority)
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

public class DebugToggleButtonAttribute : DebugControllerAttribute
{
    public DebugToggleButtonAttribute(string buttonName, int priority = 0) : base(priority)
    {
        ButtonName = buttonName;
    }

    public string ButtonName { get; }
}