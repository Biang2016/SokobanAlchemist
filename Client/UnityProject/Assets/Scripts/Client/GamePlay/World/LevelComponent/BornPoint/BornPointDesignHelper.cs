using BiangLibrary;
using UnityEngine;

#if UNITY_EDITOR

#endif

public class BornPointDesignHelper : MonoBehaviour
{
    public BornPointData BornPointData;

#if UNITY_EDITOR

    public uint ProbablyShow
    {
        get
        {
            if (BornPointData.RawEntityExtraSerializeData != null)
            {
                foreach (EntityPassiveSkill eps in BornPointData.RawEntityExtraSerializeData.EntityPassiveSkills)
                {
                    if (eps is EntityPassiveSkill_ProbablyShow ps) return ps.ShowProbabilityPercent;
                }
            }

            return 100;
        }
    }

    public bool FormatAllName_Editor()
    {
        bool dirty = false;
        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();
        WorldModuleDesignHelper module = GetComponentInParent<WorldModuleDesignHelper>();
        if (world && module)
        {
            return false;
        }

        string goName = "BornPoint_" + BornPointData.ActorTypeName;
        goName += string.IsNullOrEmpty(BornPointData.BornPointAlias) ? "" : "_" + BornPointData.BornPointAlias;
        dirty = !gameObject.name.Equals(goName);
        gameObject.name = goName;
        return dirty;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Color gizmosColor;
            if (BornPointData.ActorCategory == ActorCategory.Player)
            {
                gizmosColor = "#FF133A".HTMLColorToColor();
            }
            else
            {
                gizmosColor = "#9DFF01".HTMLColorToColor();
            }

            Gizmos.color = gizmosColor;
            Gizmos.DrawSphere(transform.position + Vector3.left * 0.25f + Vector3.forward * 0.25f, 0.1f);
            transform.DrawSpecialTip(Vector3.up + Vector3.left * 0.15f + Vector3.forward * -0.2f, gizmosColor, gizmosColor, BornPointData.ActorTypeName.Replace("Enemy", ""));
            if (ProbablyShow < 100)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f + Vector3.forward * 0.05f, Color.clear, Color.yellow, $"{ProbablyShow}%现");
            }
        }
    }

#endif
}