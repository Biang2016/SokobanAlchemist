using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BiangStudio.GamePlay.UI
{
    public class OnMouseClick : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent LeftClick;
        public UnityEvent LeftDoubleClick;
        public UnityEvent RightClick;
        public UnityEvent RightDoubleClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (eventData.clickCount == 1)
                {
                    LeftClick?.Invoke();
                }
                else if (eventData.clickCount == 2)
                {
                    LeftDoubleClick?.Invoke();
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (eventData.clickCount == 1)
                {
                    RightClick?.Invoke();
                }
                else if (eventData.clickCount == 2)
                {
                    RightDoubleClick?.Invoke();
                }
            }
        }

        public void ResetListeners()
        {
            LeftClick = new UnityEvent();
            LeftDoubleClick = new UnityEvent();
            RightClick = new UnityEvent();
            RightDoubleClick = new UnityEvent();
        }
    }
}