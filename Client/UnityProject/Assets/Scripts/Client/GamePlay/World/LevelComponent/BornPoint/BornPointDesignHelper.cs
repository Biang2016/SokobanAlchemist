using BiangLibrary;
using UnityEngine;

#if UNITY_EDITOR

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
            gizmosColor = "#FF133A".HTMLColorToColor();
        }
        else
        {
            gizmosColor = "#9DFF01".HTMLColorToColor();
        }

        Gizmos.color = gizmosColor;
        Gizmos.DrawSphere(transform.position + Vector3.left * 0.25f + Vector3.forward * 0.25f, 0.25f);
        transform.DrawSpecialTip(Vector3.up + Vector3.left * 0.15f, gizmosColor, gizmosColor, BornPointData.ActorType.Replace("Enemy", ""));
    }
#endif
}