using System;
using UnityEngine;

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
    public DebugButtonAttribute(string buttonName, KeyCode shortcut = KeyCode.None, int priority = 0) : base(priority)
    {
        ButtonName = buttonName;
        Shortcut = shortcut;
    }

    public DebugButtonAttribute(string buttonName, string methodName_1, int priority = 0) : base(priority)
    {
        ButtonName = buttonName;
        MethodName_1 = methodName_1;
    }

    public DebugButtonAttribute(string buttonName, string methodName_1, string methodName_2, int priority = 0) : base(priority)
    {
        ButtonName = buttonName;
        MethodName_1 = methodName_1;
        MethodName_2 = methodName_2;
    }

    public string ButtonName { get; }
    public KeyCode Shortcut { get; }
    public string MethodName_1 { get; }
    public string MethodName_2 { get; }
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