using UnityEngine;

namespace BiangStudio.DragHover
{
    public class MouseHoverComponent : MonoBehaviour
    {
        /// <summary>
        /// 鼠标悬停功能，应用于模块等
        /// 实现悬停过程中卡牌展示的功能
        /// </summary>
        private IMouseHoverComponent caller;

        void Awake()
        {
            caller = GetComponent<IMouseHoverComponent>();
        }

        void Start()
        {
        }

        void Update()
        {
        }

        private bool isOnFocus;

        internal bool IsOnFocus
        {
            get { return isOnFocus; }

            set
            {
                if (!isOnFocus && value)
                {
                    Vector3 cameraPosition = Camera.main.ScreenToWorldPoint(MouseHoverManager.Instance.GetMousePositionHandler());
                    isOnFocus = value;
                    caller.MouseHoverComponent_OnFocusBegin(cameraPosition);
                }

                else if (isOnFocus && !value)
                {
                    isOnFocus = value;
                    caller.MouseHoverComponent_OnFocusEnd();
                }
            }
        }

        private bool isOnHover;

        internal bool IsOnHover
        {
            get { return isOnHover; }

            set
            {
                if (!isOnHover && value)
                {
                    Vector3 cameraPosition = Camera.main.ScreenToWorldPoint(MouseHoverManager.Instance.GetMousePositionHandler());
                    isOnHover = value;
                    caller.MouseHoverComponent_OnHoverBegin(cameraPosition);
                }

                else if (isOnHover && !value)
                {
                    isOnHover = value;
                    caller.MouseHoverComponent_OnHoverEnd();
                }
            }
        }

        private bool isOnPressHover;

        internal bool IsOnPressHover
        {
            get { return isOnPressHover; }

            set
            {
                if (!isOnPressHover && value)
                {
                    Vector3 cameraPosition = Camera.main.ScreenToWorldPoint(MouseHoverManager.Instance.GetMousePositionHandler());
                    isOnPressHover = value;
                    caller.MouseHoverComponent_OnMousePressEnterImmediately(cameraPosition);
                }

                else if (isOnPressHover && !value)
                {
                    isOnPressHover = value;
                    caller.MouseHoverComponent_OnMousePressLeaveImmediately();
                }
            }
        }
    }

    public interface IMouseHoverComponent
    {
        /// <summary>
        /// 此接口用于将除了MouseHoverComponent通用效果之外的效果还给调用者自行处理
        /// </summary>
        void MouseHoverComponent_OnHoverBegin(Vector3 mousePosition);

        void MouseHoverComponent_OnHoverEnd();
        void MouseHoverComponent_OnFocusBegin(Vector3 mousePosition);
        void MouseHoverComponent_OnFocusEnd();
        void MouseHoverComponent_OnMousePressEnterImmediately(Vector3 mousePosition);
        void MouseHoverComponent_OnMousePressLeaveImmediately();
    }
}