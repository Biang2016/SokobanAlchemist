using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[Serializable]
public class BoxMergeConfig
{
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<BoxMergeConfigData> BoxMergeConfigDataList = new List<BoxMergeConfigData>();

    public ushort GetMergeBoxTypeIndex(int mergeCount, MergeOrientation mergeOrientation)
    {
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