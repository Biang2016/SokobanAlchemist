using BiangLibrary.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

public class EntitySkillRow : PoolObject
{
    [SerializeField]
    private Image SkillIcon;

    [SerializeField]
    private Text SkillName;

    [SerializeField]
    private Text SkillKeyBind;

    [SerializeField]
    private Text SkillDescription;

    [SerializeField]
    private Text SkillCost;

    public void Initialize(EntitySkill entitySkill, string keyBind, int goldCost)
    {
        Sprite sprite = ConfigManager.GetEntitySkillIconByName(entitySkill.SkillIcon.TypeName);
        SkillIcon.sprite = sprite;
        SkillDescription.text = entitySkill.GetSkillDescription_EN;
        SkillName.text = entitySkill.SkillName_EN;
        SkillKeyBind.gameObject.SetActive(!string.IsNullOrWhiteSpace(keyBind));
        SkillKeyBind.text = keyBind;
        SkillCost.gameObject.SetActive(goldCost > 0);
        if (goldCost > 0) SkillCost.text = $"Cost: {goldCost} Gold";
    }
}