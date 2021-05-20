using System;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillAction_PlayerHUDToggle : EntitySkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "HUD面板元素隐藏和显示";

    [LabelText("对所有的组件生效")]
    public bool ForAllComponents;

    [LabelText("生效组件")]
    [HideIf("ForAllComponents")]
    public PlayerStatHUD.HUDComponent HUDComponent;

    [LabelText("True显示False隐藏")]
    public bool Shown;

    public void Execute()
    {
        PlayerStatHUD hud = ClientGameManager.Instance.PlayerStatHUDPanel.PlayerStatHUDs_Player[0];
        if (ForAllComponents)
        {
            hud.SetAllComponentShown(Shown);
        }
        else
        {
            hud.SetComponentShown(HUDComponent, Shown);
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_PlayerHUDToggle action = ((EntitySkillAction_PlayerHUDToggle) newAction);
        action.ForAllComponents = ForAllComponents;
        action.HUDComponent = HUDComponent;
        action.Shown = Shown;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_PlayerHUDToggle action = ((EntitySkillAction_PlayerHUDToggle) srcData);
        ForAllComponents = action.ForAllComponents;
        HUDComponent = action.HUDComponent;
        Shown = action.Shown;
    }
}