using BiangStudio.GamePlay.UI;

public class PlayerHUDPanel : BaseUIPanel
{
    public PlayerHealthSlider[] HealthSliders_Player = new PlayerHealthSlider[2];

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
        for (int i = 0; i < HealthSliders_Player.Length; i++)
        {
            PlayerHealthSlider slider = HealthSliders_Player[i];
            PlayerActor player = BattleManager.Instance.MainPlayers[i];
            slider.gameObject.SetActive(player != null);
            if (player != null)
            {
                slider.Initialize(player.ActorBattleHelper);
            }
        }
    }
}