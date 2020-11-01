using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

[Serializable]
public class BornPointData : LevelComponentData
{
    [BoxGroup("监听事件")]
    [LabelText("收到事件后刷怪(空则开场刷怪)")]
    public string SpawnLevelEventAlias;

    [ValueDropdown("GetAllActorNames")]
    [LabelText("角色类型")]
    public string ActorType = "None";

    [LabelText("出生点花名")]
    [ValidateInput("ValidateBornPointAlias", "请保证此项非空且唯一")]
    public string BornPointAlias = "";

    public bool ValidateBornPointAlias(string alias)
    {
        if (string.IsNullOrEmpty(alias))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public ActorCategory ActorCategory
    {
        get
        {
            if (ActorType.StartsWith("Player"))
            {
                return ActorCategory.Player;
            }
            else
            {
                return ActorCategory.Creature;
            }
        }
    }

    private IEnumerable<string> GetAllActorNames => ConfigManager.GetAllActorNames();

    protected override void ChildClone(LevelComponentData newData)
    {
        base.ChildClone(newData);
        BornPointData data = ((BornPointData) newData);
        data.SpawnLevelEventAlias = SpawnLevelEventAlias;
        data.ActorType = ActorType;
        data.BornPointAlias = BornPointAlias;
    }
}