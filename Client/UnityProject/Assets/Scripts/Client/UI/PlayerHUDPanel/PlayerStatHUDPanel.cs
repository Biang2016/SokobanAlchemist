using BiangLibrary.GamePlay.UI;

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
            PlayerStatHUD hud = PlayerStatHUDs_Player[i];
            Actor player = BattleManager.Instance.MainPlayers[i];
            hud.gameObject.SetActive(player != null);
            if (player != null)
            {
                hud.Initialize(player.ActorBattleHelper);
            }
        }
    }
}