using BiangLibrary.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

public class EntityUpgradeRow : PoolObject
{
    [SerializeField]
    private Image UpgradeIcon;

    [SerializeField]
    private Text UpgradeName;

    [SerializeField]
    private Text UpgradeDescription;

    [SerializeField]
    private Text GoldCost;

    public void Initialize(EntityUpgrade entityUpgrade, int goldCost)
    {
        Sprite sprite = ConfigManager.GetEntitySkillIconByName(entityUpgrade.UpgradeIcon.TypeName);
        UpgradeIcon.sprite = sprite;
        UpgradeName.text = entityUpgrade.UpgradeName_EN;
        UpgradeDescription.text = entityUpgrade.UpgradeDescription_EN;
        GoldCost.gameObject.SetActive(goldCost > 0);
        if (goldCost > 0) GoldCost.text = $"Cost: {goldCost} Gold";
    }
}