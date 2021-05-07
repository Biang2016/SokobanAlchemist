using System;
using BiangLibrary.GamePlay.UI;

[Serializable]
public class EntitySkillAction_UI : EntitySkillAction, EntitySkillAction.IPureAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description
    {
        get
        {
            if (Open)
            {
                return $"打开UI面板: {UIName}";
            }
            else
            {
                return $"关闭UI面板: {UIName}";
            }
        }
    }

    public string UIName = "";
    public bool Open = false;

    public void Execute()
    {
        BaseUIPanel uiPanel = UIManager.Instance.GetBaseUIForm(UIName);
        if (Open)
        {
            UIManager.Instance.ShowUIForm(UIName);
        }
        else
        {
            if (uiPanel != null)
            {
                uiPanel.CloseUIForm();
            }
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_UI action = ((EntitySkillAction_UI) newAction);
        action.UIName = UIName;
        action.Open = Open;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_UI action = ((EntitySkillAction_UI) srcData);
        UIName = action.UIName;
        Open = action.Open;
    }
}