using BiangLibrary.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

public class TransportWorldRow : PoolObject
{
    [SerializeField]
    private Image WorldIcon;

    [SerializeField]
    private Text WorldName;

    [SerializeField]
    private Text WorldDescription;

    [SerializeField]
    private Text CostText;

    public void Initialize(WorldData worldData, int goldCost)
    {
        Sprite sprite = ConfigManager.GetEntitySkillIconByName(worldData.WorldIcon.TypeName);
        WorldIcon.sprite = sprite;
        WorldName.text = worldData.WorldName_EN;
        WorldDescription.text = worldData.WorldDescription_EN;
        CostText.gameObject.SetActive(goldCost > 0);
        if (goldCost > 0) CostText.text = $"Cost: {goldCost} Gold";
    }
}