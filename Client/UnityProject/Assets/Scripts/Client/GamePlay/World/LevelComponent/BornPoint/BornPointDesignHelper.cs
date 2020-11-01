using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class BornPointDesignHelper : MonoBehaviour
{
    public BornPointData BornPointData;

#if UNITY_EDITOR
    public bool FormatAllName_Editor()
    {
        bool dirty = false;
        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();
        WorldModuleDesignHelper module = GetComponentInParent<WorldModuleDesignHelper>();
        if (world && module)
        {
            return false;
        }

        string goName = "BornPoint_" + BornPointData.ActorType;
        goName += string.IsNullOrEmpty(BornPointData.BornPointAlias) ? "" : "_" + BornPointData.BornPointAlias;
        dirty = !gameObject.name.Equals(goName);
        gameObject.name = goName;
        return dirty;
    }

    void OnDrawGizmos()
    {
        Color gizmosColor;
        if (BornPointData.ActorType == "Player1")
        {
            gizmosColor = Color.yellow;
        }
        else
        {
            gizmosColor = Color.white;
        }

        Gizmos.color = gizmosColor;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
#endif
}