using System.Collections;
using BiangLibrary.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

public class EntitySkillRow : PoolObject
{
    [SerializeField]
    private Image SkillIcon;

    [SerializeField]
    private Text SkillDescription;

    public void Initialize(EntitySkill entitySkill)
    {
        Sprite sprite = ConfigManager.GetEntitySkillIconByName(entitySkill.SkillIcon.TypeName);
        SkillIcon.sprite = sprite;
        SkillDescription.text = entitySkill.SkillDescription;
    }
}