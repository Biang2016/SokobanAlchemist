using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BoxNameWithProbability : Probability, IClone<BoxNameWithProbability>
{
    [BoxNameList]
    [ValueDropdown("GetAllBoxTypeNames", DropdownTitle = "选择箱子类型")]
    public string BoxTypeName;

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
        newData.probability = probability;
        newData.isSingleton = isSingleton;
        return newData;
    }

    public BoxNameWithProbability Clone()
    {
        return (BoxNameWithProbability) ProbabilityClone();
    }
}