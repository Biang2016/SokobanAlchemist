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
        if (GUILayout.Button("所有Actor主动技能迁移"))
        {
            //    ConfigManager.LoadAllConfigs();
            //    foreach (string actorName in ConfigManager.GetAllTypeNames(TypeDefineType.Actor, false))
            //    {
            //        GameObject actorPrefab = ConfigManager.FindActorPrefabByName(actorName);
            //        Actor actor = actorPrefab.GetComponent<Actor>();
            //        foreach (EntityActiveSkill eas in actor.RawEntityActiveSkills)
            //        {
            //            EntityActiveSkill eas_clone = (EntityActiveSkill) eas.Clone();
            //            EntitySkillSSO esSSO = SerializedScriptableObject.CreateInstance<EntitySkillSSO>();
            //            esSSO.EntitySkill = eas_clone;

            //            Queue<EntityActiveSkill> subActiveSkillQueue = new Queue<EntityActiveSkill>();
            //            subActiveSkillQueue.Enqueue(eas_clone);
            //            while (subActiveSkillQueue.Count>0)
            //            {
            //                EntityActiveSkill curEAS = subActiveSkillQueue.Dequeue();
            //                EntityStatPropSet.SkillPropertyCollection skillProperty_clone = new EntityStatPropSet.SkillPropertyCollection();
            //                actor.RawEntityStatPropSet.SkillsPropertyCollections[(int)curEAS.EntitySkillIndex].ApplyDataTo(skillProperty_clone);
            //                esSSO.EntityActiveSkillPropertyCollections.Add(skillProperty_clone);
            //                skillProperty_clone.skillAlias = curEAS.SkillAlias;
            //                foreach (EntityActiveSkill subEAS in curEAS.RawSubActiveSkillList)
            //                {
            //                    subActiveSkillQueue.Enqueue(subEAS);
            //                }
            //            }

            //            esSSO.GenerateEntitySkillGUID();
            //            esSSO.name = eas_clone.SkillAlias;
            //            AssetDatabase.CreateAsset(esSSO, $"Assets/Designs/EntitySkill/EntityActiveSkill/{esSSO.name}_{esSSO.EntitySkill.SkillGUID}.asset");
            //            AssetDatabase.SaveAssets();
            //        }
            //    }
        }
    }
}