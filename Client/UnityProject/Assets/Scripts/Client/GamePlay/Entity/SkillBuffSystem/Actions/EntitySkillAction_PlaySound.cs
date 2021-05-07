using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillAction_PlaySound : EntitySkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "播放音效";

    [LabelText("音效")]
    public WwiseAudioManager.CommonAudioEvent CommonAudioEvent;

    public void Execute()
    {
        WwiseAudioManager.Instance.PlayCommonAudioSound(CommonAudioEvent, Entity.gameObject);
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_PlaySound action = ((EntitySkillAction_PlaySound) newAction);
        action.CommonAudioEvent = CommonAudioEvent;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_PlaySound action = ((EntitySkillAction_PlaySound) srcData);
        CommonAudioEvent = action.CommonAudioEvent;
    }
}