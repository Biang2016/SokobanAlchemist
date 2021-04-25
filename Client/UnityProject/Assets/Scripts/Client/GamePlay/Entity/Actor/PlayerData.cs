﻿using System.Collections;
using BiangLibrary.CloneVariant;
using UnityEngine;

public class PlayerData : IClone<PlayerData>
{
    public EntityStatPropSet EntityStatPropSet = new EntityStatPropSet();
    public ActorSkillLearningData ActorSkillLearningData;

    public PlayerData()
    {
        EntityStatPropSet.Initialize(null);
    }

    public static PlayerData GetPlayerData()
    {
        PlayerData playerData = new PlayerData();
        BattleManager.Instance.Player1.EntityStatPropSet.ApplyDataTo(playerData.EntityStatPropSet);
        playerData.ActorSkillLearningData = BattleManager.Instance.Player1.ActorSkillLearningHelper.ActorSkillLearningData.Clone();
        return playerData;
    }

    public void ApplyDataOnPlayer()
    {
        BattleManager.Instance.Player1.ReloadESPS(EntityStatPropSet);
        BattleManager.Instance.Player1.ReloadActorSkillLearningData(ActorSkillLearningData);
    }

    public PlayerData Clone()
    {
        PlayerData cloneData = new PlayerData();
        EntityStatPropSet.ApplyDataTo(cloneData.EntityStatPropSet);
        cloneData.ActorSkillLearningData = ActorSkillLearningData.Clone();
        return cloneData;
    }
}