using System;
using System.Collections.Generic;
using System.Reflection;
using BiangLibrary;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class EntitySkillEditorWindow : EditorWindow
{
    [MenuItem("开发工具/配置/技能编辑器")]
    public static void ShowEntitySkillEditorWindow()
    {
        EntitySkillEditorWindow window = GetWindow<EntitySkillEditorWindow>(false, "技能编辑器");
        window.Show();
    }

    void OnEnable()
    {
        name = "技能编辑器";
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("");
        if (GUILayout.Button("所有Entity被动技能迁移"))
        {
            ConfigManager.LoadAllConfigs();
            //foreach (string actorName in ConfigManager.GetAllTypeNames(TypeDefineType.Actor, false))
            //{
            //    GameObject actorPrefab = ConfigManager.FindActorPrefabByName(actorName);
            //    Actor actor = actorPrefab.GetComponent<Actor>();
            //    foreach (EntitySkillSSO sso in actor.RawEntityPassiveSkillSSOs)
            //    {
            //        EntityPassiveSkill eps_clone = (EntityPassiveSkill)sso.EntitySkill.Clone();
            //        EntitySkillSO esSO = ScriptableObject.CreateInstance<EntitySkillSO>();
            //        esSO.EntitySkill = eps_clone;
            //        esSO.GenerateEntitySkillGUID();
            //        esSO.name = eps_clone.SkillAlias;
            //        AssetDatabase.CreateAsset(esSO, $"Assets/Designs/EntitySkill_Copy/EntityPassiveSkill/Actor/{esSO.name}_{esSO.EntitySkill.SkillGUID}.asset");
            //        AssetDatabase.SaveAssets();
            //        actor.RawEntityPassiveSkillSOs.Add(esSO);
            //    }

            //    EditorUtility.SetDirty(actor.gameObject);
            //}

            //foreach (string actorName in ConfigManager.GetAllTypeNames(TypeDefineType.Actor, false))
            //{
            //    GameObject actorPrefab = ConfigManager.FindActorPrefabByName(actorName);
            //    Actor actor = actorPrefab.GetComponent<Actor>();
            //    foreach (EntitySkillSSO sso in actor.RawEntityActiveSkillSSOs)
            //    {
            //        EntityActiveSkill eps_clone = (EntityActiveSkill)sso.EntitySkill.Clone();
            //        EntitySkillSO esSO = ScriptableObject.CreateInstance<EntitySkillSO>();
            //        esSO.EntitySkill = eps_clone;
            //        esSO.GenerateEntitySkillGUID();
            //        esSO.name = eps_clone.SkillAlias;
            //        AssetDatabase.CreateAsset(esSO, $"Assets/Designs/EntitySkill_Copy/EntityActiveSkill/Actor/{esSO.name}_{esSO.EntitySkill.SkillGUID}.asset");
            //        AssetDatabase.SaveAssets();
            //        actor.RawEntityActiveSkillSOs.Add(esSO);
            //    }

            //    EditorUtility.SetDirty(actor.gameObject);
            //}

            //foreach (string boxName in ConfigManager.GetAllTypeNames(TypeDefineType.Box, false))
            //{
            //    GameObject boxPrefab = ConfigManager.FindBoxPrefabByName(boxName);
            //    Box box = boxPrefab.GetComponent<Box>();
            //    foreach (EntitySkillSSO sso in box.RawEntityPassiveSkillSSOs)
            //    {
            //        EntityPassiveSkill eps_clone = (EntityPassiveSkill)sso.EntitySkill.Clone();
            //        EntitySkillSO esSO = ScriptableObject.CreateInstance<EntitySkillSO>();
            //        esSO.EntitySkill = eps_clone;
            //        esSO.GenerateEntitySkillGUID();
            //        esSO.name = eps_clone.SkillAlias;
            //        AssetDatabase.CreateAsset(esSO, $"Assets/Designs/EntitySkill_Copy/EntityPassiveSkill/Box/{esSO.name}_{esSO.EntitySkill.SkillGUID}.asset");
            //        AssetDatabase.SaveAssets();
            //        box.RawEntityPassiveSkillSOs.Add(esSO);
            //    }

            //    EditorUtility.SetDirty(box.gameObject);
            //}

            AssetDatabase.SaveAssets();
        }
    }
}