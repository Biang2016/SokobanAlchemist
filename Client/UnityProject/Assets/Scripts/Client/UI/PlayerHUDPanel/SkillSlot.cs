using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour, ISkillBind
{
    [SerializeField]
    private PlayerControllerHelper.KeyBind MyKeyBind;

    [SerializeField]
    private Image SkillIcon;

    [SerializeField]
    private TextMeshProUGUI SkillKeyBind_Text;

    [SerializeField]
    private Animator Anim;

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
    public EntitySkill BoundEntitySkill => boundEntitySkill;
    private EntitySkill boundEntitySkill;

    public bool EmptySkill => curEntitySkillRow == null;

    void Awake()
    {
        SkillPhaseClockDict.Add(ActiveSkillPhase.WingingUp, SkillClock_WingingUp);
        SkillPhaseClockDict.Add(ActiveSkillPhase.Casting, SkillClock_Casting);
        SkillPhaseClockDict.Add(ActiveSkillPhase.CoolingDown, SkillClock_CoolingDown);
    }

    public void BindSkill(EntitySkill entitySkill)
    {
        if (entitySkill == null)
        {
            if (BoundEntitySkill != null && BoundEntitySkill is EntityActiveSkill eas)
            {
                eas.OnSkillWingingUp -= RefreshSkillCD;
                eas.OnSkillCasting -= RefreshSkillCD;
                eas.OnSkillCoolingDown -= RefreshSkillCD;
            }

            SkillKeyBind_Text.gameObject.SetActive(false);

            RefreshSkillCD(ActiveSkillPhase.Ready, 0, 0);
            boundEntitySkill = null;
            SkillIcon.color = Color.clear;
            SkillIcon.sprite = null;
            Anim.SetTrigger("SetEmpty");

            curEntitySkillRow?.PoolRecycle();
            curEntitySkillRow = null;
        }
        else
        {
            boundEntitySkill = entitySkill;
            if (BoundEntitySkill is EntityActiveSkill eas)
            {
                eas.OnSkillWingingUp += RefreshSkillCD;
                eas.OnSkillCasting += RefreshSkillCD;
                eas.OnSkillCoolingDown += RefreshSkillCD;
            }

            SkillKeyBind_Text.gameObject.SetActive(BoundEntitySkill is EntityActiveSkill);

            RefreshSkillCD(ActiveSkillPhase.Ready, 0, 0);
            SkillIcon.color = Color.white;
            Sprite sprite = ConfigManager.GetEntitySkillIconByName(entitySkill.SkillIcon.TypeName);
            SkillIcon.sprite = sprite;
            Anim.SetTrigger("SetSkill");

            curEntitySkillRow?.PoolRecycle();
            curEntitySkillRow = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntitySkillRow].AllocateGameObject<EntitySkillRow>(SkillRowContainer);
            curEntitySkillRow.Initialize(entitySkill, (BoundEntitySkill is EntityActiveSkill) ? MyKeyBind.ToString() : "", 0);
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

public interface ISkillBind
{
    void BindSkill(EntitySkill entitySkill);
    bool EmptySkill { get; }
    EntitySkill BoundEntitySkill { get; }
}