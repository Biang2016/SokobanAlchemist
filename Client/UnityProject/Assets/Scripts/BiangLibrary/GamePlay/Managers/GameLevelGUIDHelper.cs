#if UNITY_EDITOR
using System;
using System.Collections.Generic;

public static class GameLevelGUIDHelper
{
    private static Dictionary<object, string> dictMapGuid = new Dictionary<object, string>();
    private static HashSet<string> setUUid = new HashSet<string>();

    public static string GetGUIDFromObject(object anObject)
    {
        if (dictMapGuid.ContainsKey(anObject))
        {
            return dictMapGuid[anObject];
        }
        else
        {
            string newUUID = Guid.NewGuid().ToString();

            while (setUUid.Contains(newUUID))
            {
                newUUID = Guid.NewGuid().ToString();
            }

            dictMapGuid.Add(anObject, newUUID);
        }

        return dictMapGuid[anObject];
    }
}
#endif