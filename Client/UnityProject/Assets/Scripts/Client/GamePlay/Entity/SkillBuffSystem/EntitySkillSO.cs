using System;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(menuName = "EntitySkillSO")]
public class EntitySkillSO : ScriptableObject
{
#if UNITY_EDITOR
    [ButtonGroup("Buttons")]
    [Button("刷新Asset名称", ButtonSizes.Large)]
    public void RefreshAssetName()
    {
        if (EntitySkill != null)
        {
            string path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.RenameAsset(path, $"{EntitySkill.SkillAlias}_{EntitySkill.SkillGUID}");
        }
    }

    [ButtonGroup("Buttons")]
    [Button("强制保存")]
    public void ForceSave()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif

    [BoxGroup("技能")]
    [HideLabel]
    [InlineProperty]
    [SerializeReference]
    public EntitySkill EntitySkill;

    private bool isActiveSkill => EntitySkill is EntityActiveSkill;

    private bool hasGUID => EntitySkill != null && !string.IsNullOrWhiteSpace(EntitySkill.SkillGUID);

    [ButtonGroup("Buttons")]
    [Button("生成技能GUID")]
    [HideIf("hasGUID")]
    public void GenerateEntitySkillGUID()
    {
        if (EntitySkill != null && string.IsNullOrWhiteSpace(EntitySkill.SkillGUID))
        {
            EntitySkill.SkillGUID = Guid.NewGuid().ToString("P"); // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);
        }
    }

    [ButtonGroup("Buttons")]
    [Button("强制刷新技能GUID")]
    [ShowIf("hasGUID")]
    public void ForceRefreshEntitySkillGUID()
    {
        if (EntitySkill != null && !string.IsNullOrWhiteSpace(EntitySkill.SkillGUID))
        {
            EntitySkill.SkillGUID = Guid.NewGuid().ToString("P"); // e.g: (ade24d16-db0f-40af-8794-1e08e2040df3);
        }
    }
}