using System;
using System.Collections.Generic;
using BiangLibrary.GamePlay.UI;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntitySkillAction_ShowLearnSkillPanel : EntitySkillAction, EntitySkillAction.ITriggerAction
{
    public override void OnRecycled()
    {
        m_PageGUID = 0;
    }

    protected override string Description => "询问是否学习技能界面";

    [LabelText("技能")]
    [OnValueChanged("RefreshSkillGUID")]
    public EntitySkillSO EntitySkillSO;

    private bool isActiveSkill // 仅Editor下用
    {
        get
        {
            if (EntitySkillSO != null && EntitySkillSO.EntitySkill != null && EntitySkillSO.EntitySkill is EntityActiveSkill)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    [LabelText("指定键位")]
    [ShowIf("isActiveSkill")]
    public PlayerControllerHelper.KeyBind KeyBind;

    public void RefreshSkillGUID()
    {
        if (EntitySkillSO != null)
        {
            SkillGUID = EntitySkillSO.EntitySkill.SkillGUID;
        }
        else
        {
            SkillGUID = "";
        }
    }

    [ReadOnly]
    [LabelText("技能GUID")]
    public string SkillGUID = "";

    [LabelText("价格")]
    public int GoldCost = 0;

    [LabelText("生效于相对阵营")]
    public RelativeCamp EffectiveOnRelativeCamp;

    [LabelText("生效于特定种类Entity")]
    public bool EffectiveOnSpecificEntity;

    [LabelText("Entity种类")]
    [ShowIf("EffectiveOnSpecificEntity")]
    public TypeSelectHelper EffectiveOnSpecificEntityType = new TypeSelectHelper {TypeDefineType = TypeDefineType.Box};

    public HashSet<uint> EntityStayHashSet = new HashSet<uint>();

    private uint m_PageGUID;

    public void ExecuteOnTriggerEnter(Collider collider)
    {
        if (UIManager.Instance.IsUIShown<ExitMenuPanel>()) return;
        if (UIManager.Instance.IsUIShown<TransportWorldPanel>()) return;
        if (LayerManager.Instance.CheckLayerValid(Entity.Camp, EffectiveOnRelativeCamp, collider.gameObject.layer))
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target.IsNotNullAndAlive())
            {
                if (EffectiveOnSpecificEntity)
                {
                    if (target.EntityTypeIndex != ConfigManager.GetTypeIndex(EffectiveOnSpecificEntityType.TypeDefineType, EffectiveOnSpecificEntityType.TypeName))
                    {
                        return;
                    }
                }

                if (!EntityStayHashSet.Contains(target.GUID))
                {
                    EntityStayHashSet.Add(target.GUID);
                }

                m_PageGUID = ClientGameManager.Instance.LearnSkillUpgradePanel.AddLearnInfo(new LearnInfo
                {
                    LearnType = LearnType.Skill,
                    SkillGUID = SkillGUID,
                    LearnCallback = OnLearned,
                    KeyBind = KeyBind,
                    GoldCost = GoldCost
                });
            }
        }
    }

    public void ExecuteOnTriggerStay(Collider collider)
    {
    }

    public void ExecuteOnTriggerExit(Collider collider)
    {
        if (LayerManager.Instance.CheckLayerValid(Entity.Camp, EffectiveOnRelativeCamp, collider.gameObject.layer))
        {
            Entity target = collider.GetComponentInParent<Entity>();
            if (target.IsNotNullAndAlive())
            {
                if (EffectiveOnSpecificEntity)
                {
                    if (target.EntityTypeIndex != ConfigManager.GetTypeIndex(EffectiveOnSpecificEntityType.TypeDefineType, EffectiveOnSpecificEntityType.TypeName))
                    {
                        return;
                    }
                }

                if (EntityStayHashSet.Contains(target.GUID))
                {
                    EntityStayHashSet.Remove(target.GUID);
                    ClientGameManager.Instance.LearnSkillUpgradePanel.RemovePage(m_PageGUID);
                    m_PageGUID = 0;
                }
            }
        }
    }

    private void OnLearned()
    {
        BattleManager.Instance.Player1.EntityStatPropSet.Gold.SetValue(BattleManager.Instance.Player1.EntityStatPropSet.Gold.Value - GoldCost);
        Entity.DestroySelf();
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_ShowLearnSkillPanel action = ((EntitySkillAction_ShowLearnSkillPanel) newAction);
        action.SkillGUID = SkillGUID;
        action.KeyBind = KeyBind;
        action.GoldCost = GoldCost;
        action.EffectiveOnRelativeCamp = EffectiveOnRelativeCamp;
        action.EffectiveOnSpecificEntity = EffectiveOnSpecificEntity;
        action.EffectiveOnSpecificEntityType = EffectiveOnSpecificEntityType.Clone();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_ShowLearnSkillPanel action = ((EntitySkillAction_ShowLearnSkillPanel) srcData);
        SkillGUID = action.SkillGUID;
        KeyBind = action.KeyBind;
        GoldCost = action.GoldCost;
        EffectiveOnRelativeCamp = action.EffectiveOnRelativeCamp;
        EffectiveOnSpecificEntity = action.EffectiveOnSpecificEntity;
        EffectiveOnSpecificEntityType = action.EffectiveOnSpecificEntityType.Clone();
    }
}