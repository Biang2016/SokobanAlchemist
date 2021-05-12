using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementBottle : MonoBehaviour, ISkillBind
{
    [SerializeField]
    private PlayerControllerHelper.KeyBind MyKeyBind;

    [SerializeField]
    private TextMeshProUGUI SkillKeyBind_Text;

    [SerializeField]
    private TextMeshProUGUI CurrentValue_Text;

    [SerializeField]
    private TextMeshProUGUI MaxValue_Text;

    [SerializeField]
    private Image Image;

    [SerializeField]
    private string ElementDescHeader;

    [SerializeField]
    private Text ElementDesc;

    [SerializeField]
    private Sprite[] Sprites;

    [SerializeField]
    private Animator FillAnim;

    [SerializeField]
    private Transform SkillRowContainer;

    private EntitySkillRow curEntitySkillRow;
    public EntitySkill BoundEntitySkill => boundEntitySkill;
    private EntitySkill boundEntitySkill;

    public bool EmptySkill => curEntitySkillRow == null;

    void Start()
    {
        RefreshValue(0, 0, 0);
        ControlManager.Instance.OnControlSchemeChanged += (before, after) =>
        {
            if (curEntitySkillRow != null && boundEntitySkill != null)
            {
                PlayerControllerHelper.KeyMappingDict.TryGetValue(MyKeyBind, out ButtonNames keyBindButtonName);
                string keyBindStr = ControlManager.Instance.GetControlDescText(keyBindButtonName, false);
                curEntitySkillRow.Initialize(BoundEntitySkill, (BoundEntitySkill is EntityActiveSkill) ? keyBindStr : "", 0);
            }
        };
    }

    public void Initialize()
    {
        SkillKeyBind_Text.gameObject.SetActive(false);
    }

    public void BindSkill(EntitySkill entitySkill)
    {
        if (entitySkill == null)
        {
            SkillKeyBind_Text.gameObject.SetActive(false);
            boundEntitySkill = null;
            curEntitySkillRow?.PoolRecycle();
            curEntitySkillRow = null;
        }
        else
        {
            boundEntitySkill = entitySkill;
            SkillKeyBind_Text.gameObject.SetActive(BoundEntitySkill is EntityActiveSkill);

            curEntitySkillRow?.PoolRecycle();
            curEntitySkillRow = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.EntitySkillRow].AllocateGameObject<EntitySkillRow>(SkillRowContainer);
            PlayerControllerHelper.KeyMappingDict.TryGetValue(MyKeyBind, out ButtonNames keyBindButtonName);
            string keyBindStr = ControlManager.Instance.GetControlDescText(keyBindButtonName, false);
            curEntitySkillRow.Initialize(entitySkill, (BoundEntitySkill is EntityActiveSkill) ? keyBindStr : "", 0);
        }
    }

    public void RefreshValue(int currentValue, int minValue, int maxValue)
    {
        ElementDesc.text = $"{ElementDescHeader}: {currentValue} / {maxValue}";
        gameObject.SetActive(maxValue > 0);

        int ratio = 0;
        if (currentValue != 0 && maxValue != 0)
        {
            ratio = Mathf.CeilToInt((float) currentValue / maxValue * 10f);
        }

        Image.sprite = Sprites[ratio];
        CurrentValue_Text.text = currentValue + "/";
        if (MaxValue_Text)
        {
            CurrentValue_Text.text = currentValue + "/";
            MaxValue_Text.text = maxValue.ToString();
        }
        else
        {
            CurrentValue_Text.text = currentValue.ToString();
        }

        if (FillAnim) FillAnim.SetTrigger("ValueChange");
    }

    public void OnStatLowWarning()
    {
        if (FillAnim) FillAnim.SetTrigger("LowWarning");
    }
}