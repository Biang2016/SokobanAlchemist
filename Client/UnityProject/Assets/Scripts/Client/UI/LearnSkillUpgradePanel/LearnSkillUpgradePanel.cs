using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.GamePlay.UI;
using UnityEngine;
using UnityEngine.Events;

public class LearnSkillUpgradePanel : BaseUIPanel
{
    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Normal,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);
    }

    [SerializeField]
    private RectTransform PageContainer;

    [SerializeField]
    private RectTransform AddingPageContainer;

    public bool HasPage => PageList.Count + AddingPageQueue.Count > 0;
    public int PageCount => PageList.Count;

    private int currentFocusPageIndex = 0;

    private int CurrentFocusPageIndex
    {
        get
        {
            if (currentFocusPageIndex < 0) currentFocusPageIndex = 0;
            if (currentFocusPageIndex >= PageList.Count) currentFocusPageIndex = PageList.Count - 1;
            return currentFocusPageIndex;
        }
        set
        {
            if (value < 0) value = PageList.Count - 1;
            if (value >= PageList.Count) value = 0;
            if (currentFocusPageIndex != value)
            {
                if (PageList.Count == 0)
                {
                    currentFocusPageIndex = 0;
                }
                else
                {
                    currentFocusPageIndex = value % PageList.Count;
                }

                refreshDurationTick = 0f;
                RefreshPages();
            }
        }
    }

    internal Dictionary<uint, LearnSkillUpgradePage> PageDict = new Dictionary<uint, LearnSkillUpgradePage>();

    private List<uint> PageList = new List<uint>();
    private List<uint> AddingPageQueue = new List<uint>();
    private List<uint> RemovingPageQueue = new List<uint>();

    public uint AddLearnInfo(LearnInfo learnInfo)
    {
        LearnSkillUpgradePage page = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.LearnSkillUpgradePage].AllocateGameObject<LearnSkillUpgradePage>(AddingPageContainer);
        page.Initialize(learnInfo);
        AddingPageQueue.Add(page.GUID);
        refreshDurationTick = 0f;
        RefreshPages();
        return page.GUID;
    }

    public void RemovePage(uint pageGUID)
    {
        if (PageDict.TryGetValue(pageGUID, out LearnSkillUpgradePage page))
        {
            if (AddingPageQueue.Contains(pageGUID))
            {
                AddingPageQueue.Remove(pageGUID);
                page.PoolRecycle();
                return;
            }

            RemovingPageQueue.Add(pageGUID);
            refreshDurationTick = 0f;
            RefreshPages();
        }
    }

    #region Refresh

    private Coroutine refreshCoroutine;

    public void RefreshPages()
    {
        if (isRefreshing) return;
        refreshCoroutine = StartCoroutine(Co_RefreshPages());
    }

    private bool isRefreshing = false;
    private float refreshDurationTick = 0f;

    public IEnumerator Co_RefreshPages()
    {
        isRefreshing = true;
        float addPageIntervalTick = 0f;
        float removePageIntervalTick = 0f;
        while (refreshDurationTick < 1f)
        {
            refreshDurationTick += Time.deltaTime;

            addPageIntervalTick += Time.deltaTime;
            if (addPageIntervalTick > 0.2f)
            {
                addPageIntervalTick = 0f;
                if (AddingPageQueue.Count > 0)
                {
                    if (PageDict.TryGetValue(AddingPageQueue[0], out LearnSkillUpgradePage addPage))
                    {
                        AddingPageQueue.RemoveAt(0);
                        PageList.Add(addPage.GUID);
                        addPage.transform.SetParent(PageContainer);
                        addPage.transform.SetAsFirstSibling();
                        addPage.Anim.SetTrigger("Jump");
                    }
                    else
                    {
                        AddingPageQueue.RemoveAt(0);
                    }
                }
            }

            removePageIntervalTick += Time.deltaTime;
            if (removePageIntervalTick > 0.2f)
            {
                removePageIntervalTick = 0f;
                if (RemovingPageQueue.Count > 0)
                {
                    if (PageDict.TryGetValue(RemovingPageQueue[0], out LearnSkillUpgradePage removePage))
                    {
                        RemovingPageQueue.RemoveAt(0);
                        if (PageList.Contains(removePage.GUID))
                        {
                            PageList.Remove(removePage.GUID);
                            StartCoroutine(removePage.Co_Remove());
                        }
                    }
                    else
                    {
                        RemovingPageQueue.RemoveAt(0);
                    }
                }
            }

            List<uint> removeList = new List<uint>();
            foreach (uint guid in PageList)
            {
                if (!PageDict.ContainsKey(guid))
                {
                    removeList.Add(guid);
                }
            }

            foreach (uint guid in removeList)
            {
                PageList.Remove(guid);
            }

            for (int index = 0; index < PageList.Count; index++)
            {
                LearnSkillUpgradePage page = PageDict[PageList[index]];
                page.IsSelected = index == CurrentFocusPageIndex;
                page.IsFirst = index == 0;
                float distanceRatio = (float) Mathf.Abs(index - CurrentFocusPageIndex) / PageList.Count;
                float scale = distanceRatio.Remap(0, 1, 0.7f, 0.3f);
                if (index == CurrentFocusPageIndex) scale = 1f;
                StartCoroutine(page.Co_SetScale(scale, 0.2f));
            }

            yield return CommonUtils.UpdateLayout(PageContainer);
        }

        refreshCoroutine = null;
        isRefreshing = false;
    }

    #endregion

    void FixedUpdate()
    {
        if (IsShown)
        {
            if (ControlManager.Instance.Common_Exit.Up)
            {
                if (PageList.Count > 0)
                {
                    RemovePage(PageList[CurrentFocusPageIndex]);
                }
            }

            if (ControlManager.Instance.Battle_LeftSwitch.Up)
            {
                CurrentFocusPageIndex--;
            }

            if (ControlManager.Instance.Battle_RightSwitch.Up)
            {
                CurrentFocusPageIndex++;
            }

            if (ControlManager.Instance.Common_InteractiveKey.Up)
            {
            }
        }
    }

    public void HidePanel()
    {
        foreach (uint guid in AddingPageQueue)
        {
            RemovePage(guid);
        }

        foreach (uint guid in PageList)
        {
            RemovePage(guid);
        }
    }
}

public class LearnInfo
{
    public LearnType LearnType;
    public string SkillGUID;
    public EntityUpgrade EntityUpgrade;
    public UnityAction LearnCallback;
    public PlayerControllerHelper.KeyBind KeyBind;
    public int GoldCost;
}

public enum LearnType
{
    Skill,
    Upgrade
}