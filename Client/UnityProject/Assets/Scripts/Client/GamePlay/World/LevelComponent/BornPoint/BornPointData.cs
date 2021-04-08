using System;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

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

    [LabelText("出生点花名")]
    [ValidateInput("ValidateBornPointAlias", "请保证此项非空时是唯一的；且一个模组只允许有一个玩家出生点花名为空")]
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

    protected override void ChildClone(LevelComponentData newData)
    {
        base.ChildClone(newData);
        BornPointData data = ((BornPointData) newData);
        data.BornPointAlias = BornPointAlias;
    }
}