using BiangLibrary;
using BiangLibrary.ObjectPool;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BattleTipPrefabType
{
    None,

    UIBattleTip_PlayerGetDamaged,
    UIBattleTip_PlayerGetGreatDamaged,
    UIBattleTip_PlayerGetHealed,

    UIBattleTip_FriendGetDamaged,
    UIBattleTip_FriendGetGreatDamaged,
    UIBattleTip_FriendGetHealed,

    UIBattleTip_EnemyGetDamaged,
    UIBattleTip_EnemyGetGreatDamaged,
    UIBattleTip_EnemyGetHealed,

    UIBattleTip_AttributeTip,
    UIBattleTip_GainGoldTip,
}

public enum BattleTipType
{
    None = 0,
    Damage, //普通伤害-
    CriticalDamage, //暴击-
    Defense, // 伤害抵消
    Heal, //加血 
    Gold, //获得金子 
    Dodge, //躲闪
    Stun, //晕眩
    Shield, //护盾
    Hiding, //隐身
    SpeedUp, //加速
    SlowDown, //减速
    Invincible, //无敌
    Frozen, // 冰冻
    Poison, //中毒
    Firing, //燃烧
    SecKill, //秒杀----
    Bullet, //弹药 +/- xxx
    ExtraDamage, //额外伤害 +/- xxx
    PickWeapon,
    Parry, //格挡

    RaceDamage, //种族伤害, 加深伤害

    GetBuff, //获得Buff
    GetBestBuff, //获得强力BUFF
    UpLevel, //升级，大幅提升武器伤害

    InvincibleSpirit, //不屈意志！
    Overloading, //超载！
    AirAttack, //空袭！
    BoomShield, //防护盾！
    Thud, //震击
    Quickness, //急速
    WarmBlood, //热血
    NerveGas, //神经毒气
    Sober, //清醒
    Discipline, //惩戒
    EleInterference, //电磁干扰
    FastHatch, //快速填充
    LevelIncrease, //等级提升
    LeaderAppeared, //首领现身
    LeaderKilled, //首领已被击杀
    WeaponOutbreak, //武器爆发
    BeFound, //被发现, 【通用机制】被敌兵发现时增加飘字“被发现”
    MpDamage, //【通用机制】MP盾特殊飘字 MP伤害

    ParticalCannon, //粒子炮
    DevilBomb, //恶魔炸弹
    IonArc, //离子电弧
    AbsolutenessDomain, //绝对领域
    UW8AddGold, //【水下8关】【单局】【敌兵掉落】敌兵掉落表现
    PlaySoul, //播放枪魂
    ExpNum, // Exp + xx

    NoDamage, //1.10英雄天赋（跨版本）：固定值伤害减伤BUFF storyID:61774891 BUFF持续时间内，每次受到的伤害减少固定值例如1000（伤害最低减少到0），减少到0的时候免疫此次伤害并飘字“免疫”提示玩家

    SafeZone, // 吃鸡毒圈

    NoAttackSeparate = 200,
    AddScore, //得分
    SwitchWeapon, //切换武器时飘字
    GetOverlap, //获取叉乘
    BulletTimeAttackTip, //子弹时间暴击提示
    DesignTest,

    ScreenCenterSeparate, //不取挂点，直接获取屏幕中心
    BulletTimeTip, //子弹时间屏幕中心得示
    ScreenCenterTest2,
    ScreenCenterTest3,

    FollowDummySeparate, //取挂点，且一直随着挂点移动
    BulletTimeTutorialTip, //新手引导用的子弹时间
}

public class UIBattleTip : PoolObject
{
    private float disappearTick = 0;
    UIBattleTipInfo UIBattleTipInfo;

    private int sortingOrder;

    public int SortingOrder
    {
        get { return sortingOrder; }
        set
        {
            sortingOrder = value;
            TextType.sortingOrder = sortingOrder;
            TextContent.sortingOrder = sortingOrder;
            TextElementContent.sortingOrder = sortingOrder;
        }
    }

    [LabelText("图标")]
    public Image Icon;

    [LabelText("类型")]
    public TextMeshPro TextType;

    [LabelText("数值")]
    public TextMeshPro TextContent;

    [LabelText("元素数值")]
    public TextMeshPro TextElementContent;

    public Animator Animator;

    public AnimCurve3D AnimCurve3D;

    [LabelText("颜色变化")]
    public Gradient ColorDuringLife;

    protected Vector3 default_IconLocalPos = Vector3.zero;
    protected Vector3 default_TextTypeLocalPos = Vector3.zero;
    protected Vector3 default_TextContextLocalPos = Vector3.zero;
    protected Vector3 default_TextElementContextLocalPos = Vector3.zero;

