using System;

namespace BiangStudio.DragHover
{
    public class DragAreaDefines
    {
        public static DragArea None = new DragArea("None");
    }

    [Serializable]
    public struct DragArea
    {
        public string DragAreaName;

        public DragArea(string dragAreaName)
        {
            DragAreaName = dragAreaName;
        }

        public bool Equals(DragArea other)
        {
            return DragAreaName.Equals(other.DragAreaName);
        }
    }
}