using System.Collections.Generic;
using BiangLibrary.GamePlay.UI;
using UnityEngine;

public class EntitySkillPreviewPanel : BaseUIPanel
{
    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Fixed,
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
            if (!eps.ShowInSkillPreviewPanel) continue;
            EntitySkillRow esr = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntitySkillRow].AllocateGameObject<EntitySkillRow>(PassiveSkillContainer);
            PassiveSkillRows.Add(esr);
            esr.Initialize(eps, "", 0);
        }

        foreach (KeyValuePair<EntitySkillIndex, EntityActiveSkill> kv in entity.EntityActiveSkillDict)
        {
            if (!kv.Value.ShowInSkillPreviewPanel) continue;
            EntitySkillRow esr = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntitySkillRow].AllocateGameObject<EntitySkillRow>(ActiveSkillContainer);
            ActiveSkillRows.Add(esr);

            string keyBindStr = "";
            if (entity is Actor actor)
            {
                if (actor.ActorControllerHelper is PlayerControllerHelper pch)
                {
                    foreach (KeyValuePair<PlayerControllerHelper.KeyBind, List<EntitySkillIndex>> _kv in pch.SkillKeyMappings)
                    {
                        foreach (EntitySkillIndex entitySkillIndex in _kv.Value)
                        {
                            if (kv.Key == entitySkillIndex)
                            {
                                PlayerControllerHelper.KeyMappingDict.TryGetValue(_kv.Key, out ButtonNames keyBindButtonName);
                                keyBindStr = ControlManager.Instance.GetControlDescText(keyBindButtonName, false);
                                esr.transform.SetAsFirstSibling();
                            }
                        }
                    }
                }
            }

            esr.Initialize(kv.Value, keyBindStr, 0);
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

    public override void Display()
    {
        base.Display();
        ControlManager.Instance.BattleActionEnabled = false;
    }

    public override void Hide()
    {
        base.Hide();
        ControlManager.Instance.BattleActionEnabled = true;
    }
}