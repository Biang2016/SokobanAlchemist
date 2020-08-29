using UnityEngine;
using UnityEngine.Events;

namespace BiangStudio.DragHover
{
    public class DragProcessor<T> : DragProcessor where T : MonoBehaviour
    {
        private UnityAction<T, Collider, DragProcessor> OnBeginDrag;
        private UnityAction<T, Collider, DragProcessor> OnCancelDrag;

        public void Init(
            Camera camera,
            int layerMask,
            DragManager.GetScreenMousePositionDelegate getScreenMousePositionHandler,
            DragManager.ScreenMousePositionToWorldDelegate screenMousePositionToWorldHandler,
            UnityAction<T, Collider, DragProcessor> onBeginDrag,
            UnityAction<T, Collider, DragProcessor> onCancelDrag,
            float maxRaycastDistance = 1000f)
        {
            base.Init(camera, layerMask, getScreenMousePositionHandler, screenMousePositionToWorldHandler, maxRaycastDistance);
            OnBeginDrag = onBeginDrag;
            OnCancelDrag = onCancelDrag;
            DragManager.Instance.RegisterDragProcessor(this);
        }

        public override void ExecuteDrag()
        {
            base.ExecuteDrag();
            Ray ray = Camera.ScreenPointToRay(CurrentMousePosition_Screen);
            Physics.Raycast(ray, out RaycastHit hit, MaxRaycastDistance, LayerMask);
            if (hit.collider)
            {
                T dragItem = hit.collider.gameObject.GetComponentInParent<T>();
                if (dragItem)
                {
                    DragManager.Instance.OnCancelDrag = () => OnCancelDrag?.Invoke(dragItem, hit.collider, this);
                    DragManager.Instance.CurrentDrag = dragItem.gameObject.GetComponent<Draggable>();
                    DragManager.Instance.CurrentDrag.SetOnDrag(true, hit.collider, this);
                    OnBeginDrag?.Invoke(dragItem, hit.collider, this);
                }
            }
        }
    }
}