    protected Vector3 offsetPos = new Vector3();

    public override void OnRecycled()
    {
        ResetTip();
        base.OnRecycled();
    }

    void Awake()
    {
        if (TextType) default_TextTypeLocalPos = TextType.transform.localPosition;
        if (TextContent) default_TextContextLocalPos = TextContent.transform.localPosition;
        if (TextElementContent) default_TextElementContextLocalPos = TextElementContent.transform.localPosition;
        if (Icon) default_IconLocalPos = Icon.transform.localPosition;
    }

    void Update()
    {
        if (!IsRecycled)
        {
            disappearTick += Time.deltaTime;
            if (disappearTick > UIBattleTipInfo.DisappearTime)
            {
                PoolRecycle();
                UIBattleTipManager.Instance.UIBattleTipList.Remove(this);
            }
            else
            {
                RefreshColor(disappearTick / UIBattleTipInfo.DisappearTime);
            }
        }
    }

    private void ResetTip()
    {
        Animator.speed = 1;
        UIBattleTipInfo = null;
        disappearTick = 0;
        if (TextType)
        {
            TextType.transform.localPosition = default_TextTypeLocalPos;
            TextType.color = new Color(0, 0, 0, 0);
        }

        if (TextContent)
        {
            TextContent.transform.localPosition = default_TextContextLocalPos;
            TextContent.color = new Color(0, 0, 0, 0);
        }

        if (TextElementContent)
        {
            TextElementContent.transform.localPosition = default_TextElementContextLocalPos;
            TextElementContent.color = new Color(0, 0, 0, 0);
        }

        if (Icon)
        {
            Icon.transform.localPosition = default_IconLocalPos;
            Icon.color = new Color(0, 0, 0, 0);
        }
    }

    public void Initialize(UIBattleTipInfo info, int sortingOrder)
    {
        SortingOrder = sortingOrder;
        UIBattleTipInfo = info;
        disappearTick = 0;

        transform.localScale = Vector3.one * info.Scale * CameraManager.Instance.FieldCamera.GetScaleForBattleUI();

        if (info.RandomRange.magnitude > 0)
        {
            offsetPos.x += Random.Range(-UIBattleTipInfo.RandomRange.x, UIBattleTipInfo.RandomRange.x);
            offsetPos.y += Random.Range(-UIBattleTipInfo.RandomRange.y, UIBattleTipInfo.RandomRange.y);
        }

        transform.localPosition = UIBattleTipInfo.StartPos;
        ClientUtils.InGameUIFaceToCamera(transform);

        bool needAddPlusSign = info.BattleTipType == BattleTipType.Heal || info.BattleTipType == BattleTipType.Gold;

        SetTextType(TextType);
        SetTextContext(TextContent, needAddPlusSign ? $"+{info.DiffHP}" : info.DiffHP.ToString());
        SetElementTextContext(TextElementContent, info.ElementHP == 0 ? "" : (needAddPlusSign ? $"+{info.ElementHP}" : info.ElementHP.ToString()));

        Animator.SetTrigger("Play");
        float duration_ori = CommonUtils.GetClipLength(Animator, "Float");
        Animator.speed = Animator.speed * duration_ori / info.DisappearTime;
    }

    private void SetTextType(TextMeshPro text)
    {
        text.text = "";
        text.color = ColorDuringLife.Evaluate(0);
        text.transform.localPosition = default_TextContextLocalPos + offsetPos;
    }

    private void SetTextContext(TextMeshPro text, string diffHP)
    {
        text.text = diffHP;
        text.color = ColorDuringLife.Evaluate(0);
        text.transform.localPosition = default_TextContextLocalPos + offsetPos;
    }

    private void SetElementTextContext(TextMeshPro text, string diffHP)
    {
        if (diffHP.IsNullOrWhitespace())
        {
            text.gameObject.SetActive(false);
        }
        else
        {
            text.gameObject.SetActive(true);
            text.text = diffHP;
        }

        text.color = ColorDuringLife.Evaluate(0);
        text.transform.localPosition = default_TextElementContextLocalPos + offsetPos;
    }

    private void RefreshColor(float timePortion)
    {
        if (TextType)
        {
            TextType.color = ColorDuringLife.Evaluate(timePortion);
        }

        if (TextContent)
        {
            TextContent.color = ColorDuringLife.Evaluate(timePortion);
        }

        if (TextElementContent)
        {
            TextElementContent.color = ColorDuringLife.Evaluate(timePortion);
        }
    }
}