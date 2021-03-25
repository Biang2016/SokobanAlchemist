using System;
using System.Collections.Generic;
using System.Text;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class Box_LevelEditor : MonoBehaviour
{
    public GameObject ModelRoot;
    public GameObject BoxIndicatorHelperGO;

    [BoxGroup("箱子额外数据")]
    [HideLabel]
    public EntityExtraSerializeData RawEntityExtraSerializeData = new EntityExtraSerializeData(); // 干数据，禁修改

    [LabelText("箱子朝向")]
    [OnValueChanged("RefreshOrientation")]
    [EnumToggleButtons]
    public GridPosR.Orientation BoxOrientation;

    public bool RefreshOrientation()
    {
        bool dirty = Math.Abs(ModelRoot.transform.rotation.eulerAngles.y - (int) BoxOrientation * 90f) > 1f;
        if (dirty)
        {
            GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, BoxOrientation), ModelRoot.transform, 1);
            GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, BoxOrientation), BoxIndicatorHelperGO.transform, 1);
#if UNITY_EDITOR
            EditorWindow view = EditorWindow.GetWindow<SceneView>();
            view.Repaint();
#endif
        }

        return dirty;
    }

    public EntityExtraSerializeData GetBoxExtraSerializeData()
    {
        EntityExtraSerializeData data = new EntityExtraSerializeData();
        data.EntityPassiveSkills = new List<EntityPassiveSkill>();
        foreach (EntityPassiveSkill bf in RawEntityExtraSerializeData.EntityPassiveSkills)
        {
            if (bf is BoxPassiveSkill_LevelEventTriggerAppear) continue;
            data.EntityPassiveSkills.Add(bf.Clone());
        }

        return data;
    }

#if UNITY_EDITOR

    public bool RequireSerializePassiveSkillsIntoWorldModule => RawEntityExtraSerializeData.EntityPassiveSkills.Count > 0;

    public uint ProbablyShow
    {
        get
        {
            foreach (EntityPassiveSkill eps in RawEntityExtraSerializeData.EntityPassiveSkills)
            {
                if (eps is EntityPassiveSkill_ProbablyShow ps) return ps.ShowProbabilityPercent;
            }

            return 100;
        }
    }

    public bool LevelEventTriggerAppearInWorldModule
    {
        get
        {
            foreach (EntityPassiveSkill bf in RawEntityExtraSerializeData.EntityPassiveSkills)
            {
                if (bf is BoxPassiveSkill_LevelEventTriggerAppear appear) return true;
            }

            return false;
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (RequireSerializePassiveSkillsIntoWorldModule)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f + Vector3.forward * -0.2f, "#0AFFF1".HTMLColorToColor(), Color.cyan, "特殊");
            }

            if (ProbablyShow < 100)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f + Vector3.forward * 0.05f, Color.clear, Color.yellow, $"{ProbablyShow}%现");
            }

            if (LevelEventTriggerAppearInWorldModule)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f + Vector3.forward * 0.3f, Color.clear, Color.grey, "预隐");
            }
        }
    }

#endif

#if UNITY_EDITOR
    [HideInPlayMode]
    [HideInPrefabAssets]
    [ShowInInspector]
    [NonSerialized]
    [BoxGroup("快速替换")]
    [LabelText("@\"替换Box类型\t\"+ReplaceBoxTypeName")]
    private TypeSelectHelper ReplaceBoxTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};

    [HideInPlayMode]
    [HideInPrefabAssets]
    [BoxGroup("快速替换")]
    [Button("替换Box", ButtonSizes.Large)]
    [GUIColor(0f, 1f, 1f)]
    private void ReplaceBox_Editor()
    {
        WorldModuleDesignHelper module = GetComponentInParent<WorldModuleDesignHelper>();
        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();

        if (module && world)
        {
            Debug.LogError("此功能只能在模组编辑器中使用");
            return;
        }

        GameObject prefab = (GameObject) Resources.Load("Prefabs/Designs/Box_LevelEditor/" + ReplaceBoxTypeName.TypeName + "_LevelEditor");
        GameObject go = (GameObject) PrefabUtility.InstantiatePrefab(prefab, transform.parent);
        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.identity;
        DestroyImmediate(gameObject);
    }

    /// <summary>
    /// 专门作为工具使用，有时需要批量migrate数据
    /// </summary>
    /// <returns></returns>
    public bool RefreshData()
    {
        //bool isDirty = false;
        //foreach (EntityPassiveSkill bf in RawEntityExtraSerializeData.EntityPassiveSkills)
        //{
        //    if (bf is EntityPassiveSkill_Conditional condition)
        //    {
        //        foreach (EntityPassiveSkillAction action in condition.RawEntityPassiveSkillActions)
        //        {
        //            if (action is BoxPassiveSkillAction_ChangeBoxToEnemy cbt)
        //            {
        //                //cbt.RefreshABC();
        //                isDirty = true;
        //            }
        //        }
        //    }
        //}

        //return isDirty;
        return false;
    }

#endif
}