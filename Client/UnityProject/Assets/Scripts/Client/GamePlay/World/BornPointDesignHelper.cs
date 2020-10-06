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
        if (!world)
        {
            Debug.LogError("此功能只能在世界编辑器中使用");
            return;
        }

        if (BornPointData.BornPointType == BornPointType.Player)
        {
            gameObject.name = "BornPoint_" + BornPointData.PlayerNumber;
        }
        else
        {
            gameObject.name = "BornPoint_" + BornPointData.EnemyName;
        }
    }

    void OnDrawGizmos()
    {
        Color gizmosColor;
        if (BornPointData.BornPointType == BornPointType.Player)
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