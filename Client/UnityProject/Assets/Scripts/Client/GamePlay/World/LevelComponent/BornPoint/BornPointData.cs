using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

[Serializable]
public class BornPointData : LevelComponentData
{
    [ValueDropdown("GetAllActorNames")]
    [LabelText("角色类型")]
    [FormerlySerializedAs("EnemyName")]
    public string ActorType;

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
        data.ActorType = ActorType;
    }
}