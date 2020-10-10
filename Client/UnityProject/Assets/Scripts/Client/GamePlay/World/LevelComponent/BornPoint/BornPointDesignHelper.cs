using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class BornPointDesignHelper : MonoBehaviour
{
    public BornPointData BornPointData;

#if UNITY_EDITOR
    [HideInPlayMode]
    [HideInPrefabAssets]
    [Button("命名重置为怪物名称", ButtonSizes.Large)]
    [GUIColor(0f, 1f, 0f)]
    private void FormatAllName_Editor()
    {
        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();
        WorldModuleDesignHelper module = GetComponentInParent<WorldModuleDesignHelper>();
        if (world && module)
        {
            Debug.LogError("无法重命名模组内的BornPoint");
            return;
        }

        gameObject.name = "BornPoint_" + BornPointData.ActorType;
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