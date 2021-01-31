using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

[ExecuteInEditMode]
public class GridSnapper : MonoBehaviour
{
    public int SnapperGridSize = 1;
    public bool EnableInRuntime = true;
    public bool EnableInEditor = true;

    void LateUpdate()
    {
        if ((EnableInRuntime && Application.isPlaying) || (EnableInEditor && !Application.isPlaying))
        {
            GridPos3D gp = GridPos3D.GetGridPosByLocalTrans(transform, SnapperGridSize);
            transform.localPosition = new Vector3(gp.x * SnapperGridSize, gp.y * SnapperGridSize, gp.z * SnapperGridSize);
            Vector3 eulerAngles = transform.localRotation.eulerAngles;
            float y = Mathf.RoundToInt(eulerAngles.y / 90) * 90;
            transform.localRotation = Quaternion.Euler(0, y, 0);
        }
    }
}