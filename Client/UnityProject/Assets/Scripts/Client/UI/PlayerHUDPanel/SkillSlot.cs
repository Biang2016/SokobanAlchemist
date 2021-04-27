using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    [SerializeField]
    private Button SkillButton;

    [SerializeField]
    private Image SkillIcon;

    [SerializeField]
    private Text SkillDescription;

    public void Initialize(EntitySkill entitySkill)
    {
        if (entitySkill == null)
        {
            SkillIcon.color = Color.clear;
            SkillIcon.sprite = null;
            SkillDescription.text = "";
        }
        else
        {
            SkillIcon.color = Color.white;
            Sprite sprite = ConfigManager.GetEntitySkillIconByName(entitySkill.SkillIcon.TypeName);
            SkillIcon.sprite = sprite;
            SkillDescription.text = entitySkill.SkillDescription;
        }
    }
}