using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

public class BattleStateEditorWindow : OdinEditorWindow
{
    [MenuItem("开发工具/战场状态监视器")]
    public static void ShowBattleStateEditorWindow()
    {
        BattleStateEditorWindow window = new BattleStateEditorWindow();
        window.ShowUtility();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        name = "战场状态监视器";
        Init();
    }

    private void Init()
    {
    }

    [ShowInInspector]
    public Dictionary<string, BattleManager.BattleStateBool> BattleStateBoolDict => BattleManager.Instance != null ? BattleManager.Instance.BattleStateBoolDict : null;
}