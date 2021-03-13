using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.Singleton;
using UnityEngine;
using UnityEngine.Events;

namespace BiangLibrary.DragHover
{
    public class DragManager : TSingletonBaseManager<DragManager>
    {
        public delegate void LogErrorDelegate(string log);

        public delegate bool DragKeyDelegate();

        public delegate bool GetScreenMousePositionDelegate(out Vector2 mouseScreenPos);

        public delegate bool ScreenMousePositionToWorldDelegate(out Vector3 pos_world, out Vector3 pos_local, out Vector3 pos_matrix, out GridPos gp_matrix);

        private LogErrorDelegate LogErrorHandler;
        private DragKeyDelegate DragKeyDownHandler;
        private DragKeyDelegate DragKeyUpHandler;
        internal int DragAreaLayerMask;

        public List<Draggable> AllDraggables = new List<Draggable>();

        internal UnityAction OnCancelDrag;
        private List<DragProcessor> DragProcessors = new List<DragProcessor>();

        private Draggable currentDrag;
        private Draggable pausedDrag; // last drag that was interrupted by current drag, when current drag fails, the last drag also fails.

        public bool ForbidDrag = false;
        public DragAreaIndicator Current_DragAreaIndicator;

        public Draggable CurrentDrag
        {
            get { return currentDrag; }
            set
            {
                if (currentDrag != value)
                {
                    CancelCurrentDrag();
                    currentDrag = value;
                }
            }
        }

        public Draggable PausedDrag => pausedDrag;

        public void Init(DragKeyDelegate dragKeyDownHandler, DragKeyDelegate dragKeyUpHandler, LogErrorDelegate logErrorHandler, int dragAreaLayerMask)
        {
            DragKeyDownHandler = dragKeyDownHandler;
            DragKeyUpHandler = dragKeyUpHandler;
            LogErrorHandler = logErrorHandler;
            DragAreaLayerMask = dragAreaLayerMask;
        }

        public override void Update(float deltaTime)
        {
            Current_DragAreaIndicator = CheckCurrentDragAreaIndicator();
            if (ForbidDrag)
            {
                CancelCurrentAndLastDrag();
            }
            else
            {
                CheckDragStartOrEnd();
            }
        }

        internal void RegisterDragProcessor(DragProcessor dragProcessor)
        {
            if (!DragProcessors.Contains(dragProcessor))
            {
                DragProcessors.Add(dragProcessor);
            }
        }

        internal void UnregisterDragProcessor(DragProcessor dragProcessor)
        {
            DragProcessors.Remove(dragProcessor);
        }

        private void CheckDragStartOrEnd()
        {
            foreach (DragProcessor dragProcessor in DragProcessors)
            {
                dragProcessor.Update();
                if (DragKeyDownHandler != null && DragKeyDownHandler.Invoke())
                {
                    if (!CurrentDrag)
                    {
                        dragProcessor.ExecuteDrag();
                    }
                }
            }

            if (DragKeyUpHandler != null && DragKeyUpHandler.Invoke())
            {
                CancelCurrentAndLastDrag();
            }
        }

        private DragAreaIndicator CheckCurrentDragAreaIndicator()
        {
            foreach (DragProcessor dragProcessor in DragProcessors)
            {
                DragAreaIndicator dragAreaIndicator = dragProcessor.GetCurrentDragAreaIndicator();
                if (dragAreaIndicator != null)
                {
                    return dragAreaIndicator;
                }
            }

            return null;
        }

        internal void CancelCurrentAndLastDrag()
        {
            ResumePausedDrag();
            CancelCurrentDrag();
        }

        internal void CancelCurrentDrag()
        {
            if (currentDrag)
            {
                OnCancelDrag?.Invoke();
                Draggable cDrag = currentDrag;
                currentDrag = null;
                cDrag.SetOnDrag(false, null, null);
            }
        }

        public void PauseDrag()
        {
            if (currentDrag)
            {
                currentDrag.Pause();
                pausedDrag = currentDrag;
                currentDrag = null;
            }
        }

        public void ResumePausedDrag()
        {
            if (currentDrag)
            {
                CancelCurrentDrag();
                currentDrag = pausedDrag;
                pausedDrag = null;
                if (currentDrag)
                {
                    currentDrag.Resume();
                }
            }
        }

        public void SucceedPausedDrag()
        {
            if (pausedDrag)
            {
                pausedDrag.SucceedWhenPaused();
                pausedDrag = null;
            }
        }

        public DragProcessor<T> GetDragProcessor<T>() where T : MonoBehaviour
        {
            foreach (DragProcessor dragProcessor in DragProcessors)
            {
                if (dragProcessor is DragProcessor<T> dp)
                {
                    return dp;
                }
            }

            return null;
        }

        internal static void LogError(string log)
        {
            Instance.LogErrorHandler?.Invoke(log);
        }

        public override void ShutDown()
        {
            base.ShutDown();
            DragProcessors.Clear();
        }
    }
}