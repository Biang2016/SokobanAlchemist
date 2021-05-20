using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[Serializable]
public class BoxMergeConfig
{
    internal bool MergeEnable = true;

    [LabelText("合并延迟")]
    public float MergeDelay = 0;

    [ListDrawerSettings(ListElementLabelName = "Description")]
    [LabelText("合成配方")]
    public List<BoxMergeConfigData> BoxMergeConfigDataList = new List<BoxMergeConfigData>();

    [ShowInInspector]
    [ReadOnly]
    [HideInEditorMode]
    internal string Temp_NextMergeEntityDataOverrideKey = "";

    [ShowInInspector]
    [ReadOnly]
    [HideInEditorMode]
    internal EntityData Temp_NextMergeEntityData;

    public ushort GetMergeBoxTypeIndex(int mergeCount, MergeOrientation mergeOrientation)
    {
        if (!MergeEnable) return 0;
        foreach (BoxMergeConfigData data in BoxMergeConfigDataList)
        {
            if (data.MergeCount == mergeCount && (data.MergeOrientation & mergeOrientation) != 0)
            {
                return ConfigManager.GetTypeIndex(TypeDefineType.Box, data.MergeBox.TypeName);
            }
        }

        return 0;
    }

    public List<int> GetAllValidMergeCounts(MergeOrientation mergeOrientation)
    {
        List<int> validMergeCounts = new List<int>();
        if (!MergeEnable) return validMergeCounts;
        foreach (BoxMergeConfigData data in BoxMergeConfigDataList)
        {
            if ((data.MergeOrientation & mergeOrientation) != 0)
            {
                validMergeCounts.Add(data.MergeCount);
            }
        }

        validMergeCounts.Sort((a, b) => -a.CompareTo(b));
        return validMergeCounts;
    }

    public void OnRecycled()
    {
        Temp_NextMergeEntityDataOverrideKey = "";
        Temp_NextMergeEntityData = null;
        MergeEnable = true;
    }

    public void OnUsed()
    {
        Temp_NextMergeEntityDataOverrideKey = "";
        Temp_NextMergeEntityData = null;
        MergeEnable = true;
    }
}

[Serializable]
public class BoxMergeConfigData
{
    public string Description => $"{MergeOrientation} Merge_{MergeCount}->{MergeBox.TypeName}";

    public int MergeCount = 3;

    public MergeOrientation MergeOrientation = MergeOrientation.XZ;

    [HideLabel]
    public TypeSelectHelper MergeBox = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};
}

[Flags]
public enum MergeOrientation
{
    X = 1 << 0,
    Z = 1 << 1,
    XZ = X | Z,
}