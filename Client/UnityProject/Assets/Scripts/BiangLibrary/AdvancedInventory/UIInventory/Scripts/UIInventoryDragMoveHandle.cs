using UnityEngine;
using UnityEngine.EventSystems;

public class UIInventoryDragMoveHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    internal bool EnableScreenClamp = true;

    [SerializeField]
    private RectTransform UIInventoryPanelRectTransform;

    private Vector2 lastMousePosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentMousePosition = eventData.position;
        Vector2 diff = currentMousePosition - lastMousePosition;
        Vector2 oldPos = UIInventoryPanelRectTransform.anchoredPosition;
        Vector2 newPosition_withX = oldPos + new Vector2(diff.x, 0);
        Vector2 newPosition_withY = oldPos + new Vector2(0, diff.y);
        Vector2 newPosition_withXY = oldPos + new Vector2(diff.x, diff.y);

        UIInventoryPanelRectTransform.anchoredPosition = newPosition_withXY;
        if (EnableScreenClamp)
        {
            if (!IsRectTransformInsideScreen(UIInventoryPanelRectTransform))
            {
                UIInventoryPanelRectTransform.anchoredPosition = newPosition_withX;
                if (!IsRectTransformInsideScreen(UIInventoryPanelRectTransform))
                {
                    UIInventoryPanelRectTransform.anchoredPosition = newPosition_withY;
                    if (!IsRectTransformInsideScreen(UIInventoryPanelRectTransform))
                    {
                        UIInventoryPanelRectTransform.anchoredPosition = oldPos;
                    }
                }
            }
        }

        lastMousePosition = currentMousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    Vector3[] cachedCorners = new Vector3[4];

    private bool IsRectTransformInsideScreen(RectTransform rectTransform)
    {
        rectTransform.GetLocalCorners(cachedCorners);
        int visibleCorners = 0;
        Rect rect = new Rect(-1, -1, Screen.width + 2, Screen.height + 2);
        foreach (Vector3 corner in cachedCorners)
        {
            Vector3 cornerScreenPos = (Vector2) corner + Vector2.Scale(rectTransform.pivot, new Vector2(Screen.width, Screen.height)) + rectTransform.anchoredPosition;
            if (rect.Contains(cornerScreenPos))
            {
                visibleCorners++;
            }
        }

        return visibleCorners == 4;
    }
}