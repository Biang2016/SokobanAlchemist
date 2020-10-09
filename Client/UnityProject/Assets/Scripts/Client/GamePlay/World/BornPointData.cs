using System;
using System.Collections.Generic;
using BiangStudio.CloneVariant;
using BiangStudio.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BornPointData : IClone<BornPointData>
{
    public BornPointType BornPointType;

    [ShowIf("BornPointType", BornPointType.Player)]
    public PlayerNumber PlayerNumber;

    [HideInInspector]
    public GridPos3D GridPos3D;

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

    public BornPointData Clone()
    {
        BornPointData newData = new BornPointData();
        newData.BornPointType = BornPointType;
        newData.PlayerNumber = PlayerNumber;
        newData.GridPos3D = GridPos3D;
        newData.EnemyName = EnemyName;
        return newData;
    }
}

public enum BornPointType
{
    None,
    Player,
    Enemy,
}