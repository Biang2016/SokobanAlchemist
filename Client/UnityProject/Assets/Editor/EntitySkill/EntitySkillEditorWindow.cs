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
            //ConfigManager.LoadAllConfigs();
            //foreach (string actorName in ConfigManager.GetAllTypeNames(TypeDefineType.Actor, false))
            //{
            //    GameObject actorPrefab = ConfigManager.FindActorPrefabByName(actorName);
            //    Actor actor = actorPrefab.GetComponent<Actor>();
            //    foreach (EntityPassiveSkill eps in actor.RawEntityPassiveSkills)
            //    {
            //        EntityPassiveSkill eps_clone = (EntityPassiveSkill) eps.Clone();
            //        EntitySkillSSO esSSO = SerializedScriptableObject.CreateInstance<EntitySkillSSO>();
            //        esSSO.EntitySkill = eps_clone;
            //        esSSO.GenerateEntitySkillGUID();
            //        esSSO.name = eps_clone.SkillAlias;
            //        AssetDatabase.CreateAsset(esSSO, $"Assets/Designs/EntitySkill/EntityPassiveSkill/Actor/{esSSO.name}_{esSSO.EntitySkill.SkillGUID}.asset");
            //        AssetDatabase.SaveAssets();
            //        actor.RawEntityPassiveSkillSSOs.Add(esSSO);
            //    }

            //    EditorUtility.SetDirty(actor.gameObject);
            //}

            //foreach (string boxName in ConfigManager.GetAllTypeNames(TypeDefineType.Box, false))
            //{
            //    GameObject boxPrefab = ConfigManager.FindBoxPrefabByName(boxName);
            //    Box box = boxPrefab.GetComponent<Box>();
            //    foreach (EntityPassiveSkill eps in box.RawEntityPassiveSkills)
            //    {
            //        EntityPassiveSkill eps_clone = (EntityPassiveSkill) eps.Clone();
            //        EntitySkillSSO esSSO = SerializedScriptableObject.CreateInstance<EntitySkillSSO>();
            //        esSSO.EntitySkill = eps_clone;
            //        esSSO.GenerateEntitySkillGUID();
            //        esSSO.name = eps_clone.SkillAlias;
            //        AssetDatabase.CreateAsset(esSSO, $"Assets/Designs/EntitySkill/EntityPassiveSkill/Box/{esSSO.name}_{esSSO.EntitySkill.SkillGUID}.asset");
            //        AssetDatabase.SaveAssets();
            //        box.RawEntityPassiveSkillSSOs.Add(esSSO);
            //    }

            //    EditorUtility.SetDirty(box.gameObject);
            //}

            //AssetDatabase.SaveAssets();
        }
    }
}