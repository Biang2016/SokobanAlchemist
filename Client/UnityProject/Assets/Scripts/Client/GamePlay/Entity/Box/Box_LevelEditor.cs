using System;
using System.Collections.Generic;
using System.Text;
using BiangLibrary;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

public class Box_LevelEditor : Entity
{
    public GameObject ModelRoot;

    #region 箱子Extra被动技能

    [NonSerialized]
    [ShowInInspector]
    [FoldoutGroup("箱子Extra被动技能")]
    [LabelText("箱子Extra被动技能")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<BoxPassiveSkill> RawBoxPassiveSkills = new List<BoxPassiveSkill>(); // 干数据，禁修改

    [HideInInspector]
    public byte[] BoxPassiveSkillBaseData;

    public override void OnBeforeSerialize()
    {
        base.OnBeforeSerialize();
        if (RawBoxPassiveSkills == null) RawBoxPassiveSkills = new List<BoxPassiveSkill>();
        BoxPassiveSkillBaseData = SerializationUtility.SerializeValue(RawBoxPassiveSkills, DataFormat.JSON);
    }

    public override void OnAfterDeserialize()
    {
        base.OnAfterDeserialize();
        RawBoxPassiveSkills = SerializationUtility.DeserializeValue<List<BoxPassiveSkill>>(BoxPassiveSkillBaseData, DataFormat.JSON);
    }

    #endregion

#if UNITY_EDITOR

    public bool RequireHideInWorldForModuleBox
    {
        get
        {
            foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
            {
                if (bf is BoxPassiveSkill_Hide hide)
                {
                    if (hide.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.World)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public bool RequireSerializeFunctionIntoWorld
    {
        get
        {
            foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
            {
                if (bf.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.World)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool RequireSerializeFunctionIntoWorldModule
    {
        get
        {
            foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
            {
                if (bf.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.Module)
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
            foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
            {
                if (bf is BoxPassiveSkill_LevelEventTriggerAppear appear)
                {
                    if (appear.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.Module)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public bool LevelEventTriggerAppearInWorld
    {
        get
        {
            foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
            {
                if (bf is BoxPassiveSkill_LevelEventTriggerAppear appear)
                {
                    if (appear.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.World)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public Box.BoxExtraSerializeData GetBoxExtraSerializeDataForWorldOverrideWorldModule()
    {
        Box.BoxExtraSerializeData data = new Box.BoxExtraSerializeData();
        data.BoxPassiveSkills = new List<BoxPassiveSkill>();
        foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
        {
            if (bf.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.World)
            {
                data.BoxPassiveSkills.Add(bf.Clone());
            }
        }

        return data;
    }

    public Box.BoxExtraSerializeData GetBoxExtraSerializeDataForWorldModule()
    {
        Box.BoxExtraSerializeData data = new Box.BoxExtraSerializeData();
        data.BoxPassiveSkills = new List<BoxPassiveSkill>();
        foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
        {
            if (bf is BoxPassiveSkill_LevelEventTriggerAppear) continue;
            if (bf.SpecialCaseType == BoxPassiveSkill.BoxPassiveSkillBaseSpecialCaseType.Module)
            {
                data.BoxPassiveSkills.Add(bf.Clone());
            }
        }

        return data;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (RequireSerializeFunctionIntoWorldModule)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f, "#0AFFF1".HTMLColorToColor(), Color.cyan, "模特");
            }

            if (RequireHideInWorldForModuleBox)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f, "#FF8000".HTMLColorToColor(), Color.yellow, "世隐");
            }
            else if (RequireSerializeFunctionIntoWorld || IsUnderWorldSpecialBoxesRoot)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f, "#FF8000".HTMLColorToColor(), Color.yellow, "世特");
            }

            if (LevelEventTriggerAppearInWorldModule)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f + Vector3.forward * 0.5f, Color.clear, "#B30AFF".HTMLColorToColor(), "模预隐");
            }
            else if (LevelEventTriggerAppearInWorld)
            {
                transform.DrawSpecialTip(Vector3.left * 0.5f + Vector3.forward * 0.5f, Color.clear, "#FF0A69".HTMLColorToColor(), "世预隐");
            }
        }
    }

    private bool IsUnderWorldSpecialBoxesRoot = false;

    void OnTransformParentChanged()
    {
        RefreshIsUnderWorldSpecialBoxesRoot();
    }

    internal void RefreshIsUnderWorldSpecialBoxesRoot()
    {
        if (!Application.isPlaying)
        {
            IsUnderWorldSpecialBoxesRoot = transform.HasAncestorName($"@_{WorldHierarchyRootType.WorldSpecialBoxesRoot}");
        }
    }
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
        if (!module)
        {
            Debug.LogError("此功能只能在模组编辑器中使用");
            return;
        }

        WorldDesignHelper world = GetComponentInParent<WorldDesignHelper>();
        if (world)
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

    public bool RenameBoxTypeName(string srcBoxName, string targetBoxName, StringBuilder info, bool moduleSpecial = false, bool worldSpecial = false)
    {
        bool isDirty = false;
        foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
        {
            bool dirty = bf.RenameBoxTypeName(name, srcBoxName, targetBoxName, info, moduleSpecial, worldSpecial);
            isDirty |= dirty;
        }

        return isDirty;
    }

    public bool DeleteBoxTypeName(string srcBoxName, StringBuilder info, bool moduleSpecial = false, bool worldSpecial = false)
    {
        bool isDirty = false;

        foreach (BoxPassiveSkill bf in RawBoxPassiveSkills)
        {
            bool dirty = bf.DeleteBoxTypeName(name, srcBoxName, info, moduleSpecial, worldSpecial);
            isDirty |= dirty;
        }

        return isDirty;
    }

#endif
}