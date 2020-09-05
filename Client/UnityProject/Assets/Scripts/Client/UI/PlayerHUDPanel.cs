using BiangStudio.GamePlay.UI;

public class PlayerHUDPanel : BaseUIPanel
{
    public HealthSlider[] HealthSliders_Player = new HealthSlider[2];

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
            HealthSlider slider = HealthSliders_Player[i];
            PlayerActor player = BattleManager.Instance.MainPlayers[i];
            slider.gameObject.SetActive(player != null);
            if (player != null)
            {
                slider.Initialize(player.ActorBattleHelper);
            }
        }
    }
}