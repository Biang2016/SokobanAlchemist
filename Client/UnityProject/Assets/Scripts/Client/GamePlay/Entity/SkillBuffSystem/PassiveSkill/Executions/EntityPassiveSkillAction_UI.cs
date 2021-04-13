using System;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay.UI;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityPassiveSkillAction_UI : EntityPassiveSkillAction, EntityPassiveSkillAction.IPureAction
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
            if (uiPanel == null)
            {
                UIManager.Instance.ShowUIForm(UIName);
            }
            else
            {
                uiPanel.Display();
            }
        }
        else
        {
            if (uiPanel != null)
            {
                uiPanel.Hide();
            }
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_UI action = ((EntityPassiveSkillAction_UI) newAction);
        action.UIName = UIName;
        action.Open = Open;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_UI action = ((EntityPassiveSkillAction_UI) srcData);
        UIName = action.UIName;
        Open = action.Open;
    }
}