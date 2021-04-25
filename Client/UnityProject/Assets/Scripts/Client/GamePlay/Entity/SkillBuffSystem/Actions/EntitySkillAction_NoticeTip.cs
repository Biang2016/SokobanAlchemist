using Sirenix.OdinInspector;

public class EntitySkillAction_NoticeTip : EntitySkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "半透明提示窗";

    [LabelText("显示/关闭")]
    public bool Show;

    [ShowIf("Show")]
    [LabelText("提示内容")]
    public string NoticeTip;

    [ShowIf("Show")]
    [LabelText("显示位置")]
    public NoticePanel.TipPositionType TipPositionType;

    [ShowIf("Show")]
    [LabelText("持续时长(负数为永久)")]
    public float Duration;

    public void Execute()
    {
        if (Show)
        {
            ClientGameManager.Instance.NoticePanel.ShowTip(NoticeTip, TipPositionType, Duration);
        }
        else
        {
            ClientGameManager.Instance.NoticePanel.HideTip();
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_NoticeTip action = ((EntitySkillAction_NoticeTip) newAction);
        action.Show = Show;
        action.NoticeTip = NoticeTip;
        action.TipPositionType = TipPositionType;
        action.Duration = Duration;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_NoticeTip action = ((EntitySkillAction_NoticeTip) srcData);
        Show = action.Show;
        NoticeTip = action.NoticeTip;
        TipPositionType = action.TipPositionType;
        Duration = action.Duration;
    }
}