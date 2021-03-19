using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class BornPointData : LevelComponentData
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    public uint GUID;

    private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.BornPointData;

    protected uint GetGUID()
    {
        return guidGenerator++;
    }

    internal void InitGUID()
    {
        if (GUID == 0)
        {
            GUID = GetGUID();
        }
    }

    #endregion

    [BoxGroup("监听事件")]
    [LabelText("收到事件后刷怪(空则开场刷怪)")]
    public string SpawnLevelEventAlias;

    public bool IsPlayer = false;

    [LabelText("角色类型")]
    [HideIf("IsPlayer")]
    public TypeSelectHelper EnemyType = new TypeSelectHelper {TypeDefineType = TypeDefineType.Enemy};

    [LabelText("角色朝向")]
    [EnumToggleButtons]
    public GridPosR.Orientation ActorOrientation;

    [LabelText("出生点花名")]
    [ValidateInput("ValidateBornPointAlias", "请保证此项非空时是唯一的；且一个模组只允许有一个玩家出生点花名为空")]
    public string BornPointAlias = "";

    [LabelText("角色额外数据")]
    public EntityExtraSerializeData RawEntityExtraSerializeData = new EntityExtraSerializeData(); // 干数据，禁修改

    public bool ValidateBornPointAlias(string alias)
    {
        if (ActorCategory == ActorCategory.Player)
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
        else
        {
            return true;
        }
    }

    public ActorCategory ActorCategory
    {
        get
        {
            if (IsPlayer)
            {
                return ActorCategory.Player;
            }
            else
            {
                return ActorCategory.Creature;
            }
        }
    }

    protected override void ChildClone(LevelComponentData newData)
    {
        base.ChildClone(newData);
        BornPointData data = ((BornPointData) newData);
        data.SpawnLevelEventAlias = SpawnLevelEventAlias;
        data.IsPlayer = IsPlayer;
        data.EnemyType = EnemyType.Clone();
        data.ActorOrientation = ActorOrientation;
        data.BornPointAlias = BornPointAlias;
        data.RawEntityExtraSerializeData = RawEntityExtraSerializeData?.Clone();
    }
}