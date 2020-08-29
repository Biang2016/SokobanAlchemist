using UnityEngine;
using UnityEngine.Events;

namespace BiangStudio.DragHover
{
    public class MouseHoverUI : MonoBehaviour, IMouseHoverComponent
    {
        [SerializeField]
        private Animator Anim;

        [SerializeField]
        private string Hover_SFX;

        [SerializeField]
        [Range(0, 1)]
        private float Hover_SFX_Volume;

        private bool isHover;

        public delegate void MouseHoverBegin();

        public MouseHoverBegin MouseHoverBeginHandler;

        public delegate void MouseHoverEnd();

        public MouseHoverEnd MouseHoverEndHandler;

        public delegate void MouseFocusBegin();

        public MouseFocusBegin MouseFocusBeginHandler;

        public delegate void MouseFocusEnd();

        public MouseFocusEnd MouseFocusEndHandler;

        public delegate void MousePressEnter();

        public MousePressEnter MousePressEnterHandler;

        public delegate void MousePressLeave();

        public MousePressLeave MousePressLeaveHandler;

        public static UnityAction<string, float> OnPlaySoundAction;

        public void MouseHoverComponent_OnHoverBegin(Vector3 mousePosition)
        {
            MouseHoverBeginHandler?.Invoke();
        }

        public void MouseHoverComponent_OnHoverEnd()
        {
            MouseHoverEndHandler?.Invoke();
        }

        public void MouseHoverComponent_OnFocusBegin(Vector3 mousePosition)
        {
            if (isHover) return;
            MouseFocusBeginHandler?.Invoke();
            Anim?.SetTrigger("OnMouseEnter");
            isHover = true;
            OnPlaySoundAction?.Invoke(Hover_SFX, Hover_SFX_Volume);
        }

        public void MouseHoverComponent_OnFocusEnd()
        {
            if (!isHover) return;
            MouseFocusEndHandler?.Invoke();
            Anim?.SetTrigger("OnMouseLeave");
            isHover = false;
        }

        public void MouseHoverComponent_OnMousePressEnterImmediately(Vector3 mousePosition)
        {
            MousePressEnterHandler?.Invoke();
        }

        public void MouseHoverComponent_OnMousePressLeaveImmediately()
        {
            MousePressLeaveHandler?.Invoke();
        }
    }
}