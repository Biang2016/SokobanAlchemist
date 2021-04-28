using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    [SerializeField]
    private PlayerControllerHelper.KeyBind MyKeyBind;

    [SerializeField]
    private Image SkillIcon;

    [SerializeField]
    private TextMeshProUGUI SkillKeyBind_Text;

    [SerializeField]
    private Animator Anim;

    private EntityActiveSkill boundEntityActiveSkill;

    [SerializeField]
    private Image SkillClock_WingingUp;

    [SerializeField]
    private Image SkillClock_Casting;

    [SerializeField]
    private Image SkillClock_CoolingDown;

    [SerializeField]
    private Image SkillCD_MaskImage;

    [SerializeField]
    private TextMeshProUGUI SkillCD_Text;

    private Dictionary<ActiveSkillPhase, Image> SkillPhaseClockDict = new Dictionary<ActiveSkillPhase, Image>();

    [SerializeField]
    private Transform SkillRowContainer;

    private EntitySkillRow curEntitySkillRow;

    void Awake()
    {
        SkillPhaseClockDict.Add(ActiveSkillPhase.WingingUp, SkillClock_WingingUp);
        SkillPhaseClockDict.Add(ActiveSkillPhase.Casting, SkillClock_Casting);
        SkillPhaseClockDict.Add(ActiveSkillPhase.CoolingDown, SkillClock_CoolingDown);
    }

    public void Initialize(EntityActiveSkill entityActiveSkill)
    {
        if (entityActiveSkill == null)
        {
            if (boundEntityActiveSkill != null)
            {
                boundEntityActiveSkill.OnSkillWingingUp -= RefreshSkillCD;
                boundEntityActiveSkill.OnSkillCasting -= RefreshSkillCD;
                boundEntityActiveSkill.OnSkillCoolingDown -= RefreshSkillCD;
            }

            RefreshSkillCD(ActiveSkillPhase.Ready, 0, 0);
            boundEntityActiveSkill = null;
            SkillIcon.color = Color.clear;
            SkillIcon.sprite = null;
            Anim.SetTrigger("SetEmpty");

            curEntitySkillRow?.PoolRecycle();
            curEntitySkillRow = null;
        }
        else
        {
            boundEntityActiveSkill = entityActiveSkill;
            boundEntityActiveSkill.OnSkillWingingUp += RefreshSkillCD;
            boundEntityActiveSkill.OnSkillCasting += RefreshSkillCD;
            boundEntityActiveSkill.OnSkillCoolingDown += RefreshSkillCD;
            RefreshSkillCD(ActiveSkillPhase.Ready, 0, 0);
            SkillIcon.color = Color.white;
            Sprite sprite = ConfigManager.GetEntitySkillIconByName(entityActiveSkill.SkillIcon.TypeName);
            SkillIcon.sprite = sprite;
            Anim.SetTrigger("SetSkill");

            curEntitySkillRow?.PoolRecycle();
            curEntitySkillRow = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntitySkillRow].AllocateGameObject<EntitySkillRow>(SkillRowContainer);
            curEntitySkillRow.Initialize(entityActiveSkill, MyKeyBind.ToString());
        }
    }

    public void RefreshSkillResourcesEnough()
    {
    }

    public void RefreshSkillCD(ActiveSkillPhase skillPhase, float currentTick, float coolDownTime)
    {
        bool anyCDRunning = false;
        foreach (KeyValuePair<ActiveSkillPhase, Image> kv in SkillPhaseClockDict)
        {
            kv.Value.enabled = kv.Key == skillPhase;
            if (kv.Key == skillPhase && currentTick < coolDownTime) anyCDRunning = true;
        }

        SkillKeyBind_Text.enabled = !anyCDRunning;
        SkillCD_MaskImage.enabled = anyCDRunning;

        int showCDSecond = 0;
        if (SkillPhaseClockDict.TryGetValue(skillPhase, out Image skillClock))
        {
            if (currentTick.Equals(coolDownTime))
            {
                skillClock.fillAmount = 1;
            }
            else
            {
                if (coolDownTime.Equals(0))
                {
                    skillClock.fillAmount = 1;
                    SkillCD_Text.text = "";
                }
                else
                {
                    skillClock.fillAmount = 1 - currentTick / coolDownTime;
                    showCDSecond = Mathf.CeilToInt(coolDownTime - currentTick);
                }
            }
        }

        SkillCD_Text.text = showCDSecond > 0 ? showCDSecond.ToString() : "";
    }
}