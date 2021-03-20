using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class WorldNameWithProbability : Probability, IClone<WorldNameWithProbability>
{
    public string Description => $"{WorldTypeName}";

    [LabelText("@\"世界类型\t\"+WorldTypeName")]
    public TypeSelectHelper WorldTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.World};

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
        probability = src.probability;
        isSingleton = src.isSingleton;
    }
}