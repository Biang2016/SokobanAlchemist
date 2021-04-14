using System;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxNameWithProbability : Probability, IClone<BoxNameWithProbability>
{
    public string Description => $"{BoxTypeName}";

    [HideLabel]
    public TypeSelectHelper BoxTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};

    [EnumToggleButtons]
    public GridPosR.Orientation BoxOrientation;

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
        BoxNameWithProbability newData = new BoxNameWithProbability();
        newData.BoxTypeName = BoxTypeName.Clone();
        newData.BoxOrientation = BoxOrientation;
        newData.probability = probability;
        newData.isSingleton = isSingleton;
        return newData;
    }

    public BoxNameWithProbability Clone()
    {
        return (BoxNameWithProbability) ProbabilityClone();
    }

    public void CopyDataFrom(BoxNameWithProbability src)
    {
        BoxTypeName.CopyDataFrom(src.BoxTypeName);
        BoxOrientation = src.BoxOrientation;
        probability = src.probability;
        isSingleton = src.isSingleton;
    }
}