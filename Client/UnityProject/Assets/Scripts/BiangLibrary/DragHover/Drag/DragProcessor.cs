using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

namespace BiangLibrary.DragHover
{
    public abstract class DragProcessor
    {
        public Camera Camera;
        protected int LayerMask;
        public int M_LayerMask => LayerMask;
        protected float MaxRaycastDistance;
        protected DragManager.GetScreenMousePositionDelegate GetScreenMousePositionHandler;
        protected DragManager.ScreenMousePositionToWorldDelegate ScreenMousePositionToWorldHandler;

        public Vector2 DeltaMousePosition_Screen => CurrentMousePosition_Screen - LastMousePosition_Screen;
        public Vector2 LastMousePosition_Screen;
        public Vector2 CurrentMousePosition_Screen;
        public Vector2 DeltaMousePosition_World => CurrentMousePosition_World - LastMousePosition_World;
        public Vector3 LastMousePosition_World;
        public Vector3 CurrentMousePosition_World;

        public void Update()
        {
            LastMousePosition_Screen = CurrentMousePosition_Screen;
            if (GetScreenMousePositionHandler != null)
            {
                if (GetScreenMousePositionHandler.Invoke(out Vector2 newScreenPos))
                {
                    CurrentMousePosition_Screen = newScreenPos;
                }
            }

            LastMousePosition_World = CurrentMousePosition_World;
            if (ScreenMousePositionToWorldHandler != null)
            {
                ScreenMousePositionToWorldHandler.Invoke(out Vector3 pos_world, out Vector3 pos_local, out Vector3 pos_matrix, out GridPos gp_matrix);
                CurrentMousePosition_World = pos_world;
            }
        }

        public virtual void ExecuteDrag()
        {
        }

        public void Init(
            Camera camera,
            int layerMask,
            DragManager.GetScreenMousePositionDelegate getScreenMousePositionHandler,
            DragManager.ScreenMousePositionToWorldDelegate screenMousePositionToWorldHandler,
            float maxRaycastDistance = 1000f)
        {
            Camera = camera;
            LayerMask = layerMask;
            GetScreenMousePositionHandler = getScreenMousePositionHandler;
            ScreenMousePositionToWorldHandler = screenMousePositionToWorldHandler;
            MaxRaycastDistance = maxRaycastDistance;
        }

        public DragAreaIndicator GetCurrentDragAreaIndicator()
        {
            Ray ray = Camera.ScreenPointToRay(CurrentMousePosition_Screen);
            RaycastHit[] hits = Physics.RaycastAll(ray, MaxRaycastDistance, DragManager.Instance.DragAreaLayerMask);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider)
                {
                    DragAreaIndicator dai = hit.collider.gameObject.GetComponentInParent<DragAreaIndicator>();
                    if (dai != null) return dai;
                }
            }

            return null;
        }
    }
}