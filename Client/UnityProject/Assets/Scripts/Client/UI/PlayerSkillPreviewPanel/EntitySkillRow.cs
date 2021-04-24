using System.Collections;
using BiangLibrary.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

public class EntitySkillRow : PoolObject
{
    [SerializeField]
    private Image SkillIcon;

    [SerializeField]
    private Text SkillKeyBind;

    [SerializeField]
    private Text SkillDescription;

    public void Initialize(EntitySkill entitySkill, string keyBind)
    {
        Sprite sprite = ConfigManager.GetEntitySkillIconByName(entitySkill.SkillIcon.TypeName);
        SkillIcon.sprite = sprite;
        SkillDescription.text = entitySkill.SkillDescription;
        SkillKeyBind.gameObject.SetActive(!string.IsNullOrWhiteSpace(keyBind));
        SkillKeyBind.text = keyBind;
    }
}