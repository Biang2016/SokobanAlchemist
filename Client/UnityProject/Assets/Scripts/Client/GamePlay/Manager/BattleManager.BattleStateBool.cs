using System.Collections.Generic;

public partial class BattleManager
{
    public Dictionary<string, bool> BattleStateBoolDict = new Dictionary<string, bool>();

    public bool GetStateBool(string stateAlias)
    {
        if (BattleStateBoolDict.TryGetValue(stateAlias, out bool value))
        {
            return value;
        }

        return false;
    }

    public void SetStateBool(string stateAlias, bool value)
    {
        if (BattleStateBoolDict.ContainsKey(stateAlias))
        {
            BattleStateBoolDict[stateAlias] = value;
        }
        else
        {
            BattleStateBoolDict.Add(stateAlias, value);
        }
    }
}