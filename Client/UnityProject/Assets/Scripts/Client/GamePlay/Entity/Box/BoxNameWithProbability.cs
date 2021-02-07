using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxNameWithProbability : Probability, IClone<BoxNameWithProbability>
{
    [BoxNameList]
    [ValueDropdown("GetAllBoxTypeNames", DropdownTitle = "选择箱子类型")]
    public string BoxTypeName;

    public GridPosR.Orientation BoxOrientation;

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();

    #endregion

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
        newData.BoxTypeName = BoxTypeName;
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
        BoxTypeName = src.BoxTypeName;
        BoxOrientation = src.BoxOrientation;
        probability = src.probability;
        isSingleton = src.isSingleton;
    }
}