using BiangLibrary;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR

#endif

public class BornPointDesignHelper : MonoBehaviour
{
    [HideLabel]
    [BoxGroup("出生点配置")]
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

        string goName = PlayerNumber.Player1.ToString();
        goName += string.IsNullOrEmpty(BornPointData.BornPointAlias) ? "" : "_" + BornPointData.BornPointAlias;
        dirty = !gameObject.name.Equals(goName);
        gameObject.name = goName;
        return dirty;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Color gizmosColor = "#FF133A".HTMLColorToColor();
            Gizmos.color = gizmosColor;
            Gizmos.DrawSphere(transform.position + Vector3.left * 0.25f + Vector3.forward * 0.25f, 0.1f);
            transform.DrawSpecialTip(Vector3.up + Vector3.left * 0.15f + Vector3.forward * -0.2f, gizmosColor, gizmosColor, PlayerNumber.Player1.ToString());
        }
    }

#endif
}