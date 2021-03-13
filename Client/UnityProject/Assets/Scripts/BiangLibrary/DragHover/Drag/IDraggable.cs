using UnityEngine;

namespace BiangLibrary.DragHover
{
    public interface IDraggable
    {
        void Draggable_OnMouseDown(DragAreaIndicator dragAreaIndicator, Collider collider);

        void Draggable_OnMousePressed(DragAreaIndicator dragAreaIndicator, Vector3 diffFromStart, Vector3 deltaFromLastFrame);

        void Draggable_OnMouseUp(DragAreaIndicator dragAreaIndicator, Vector3 diffFromStart, Vector3 deltaFromLastFrame);

        void Draggable_OnPaused();

        void Draggable_OnResume();

        void Draggable_OnSucceedWhenPaused();

        void Draggable_SetStates(ref bool canDrag, ref DragAreaIndicator dragFromDragAreaIndicator);
    }
}