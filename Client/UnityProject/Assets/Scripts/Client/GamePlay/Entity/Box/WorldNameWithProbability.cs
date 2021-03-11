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

    [BoxNameList]
    [ValueDropdown("GetAllWorldNames", DropdownTitle = "选择世界")]
    public string WorldTypeName;

    #region Utils

    private IEnumerable<string> GetAllWorldNames => ConfigManager.GetAllWorldNames();

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
        WorldNameWithProbability newData = new WorldNameWithProbability();
        newData.WorldTypeName = WorldTypeName;
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
        WorldTypeName = src.WorldTypeName;
        probability = src.probability;
        isSingleton = src.isSingleton;
    }
}