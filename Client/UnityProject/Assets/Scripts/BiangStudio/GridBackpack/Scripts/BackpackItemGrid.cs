using System;
using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

namespace BiangStudio.GridBackpack
{
    public class BackpackItemGrid : PoolObject
    {
        public BoxCollider BoxCollider;
        public Text LocalGridPosText;

        public GridPos LocalGridPos;

        public void Initialize(GridPos localGP, GridRect space, GridPosR.OrientationFlag adjacentConnection, GridPosR.OrientationFlag diagonalConnection)
        {
            LocalGridPosText.text = localGP.ToString();
            LocalGridPos = localGP;
            BoxCollider.size = new Vector3(space.size.x, space.size.z, 1);
            BoxCollider.center = new Vector3((space.position.x + 0.5f) * space.size.x, (space.position.z - 0.5f) * space.size.z, 0);
            ImageBorderContainer.anchoredPosition = new Vector2(space.position.x * space.size.x, space.position.z * space.size.z);
            SetConnection(adjacentConnection, diagonalConnection);
        }

        [SerializeField]
        private Image BG;

        [SerializeField]
        private RectTransform ImageBorderContainer;

        [SerializeField]
        private Image[] MainImageBorders;

        [SerializeField]
        private Image[] SideImageBorders;

        public void SetGridColor(Color color)
        {
            foreach (Image image in MainImageBorders)
            {
                image.color = color;
            }

            foreach (Image image in SideImageBorders)
            {
                image.color = color;
            }

            BG.color = new Color(color.r, color.g, color.b, 0.3f);
        }

        public void SetConnection(GridPosR.OrientationFlag adjacentConnection, GridPosR.OrientationFlag diagonalConnection)
        {
            foreach (GridPosR.Orientation orientation in Enum.GetValues(typeof(GridPosR.Orientation)))
            {
                MainImageBorders[(int) orientation].enabled = !adjacentConnection.HasFlag(orientation.ToFlag());

                GridPosR.Orientation next = GridPosR.RotateOrientationClockwise90(orientation);
                if (!adjacentConnection.HasFlag(orientation.ToFlag()) && !adjacentConnection.HasFlag(next.ToFlag()))
                {
                    SideImageBorders[((int) orientation) * 2].enabled = false;
                    SideImageBorders[((int) orientation) * 2 + 1].enabled = false;
                }
                else if (adjacentConnection.HasFlag(orientation.ToFlag()) && adjacentConnection.HasFlag(next.ToFlag()))
                {
                    if (diagonalConnection.HasFlag(orientation.ToFlag()))
                    {
                        SideImageBorders[((int) orientation) * 2].enabled = false;
                        SideImageBorders[((int) orientation) * 2 + 1].enabled = false;
                    }
                    else
                    {
                        SideImageBorders[((int) orientation) * 2].enabled = true;
                        SideImageBorders[((int) orientation) * 2 + 1].enabled = true;
                    }
                }
                else
                {
                    if (adjacentConnection.HasFlag(orientation.ToFlag()))
                    {
                        SideImageBorders[((int) orientation) * 2].enabled = true;
                        SideImageBorders[((int) orientation) * 2 + 1].enabled = false;
                    }
                    else
                    {
                        SideImageBorders[((int) orientation) * 2].enabled = false;
                        SideImageBorders[((int) orientation) * 2 + 1].enabled = true;
                    }
                }
            }
        }
    }
}