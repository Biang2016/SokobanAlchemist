using UnityEngine;

namespace BiangLibrary.DragHover
{
    public class Draggable : MonoBehaviour
    {
        protected IDraggable caller;
        private bool isDragging = false;
        private bool isPaused = false;
        protected bool canDrag;
        protected DragAreaIndicator dragFrom_DragAreaIndicator;

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
            if (isPaused) return;
            if (isDragging)
            {
                caller.Draggable_OnMousePressed(DragManager.Instance.Current_DragAreaIndicator, MyDragProcessor.CurrentMousePosition_World - StartDragPos, MyDragProcessor.DeltaMousePosition_World);
            }
        }

        public void SetOnDrag(bool drag, Collider collider, DragProcessor dragProcessor)
        {
            if (isDragging != drag)
            {
                if (drag) // Press
                {
                    MyDragProcessor = dragProcessor;
                    caller.Draggable_SetStates(ref canDrag, ref dragFrom_DragAreaIndicator);
                    if (canDrag)
                    {
                        StartDragPos = MyDragProcessor.CurrentMousePosition_World;
                        caller.Draggable_OnMouseDown(dragFrom_DragAreaIndicator, collider);
                        isDragging = true;
                    }
                    else
                    {
                        isDragging = false;
                        DragManager.Instance.CancelCurrentAndLastDrag();
                    }
                }
                else // Release
                {
                    if (canDrag)
                    {
                        caller.Draggable_OnMouseUp(DragManager.Instance.Current_DragAreaIndicator, MyDragProcessor.CurrentMousePosition_World - StartDragPos, MyDragProcessor.DeltaMousePosition_World);
                        DragManager.Instance.CurrentDrag = null;
                    }
                    else
                    {
                        DragManager.Instance.CurrentDrag = null;
                    }

                    dragFrom_DragAreaIndicator = null;
                    isDragging = false;
                    MyDragProcessor = null;
                }
            }
        }

        /// <summary>
        /// Can only be called from DragManager
        /// </summary>
        public void Pause()
        {
            isPaused = true;
            caller.Draggable_OnPaused();
        }

        /// <summary>
        /// Can only be called from DragManager
        /// </summary>
        public void Resume()
        {
            isPaused = false;
            caller.Draggable_OnResume();
        }

        /// <summary>
        /// Can only be called from DragManager
        /// </summary>
        public void SucceedWhenPaused()
        {
            isPaused = false;
            caller.Draggable_OnSucceedWhenPaused();
        }

        void OnDestroy()
        {
            DragManager.Instance.AllDraggables.Remove(this);
        }
    }
}