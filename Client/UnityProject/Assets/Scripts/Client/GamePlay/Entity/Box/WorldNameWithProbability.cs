using System;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class WorldNameWithProbability : Probability, IClone<WorldNameWithProbability>
{
    public string Description => $"{WorldTypeName}";

    [HideLabel]
    public TypeSelectHelper WorldTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.World};

    public int GoldCost = 0;

    [SerializeField]
    private int probability;

    public int Probability
    {
        get { return probability; }
        set { probability = value; }
    }

    [SerializeField]
    private bool isSingleton;

    public bool IsSingleton
    {
        get { return isSingleton; }
        set { isSingleton = value; }
    }

    public Probability ProbabilityClone()
    {
        WorldNameWithProbability newData = new WorldNameWithProbability();
        newData.WorldTypeName = WorldTypeName.Clone();
        newData.GoldCost = GoldCost;
        newData.probability = probability;
        newData.isSingleton = isSingleton;
        return newData;
    }

    public WorldNameWithProbability Clone()
    {
        return (WorldNameWithProbability) ProbabilityClone();
    }

    public void CopyDataFrom(WorldNameWithProbability src)
    {
        WorldTypeName.CopyDataFrom(src.WorldTypeName);
        GoldCost = src.GoldCost;
        probability = src.probability;
        isSingleton = src.isSingleton;
    }
}