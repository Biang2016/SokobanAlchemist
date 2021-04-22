using System.Collections.Generic;
using UnityEngine;
using BiangLibrary.GamePlay.UI;
using UnityEngine.UI;

public class EntitySkillPreviewPanel : BaseUIPanel
{
    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Normal,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);
    }

    public Transform PassiveSkillContainer;

    private List<EntitySkillRow> PassiveSkillRows = new List<EntitySkillRow>();

    public Transform ActiveSkillContainer;

    private List<EntitySkillRow> ActiveSkillRows = new List<EntitySkillRow>();

    public void Initialize(Entity entity)
    {
        ClearSkills();

        foreach (EntityPassiveSkill eps in entity.EntityPassiveSkills)
        {
            if (string.IsNullOrWhiteSpace(eps.SkillDescription)) continue;
            if (eps.SkillIcon == null || string.IsNullOrWhiteSpace(eps.SkillIcon.TypeName)) continue;
            EntitySkillRow esr = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntitySkillRow].AllocateGameObject<EntitySkillRow>(PassiveSkillContainer);
            PassiveSkillRows.Add(esr);
            esr.Initialize(eps, "");
        }

        foreach (KeyValuePair<EntitySkillIndex, EntityActiveSkill> kv in entity.EntityActiveSkillDict)
        {
            if (string.IsNullOrWhiteSpace(kv.Value.SkillDescription)) continue;
            if (kv.Value.SkillIcon == null || string.IsNullOrWhiteSpace(kv.Value.SkillIcon.TypeName)) continue;

            EntitySkillRow esr = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntitySkillRow].AllocateGameObject<EntitySkillRow>(ActiveSkillContainer);
            ActiveSkillRows.Add(esr);

            string keyBind = "";
            if (entity is Actor actor)
            {
                if (actor.ActorControllerHelper is PlayerControllerHelper pch)
                {
                    for (int i = 0; i < pch.SkillKeyMappings.Count; i++)
                    {
                        List<EntitySkillIndex> entitySkillIndices = pch.SkillKeyMappings[i];
                        foreach (EntitySkillIndex entitySkillIndex in entitySkillIndices)
                        {
                            if (kv.Key == entitySkillIndex)
                            {
                                PlayerControllerHelper.KeyMappingStrDict.TryGetValue(i, out keyBind);
                                esr.transform.SetAsFirstSibling();
                            }
                        }
                    }
                }
            }

            esr.Initialize(kv.Value, keyBind);
        }
    }

    private void ClearSkills()
    {
        foreach (EntitySkillRow esr in PassiveSkillRows)
        {
            esr.PoolRecycle();
        }

        PassiveSkillRows.Clear();
        foreach (EntitySkillRow esr in ActiveSkillRows)
        {
            esr.PoolRecycle();
        }

        ActiveSkillRows.Clear();
    }
}