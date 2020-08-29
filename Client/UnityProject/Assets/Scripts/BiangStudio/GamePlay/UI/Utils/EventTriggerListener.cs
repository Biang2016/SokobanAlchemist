using UnityEngine;
using UnityEngine.EventSystems;

namespace BiangStudio.GamePlay.UI
{
    public class EventTriggerListener : EventTrigger
    {
        public delegate void VoidDelegate(GameObject go);

        public VoidDelegate onClick;
        public VoidDelegate onDown;
        public VoidDelegate onEnter;
        public VoidDelegate onExit;
        public VoidDelegate onUp;
        public VoidDelegate onSelect;
        public VoidDelegate onUpdateSelect;

        public static EventTriggerListener Get(GameObject go)
        {
            EventTriggerListener lister = go.GetComponent<EventTriggerListener>();
            if (lister == null)
            {
                lister = go.AddComponent<EventTriggerListener>();
            }

            return lister;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(gameObject);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            onDown?.Invoke(gameObject);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            onEnter?.Invoke(gameObject);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            onExit?.Invoke(gameObject);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            onUp?.Invoke(gameObject);
        }

        public override void OnSelect(BaseEventData eventBaseData)
        {
            onSelect?.Invoke(gameObject);
        }

        public override void OnUpdateSelected(BaseEventData eventBaseData)
        {
            onUpdateSelect?.Invoke(gameObject);
        }
    }
}