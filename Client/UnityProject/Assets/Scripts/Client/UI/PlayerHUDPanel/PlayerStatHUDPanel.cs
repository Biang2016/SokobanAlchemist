using BiangStudio.GamePlay.UI;

public class PlayerStatHUDPanel : BaseUIPanel
{
    public PlayerStatHUD[] PlayerStatHUDs_Player = new PlayerStatHUD[2];

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
        for (int i = 0; i < PlayerStatHUDs_Player.Length; i++)
        {
            PlayerStatHUD slider = PlayerStatHUDs_Player[i];
            PlayerActor player = BattleManager.Instance.MainPlayers[i];
            slider.gameObject.SetActive(player != null);
            if (player != null)
            {
                slider.Initialize(player.ActorBattleHelper);
            }
        }
    }
}