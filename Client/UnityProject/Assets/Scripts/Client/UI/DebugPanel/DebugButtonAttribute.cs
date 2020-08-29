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