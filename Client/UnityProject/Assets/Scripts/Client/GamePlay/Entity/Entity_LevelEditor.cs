using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Entity_LevelEditor : MonoBehaviour
{
    public GameObject ModelRoot;

    public GameObject IndicatorHelperGO;

    [LabelText("旋转朝向")]
    [EnumToggleButtons]
    [OnValueChanged("RefreshOrientation")]
    public GridPosR.Orientation EntityOrientation;

    public EntityData EntityData = new EntityData();

    public bool RefreshOrientation()
    {
        bool dirty = EntityData.EntityOrientation != EntityOrientation;
        EntityData.EntityOrientation = EntityOrientation;
        dirty |= Math.Abs(ModelRoot.transform.rotation.eulerAngles.y - (int) EntityData.EntityOrientation * 90f) > 1f;
        if (dirty)
        {
            GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, EntityData.EntityOrientation), ModelRoot.transform, 1);
            GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, EntityData.EntityOrientation), IndicatorHelperGO.transform, 1);
#if UNITY_EDITOR
            EditorWindow view = EditorWindow.GetWindow<SceneView>();
            view.Repaint();
#endif
        }

        return dirty;
    }

#if UNITY_EDITOR

    public bool RequireSerializePassiveSkillsIntoWorldModule => EntityData.RawEntityExtraSerializeData.EntityPassiveSkills.Count > 0;

    public uint ProbablyShow
    {
        get
        {
            foreach (EntityPassiveSkill eps in EntityData.RawEntityExtraSerializeData.EntityPassiveSkills)
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
            foreach (EntityPassiveSkill bf in EntityData.RawEntityExtraSerializeData.EntityPassiveSkills)
            {
                if (bf is EntityPassiveSkill_LevelEventTriggerAppear appear) return true;
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
    [LabelText("@\"替换实体类型\t\"+ReplaceEntityTypeName")]
    [ValidateInput("ValidateReplaceEntityTypeName", "只能选择Box或者Enemy")]
    private TypeSelectHelper ReplaceEntityTypeName = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};

    private bool ValidateReplaceEntityTypeName(TypeSelectHelper value)
    {
        if (value.TypeDefineType == TypeDefineType.Box || value.TypeDefineType == TypeDefineType.Enemy) return true;
        return false;
    }

    [HideInPlayMode]
    [HideInPrefabAssets]
    [BoxGroup("快速替换")]
    [Button("替换实体", ButtonSizes.Large)]
    [GUIColor(0f, 1f, 1f)]
    private void ReplaceEntity_Editor()
    {
        WorldModuleDesignHelper module = GetComponentInParent<WorldModuleDesignHelper>();
        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();

        if (module && world)
        {
            Debug.LogError("此功能只能在模组编辑器中使用");
            return;
        }

        if (ReplaceEntityTypeName.TypeDefineType == TypeDefineType.Box)
        {
            GameObject prefab = (GameObject) Resources.Load("Prefabs/Designs/Box_LevelEditor/" + ReplaceEntityTypeName.TypeName + "_LevelEditor");
            GameObject go = (GameObject) PrefabUtility.InstantiatePrefab(prefab, transform.parent);
            go.transform.position = transform.position;
            go.transform.rotation = Quaternion.identity;
            DestroyImmediate(gameObject);
        }
        else if (ReplaceEntityTypeName.TypeDefineType == TypeDefineType.Enemy)
        {
            GameObject prefab = (GameObject) Resources.Load("Prefabs/Designs/Enemy_LevelEditor/" + ReplaceEntityTypeName.TypeName + "_LevelEditor");
            GameObject go = (GameObject) PrefabUtility.InstantiatePrefab(prefab, transform.parent);
            go.transform.position = transform.position;
            go.transform.rotation = Quaternion.identity;
            DestroyImmediate(gameObject);
        }
    }

#endif
}