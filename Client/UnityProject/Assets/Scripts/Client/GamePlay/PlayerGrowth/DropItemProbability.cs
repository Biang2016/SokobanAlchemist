using System;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;

[Serializable]
public class DropItemProbability : IClone<DropItemProbability>
{
    public string Description => $"{ItemType.TypeSelection} * {ProbabilityMin}~{ProbabilityMax}";

    [LabelText("掉落概率小值")]
    public float ProbabilityMin = 0;

    [LabelText("掉落概率大值")]
    public float ProbabilityMax = 0;

    [LabelText("类型")]
    public TypeSelectHelper ItemType = new TypeSelectHelper();

    public DropItemProbability Clone()
    {
        DropItemProbability cloneData = new DropItemProbability();
        cloneData.ItemType = ItemType.Clone();
        cloneData.ProbabilityMin = ProbabilityMin;
        cloneData.ProbabilityMax = ProbabilityMax;
        return cloneData;
    }
}