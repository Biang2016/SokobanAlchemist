using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[Serializable]
public class BornPointData : LevelComponentData
{
    public BornPointType BornPointType;

    [ShowIf("BornPointType", BornPointType.Player)]
    public PlayerNumber PlayerNumber;

    [ValueDropdown("GetAllEnemyNames")]
    [ShowIf("BornPointType", BornPointType.Enemy)]
    public string EnemyName;

    private List<string> GetAllEnemyNames()
    {
        List<string> res = new List<string>();
        foreach (KeyValuePair<string, ushort> kv in ConfigManager.EnemyTypeDefineDict.TypeIndexDict)
        {
            res.Add(kv.Key);
        }

        return res;
    }

    protected override void ChildClone(LevelComponentData newData)
    {
        base.ChildClone(newData);
        BornPointData data = ((BornPointData) newData);
        data.BornPointType = BornPointType;
        data.PlayerNumber = PlayerNumber;
        data.EnemyName = EnemyName;
    }
}

public enum BornPointType
{
    None,
    Player,
    Enemy,
}