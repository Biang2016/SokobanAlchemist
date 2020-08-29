using UnityEngine;

namespace BiangStudio.DragHover
{
    public class Draggable : MonoBehaviour
    {
        protected IDraggable caller;
        private bool isDragging = false;
        protected bool canDrag;
        protected DragArea dragFrom_DragArea;

        public Vector3 StartDragPos;

        public DragProcessor MyDragProcessor;

        void Awake()
        {
            caller = GetComponent<IDraggable>();
            if (caller == null)
            {
                DragManager.LogError("Couldn't find IDraggable in the gameObject with Draggable component.");
            }

            DragManager.Instance.AllDraggables.Add(this);
        }

        public void Update()
        {
            if (!canDrag) return;
            if (isDragging)
            {
                caller.Draggable_OnMousePressed(DragManager.Instance.Current_DragArea, MyDragProcessor.CurrentMousePosition_World - StartDragPos, MyDragProcessor.DeltaMousePosition_World);
            }
        }

        public void SetOnDrag(bool drag, Collider collider, DragProcessor dragProcessor)
        {
            if (isDragging != drag)
            {
                if (drag) // Press
                {
                    MyDragProcessor = dragProcessor;
                    caller.Draggable_SetStates(ref canDrag, ref dragFrom_DragArea);
                    if (canDrag)
                    {
                        StartDragPos = MyDragProcessor.CurrentMousePosition_World;
                        caller.Draggable_OnMouseDown(dragFrom_DragArea, collider);
                        isDragging = true;
                    }
                    else
                    {
                        isDragging = false;
                        DragManager.Instance.CancelDrag();
                    }
                }
                else // Release
                {
                    if (canDrag)
                    {
                        caller.Draggable_OnMouseUp(DragManager.Instance.Current_DragArea, MyDragProcessor.CurrentMousePosition_World - StartDragPos, MyDragProcessor.DeltaMousePosition_World);
                        DragManager.Instance.CurrentDrag = null;
                    }
                    else
                    {
                        DragManager.Instance.CurrentDrag = null;
                    }

                    dragFrom_DragArea = DragAreaDefines.None;
                    isDragging = false;
                    MyDragProcessor = null;
                }
            }
        }

        void OnDestroy()
        {
            DragManager.Instance.AllDraggables.Remove(this);
        }
    }
}