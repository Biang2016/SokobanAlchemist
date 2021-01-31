﻿using System;
using System.Collections.Generic;
using System.Text;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class Box_LevelEditor : MonoBehaviour
{
    public GameObject ModelRoot;

    #region 箱子Extra被动技能

    [FoldoutGroup("箱子Extra被动技能")]
    [LabelText("箱子Extra被动技能")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    [SerializeReference]
    public List<EntityPassiveSkill> RawBoxPassiveSkills = new List<EntityPassiveSkill>(); // 干数据，禁修改

    [LabelText("箱子朝向")]
    [OnValueChanged("RefreshOrientation")]
    [EnumToggleButtons]
    public GridPosR.Orientation BoxOrientation;

    private void RefreshOrientation()
    {
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, BoxOrientation), ModelRoot.transform, 1);
#if UNITY_EDITOR
        EditorWindow view = EditorWindow.GetWindow<SceneView>();
        view.Repaint();
#endif
    }

    #endregion

    #region BoxExtraData

    public class BoxExtraSerializeData : IClone<BoxExtraSerializeData>
    {
        public GridPos3D LocalGP; // Box在Module内的GP

        public List<EntityPassiveSkill> BoxPassiveSkills = new List<EntityPassiveSkill>();

        public BoxExtraSerializeData Clone()
        {
            return new BoxExtraSerializeData
            {
                LocalGP = LocalGP,
                BoxPassiveSkills = BoxPassiveSkills.Clone()
            };
        }
    }

    public BoxExtraSerializeData GetBoxExtraSerializeDataForWorldModule()
    {
        BoxExtraSerializeData data = new BoxExtraSerializeData();
        data.BoxPassiveSkills = new List<EntityPassiveSkill>();
        foreach (EntityPassiveSkill bf in RawBoxPassiveSkills)
        {
            if (bf is BoxPassiveSkill_LevelEventTriggerAppear) continue;
            if (bf.SpecialCaseType == EntityPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.Module)
            {
                data.BoxPassiveSkills.Add(bf.Clone());
            }
        }

        return data;
    }

    #endregion

#if UNITY_EDITOR

    public bool RequireSerializePassiveSkillsIntoWorldModule
    {
        get
        {
            foreach (EntityPassiveSkill bf in RawBoxPassiveSkills)
            {
                if (bf.SpecialCaseType == EntityPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.Module)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool LevelEventTriggerAppearInWorldModule
    {
        get
        {
            foreach (EntityPassiveSkill bf in RawBoxPassiveSkills)
            {
                if (bf is BoxPassiveSkill_LevelEventTriggerAppear appear)
                {
                    if (appear.SpecialCaseType == EntityPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.Module)
                    {
                        return true;
                    }
                }
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
                transform.DrawSpecialTip(Vector3.left * 0.5f, "#0AFFF1".HTMLColorToColor(), Color.cyan, "模特");
            }

            if (LevelEventTriggerAppearInWorldModule)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f + Vector3.forward * 0.5f, Color.clear, "#B30AFF".HTMLColorToColor(), "模预隐");
            }
        }
    }

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames => ConfigManager.GetAllBoxTypeNames();
    private IEnumerable<string> GetAllFXTypeNames => ConfigManager.GetAllFXTypeNames();

    #endregion

#endif

#if UNITY_EDITOR
    [HideInPlayMode]
    [HideInPrefabAssets]
    [ShowInInspector]
    [NonSerialized]
    [FoldoutGroup("快速替换")]
    [LabelText("替换Box类型")]
    [ValueDropdown("GetAllBoxTypeNames")]
    private string ReplaceBoxTypeName = "None";

    [HideInPlayMode]
    [HideInPrefabAssets]
    [FoldoutGroup("快速替换")]
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

        GameObject prefab = (GameObject) Resources.Load("Prefabs/Designs/Box_LevelEditor/" + ReplaceBoxTypeName + "_LevelEditor");
        GameObject go = (GameObject) PrefabUtility.InstantiatePrefab(prefab, transform.parent);
        go.transform.position = transform.position;
        go.transform.rotation = Quaternion.identity;
        DestroyImmediate(gameObject);
    }

    public bool RenameBoxTypeName(string srcBoxName, string targetBoxName, StringBuilder info, bool moduleSpecial = false)
    {
        bool isDirty = false;
        foreach (EntityPassiveSkill bf in RawBoxPassiveSkills)
        {
            bool dirty = bf.RenameBoxTypeName(name, srcBoxName, targetBoxName, info, moduleSpecial);
            isDirty |= dirty;
        }

        return isDirty;
    }

    public bool DeleteBoxTypeName(string srcBoxName, StringBuilder info, bool moduleSpecial = false)
    {
        bool isDirty = false;

        foreach (EntityPassiveSkill bf in RawBoxPassiveSkills)
        {
            bool dirty = bf.DeleteBoxTypeName(name, srcBoxName, info, moduleSpecial);
            isDirty |= dirty;
        }

        return isDirty;
    }

#endif
}