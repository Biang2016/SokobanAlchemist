using BiangStudio.GamePlay.UI;
using UnityEngine;

public class PlayerHUDPanel : BaseUIPanel
{
    public HealthSlider HealthSlider_Player1;
    public HealthSlider HealthSlider_Player2;

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

    public void Initialize()
    {
        HealthSlider_Player1.Initialize(BattleManager.Instance.MainPlayer1.ActorBattleHelper);
        HealthSlider_Player2.Initialize(BattleManager.Instance.MainPlayer2.ActorBattleHelper);
    }
}