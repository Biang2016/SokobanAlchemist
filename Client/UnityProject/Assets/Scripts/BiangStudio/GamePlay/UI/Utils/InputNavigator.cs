using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BiangStudio.GamePlay.UI
{
    /// <summary>
    /// UGUI Tab键切换InputField
    /// </summary>
    public class InputNavigator : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private EventSystem system;
        private bool isSelect = false;

        void Start()
        {
            system = EventSystem.current;
        }

        void Update()
        {
            if (UIManager.Instance.InputNavigateKeyDownHandler != null && UIManager.Instance.InputNavigateKeyDownHandler.Invoke() && isSelect)
            {
                Selectable next = null;
                Selectable sec = system.currentSelectedGameObject.GetComponent<Selectable>();

                next = sec.FindSelectableOnDown();

                if (next == null) //Recycle
                {
                    Selectable temp = sec;
                    Selectable temp_notNull = temp;
                    while (temp != null)
                    {
                        temp_notNull = temp;
                        temp = temp.FindSelectableOnUp();
                    }

                    next = temp_notNull;
                }

                InputField inputField = next.GetComponent<InputField>();
                if (inputField == null) return;
                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            isSelect = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            isSelect = false;
        }
    }
}