using UnityEngine;
using UnityEngine.EventSystems;

namespace BiangLibrary.GamePlay.UI
{
    public class BaseUIPanel : MonoBehaviour
    {
        public UIType UIType = new UIType();

        public bool IsShown = false;

        #region 窗体的四种(生命周期)状态

        private bool closeFlag = false;

        void Awake()
        {
        }

        void Update()
        {
        }

        void FixedUpdate()
        {
            ChildFixedUpdate();
            if (UIType.IsESCClose)
            {
                if (UIManager.Instance.CloseUIFormKeyDownHandler != null && UIManager.Instance.CloseUIFormKeyDownHandler.Invoke())
                {
                    BaseUIPanel peek = UIManager.Instance.GetPeekUIForm();
                    if (peek == null || peek == this)
                    {
                        closeFlag = true;
                        return;
                    }
                }
            }

            if (UIType.IsClickElsewhereClose)
            {
                bool mouseLeftDown = UIManager.Instance.MouseLeftButtonDownHandler != null && UIManager.Instance.MouseLeftButtonDownHandler.Invoke();
                bool mouseRightDown = UIManager.Instance.MouseRightButtonDownHandler != null && UIManager.Instance.MouseRightButtonDownHandler.Invoke();
                bool isClickElseWhere = (mouseLeftDown && !EventSystem.current.IsPointerOverGameObject()) || mouseRightDown;
                if (isClickElseWhere)
                {
                    BaseUIPanel peek = UIManager.Instance.GetPeekUIForm();
                    if (peek == null || peek == this)
                    {
                        closeFlag = true;
                        return;
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (closeFlag)
            {
                CloseUIForm();
                closeFlag = false;
            }
        }

        protected virtual void ChildFixedUpdate()
        {
        }

        public virtual void Display()
        {
            IsShown = true;
            gameObject.SetActive(true);
            UIMaskMgr.Instance.SetMaskWindow(gameObject, UIType.UIForms_Type, UIType.UIForm_LucencyType);
        }

        public virtual void Hide()
        {
            IsShown = false;
            gameObject.SetActive(false);
            UIMaskMgr.Instance.CancelAllMaskWindow(UIType.UIForm_LucencyType);
        }

        public virtual void Freeze()
        {
            gameObject.SetActive(true);
        }

        public void CloseUIForm()
        {
            string UIFormName = GetType().Name;
            UIManager.Instance.CloseUIForm(UIFormName);
        }

        #endregion
    }
}