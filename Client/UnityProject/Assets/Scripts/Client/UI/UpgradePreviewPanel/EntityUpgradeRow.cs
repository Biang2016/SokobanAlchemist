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

    public void Initialize(EntityUpgrade entityUpgrade)
    {
        Sprite sprite = ConfigManager.GetEntitySkillIconByName(entityUpgrade.UpgradeIcon.TypeName);
        UpgradeIcon.sprite = sprite;
        UpgradeName.text = entityUpgrade.UpgradeName_EN;
        UpgradeDescription.text = entityUpgrade.UpgradeDescription_EN;
    }
}