﻿using System.Collections;
using BiangLibrary.GamePlay.UI;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuPanel : BaseUIPanel
{
    void Awake()
    {
        UIType.InitUIType(
            false,
            true,
            false,
            UIFormTypes.Normal,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);
    }

    public Animator StartMenuAnim;

    public Button SettingButton;
    public Button StartButton;
    public Button CreditButton;

    public override void Display()
    {
        base.Display();
        StartMenuAnim.SetTrigger("Play");
    }

    public void OnSettingButtonClick()
    {
    }

    public void OnStartButtonClick()
    {
        ClientGameManager.Instance.StartGame();
        CloseUIForm();
    }

    public void OnCreditButtonClick()
    {
    }
}