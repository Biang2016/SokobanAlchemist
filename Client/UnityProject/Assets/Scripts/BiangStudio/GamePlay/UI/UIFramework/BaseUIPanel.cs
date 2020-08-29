using UnityEngine;
using UnityEngine.EventSystems;

namespace BiangStudio.GamePlay.UI
{
    public class BaseUIPanel : MonoBehaviour
    {
        public UIType UIType = new UIType();

        #region 窗体的四种(生命周期)状态

        private bool closeFlag = false;

        void Awake()
        {
        }

        void Update()
        {
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

            ChildUpdate();
        }

        private void LateUpdate()
        {
            if (closeFlag)
            {
                CloseUIForm();
                closeFlag = false;
            }
        }

        protected virtual void ChildUpdate()
        {
        }

        public virtual void Display()
        {
            gameObject.SetActive(true);
            UIMaskMgr.Instance.SetMaskWindow(gameObject, UIType.UIForms_Type, UIType.UIForm_LucencyType);
        }

        public virtual void Hide()
        {
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