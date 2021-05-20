using UnityEngine;

public class NoticeTip : MonoBehaviour
{
    public NoticePanel NoticePanel;

    public void SetStateShow()
    {
        NoticePanel.NoticeShown = true;
    }

    public void SetStateHide()
    {
        NoticePanel.NoticeShown = false;
    }
}