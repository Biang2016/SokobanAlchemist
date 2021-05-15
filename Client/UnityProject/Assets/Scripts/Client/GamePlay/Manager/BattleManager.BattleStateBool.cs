using System;
using System.Collections.Generic;

public partial class BattleManager
{
    public Dictionary<uint, Dictionary<string, BattleStateBool>> BattleStateBoolDict_ByModule = new Dictionary<uint, Dictionary<string, BattleStateBool>>();
    public Dictionary<string, BattleStateBool> BattleStateBoolDict = new Dictionary<string, BattleStateBool>();

    public void ClearAllStateBools()
    {
        BattleStateBoolDict_ByModule.Clear();
        BattleStateBoolDict.Clear();
    }

    public bool GetStateBool(string stateAlias)
    {
        if (string.IsNullOrWhiteSpace(stateAlias)) return false;
        if (BattleStateBoolDict.TryGetValue(stateAlias, out BattleStateBool bsb))
        {
            return bsb.Value;
        }

        return false;
    }

    public void SetStateBool(uint worldModuleGUID, string stateAlias, bool value)
    {
        if (!BattleStateBoolDict_ByModule.ContainsKey(worldModuleGUID))
        {
            BattleStateBoolDict_ByModule.Add(worldModuleGUID, new Dictionary<string, BattleStateBool>());
        }

        if (BattleStateBoolDict_ByModule[worldModuleGUID].ContainsKey(stateAlias))
        {
            BattleStateBoolDict_ByModule[worldModuleGUID][stateAlias].Value = value;
        }
        else
        {
            BattleStateBoolDict_ByModule[worldModuleGUID].Add(stateAlias, new BattleStateBool(worldModuleGUID, stateAlias, value));
        }

        if (BattleStateBoolDict.ContainsKey(stateAlias))
        {
            BattleStateBoolDict[stateAlias].Value = value;
        }
        else
        {
            BattleStateBoolDict.Add(stateAlias, new BattleStateBool(worldModuleGUID, stateAlias, value));
        }
    }

    public void OnRecycleWorldModule(uint worldModuleGUID)
    {
        if (BattleStateBoolDict_ByModule.TryGetValue(worldModuleGUID, out Dictionary<string, BattleStateBool> dict))
        {
            foreach (KeyValuePair<string, BattleStateBool> kv in dict)
            {
                BattleStateBoolDict.Remove(kv.Key);
            }

            BattleStateBoolDict_ByModule.Remove(worldModuleGUID);
        }
    }

    [Serializable]
    public class BattleStateBool
    {
        public uint WorldModuleGUID;
        public string StateAlias;
        public bool Value;

        public BattleStateBool(uint worldModuleGUID, string stateAlias, bool value)
        {
            WorldModuleGUID = worldModuleGUID;
            StateAlias = stateAlias;
            Value = value;
        }
    }
}