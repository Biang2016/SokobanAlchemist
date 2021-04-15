using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using BiangLibrary;
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(menuName = "EntitySkillSSO")]
public class EntitySkillSSO : SerializedScriptableObject
{
    [BoxGroup("技能")]
    [HideLabel]
    [InlineProperty]
    public EntitySkill EntitySkill;

    private bool isActiveSkill => EntitySkill is EntityActiveSkill;

    private bool hasGUID => EntitySkill != null && !string.IsNullOrWhiteSpace(EntitySkill.SkillGUID);

    [Button("生成技能GUID")]
    [HideIf("hasGUID")]
    public void GenerateEntitySkillGUID()
    {
        if (EntitySkill != null && string.IsNullOrWhiteSpace(EntitySkill.SkillGUID))
        {
            EntitySkill.SkillGUID = Guid.NewGuid().ToString("P"); // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);
        }
    }

#if UNITY_EDITOR
    [Button("刷新Asset名称")]
    public void RefreshAssetName()
    {
        if (EntitySkill != null)
        {
            string path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.RenameAsset(path, $"{EntitySkill.SkillAlias}_{EntitySkill.SkillGUID}");
        }
    }
#endif
}