using System.Collections;
using BiangLibrary.GamePlay.UI;
using TMPro;
using UnityEngine;

public class NoticePanel : BaseUIPanel
{
    [SerializeField]
    private RectTransform TipTextContainer;

    [SerializeField]
    private TextMeshProUGUI TipText;

    [SerializeField]
    private Animator TipAnim;

    internal bool NoticeShown = false;

    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Fixed,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);
    }

    private Coroutine hideTipCoroutine;

    /// <summary>
    /// 显示提示
    /// </summary>
    /// <param name="tipContent"></param>
    /// <param name="duration">负值为永久</param>
    public void ShowTip(string tipContent, TipPositionType tipPositionType, float duration)
    {
        if (TipText.text.Equals(tipContent))
        {
            tipShowDuration = Mathf.Max(tipShowDuration, duration);
            return;
        }

        TipText.text = tipContent;
        TipAnim.SetTrigger("Show");
        if (hideTipCoroutine != null) StopCoroutine(hideTipCoroutine);
        tipShowDuration = duration;
        hideTipCoroutine = StartCoroutine(Co_HideTip());
        switch (tipPositionType)
        {
            case TipPositionType.Center:
            {
                TipTextContainer.anchorMin = new Vector2(0.5f, 0.5f);
                TipTextContainer.anchorMax = new Vector2(0.5f, 0.5f);
                TipTextContainer.pivot = new Vector2(0.5f, 0.5f);
                break;
            }
            case TipPositionType.LeftCenterHigher:
            {
                TipTextContainer.anchorMin = new Vector2(0f, 0.7f);
                TipTextContainer.anchorMax = new Vector2(0f, 0.7f);
                TipTextContainer.pivot = new Vector2(0f, 0.5f);
                break;
            }
            case TipPositionType.LeftCenter:
            {
                TipTextContainer.anchorMin = new Vector2(0f, 0.5f);
                TipTextContainer.anchorMax = new Vector2(0f, 0.5f);
                TipTextContainer.pivot = new Vector2(0f, 0.5f);
                break;
            }
            case TipPositionType.LeftCenterLower:
            {
                TipTextContainer.anchorMin = new Vector2(0f, 0.3f);
                TipTextContainer.anchorMax = new Vector2(0f, 0.3f);
                TipTextContainer.pivot = new Vector2(0f, 0.5f);
                break;
            }
            case TipPositionType.RightCenterHigher:
            {
                TipTextContainer.anchorMin = new Vector2(1f, 0.7f);
                TipTextContainer.anchorMax = new Vector2(1f, 0.7f);
                TipTextContainer.pivot = new Vector2(1f, 0.5f);
                break;
            }
            case TipPositionType.RightCenter:
            {
                TipTextContainer.anchorMin = new Vector2(1f, 0.5f);
                TipTextContainer.anchorMax = new Vector2(1f, 0.5f);
                TipTextContainer.pivot = new Vector2(1f, 0.5f);
                break;
            }
            case TipPositionType.RightCenterLower:
            {
                TipTextContainer.anchorMin = new Vector2(1f, 0.3f);
                TipTextContainer.anchorMax = new Vector2(1f, 0.3f);
                TipTextContainer.pivot = new Vector2(1f, 0.5f);
                break;
            }
            case TipPositionType.TopCenter:
            {
                TipTextContainer.anchorMin = new Vector2(0.5f, 1f);
                TipTextContainer.anchorMax = new Vector2(0.5f, 1f);
                TipTextContainer.pivot = new Vector2(0.5f, 1f);
                break;
            }
            case TipPositionType.BottomCenter:
            {
                TipTextContainer.anchorMin = new Vector2(0.5f, 0f);
                TipTextContainer.anchorMax = new Vector2(0.5f, 0f);
                TipTextContainer.pivot = new Vector2(0.5f, 0f);
                break;
            }
        }
    }

    private float tipShowDuration = 0;

    IEnumerator Co_HideTip()
    {
        while (tipShowDuration > 0)
        {
            yield return null;
            tipShowDuration -= Time.deltaTime;
        }

        HideTip();
    }

    public void HideTip()
    {
        if (!NoticeShown) return;
        if (hideTipCoroutine != null) StopCoroutine(hideTipCoroutine);
        TipAnim.SetTrigger("Hide");
        TipText.text = "";
    }

    public enum TipPositionType
    {
        Center,
        LeftCenterHigher,
        LeftCenter,
        LeftCenterLower,
        RightCenterHigher,
        RightCenter,
        RightCenterLower,
        TopCenter,
        BottomCenter,
    }
}