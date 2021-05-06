using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;

public class BoxEffectHelper : BoxMonoHelper
{
    private List<BoxTrail> BoxTrails = new List<BoxTrail>();

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        HideTrails();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }

    public void ShowTrails()
    {
        foreach (GridPos3D offset in Box.GetEntityOccupationGPs_Rotated())
        {
            BoxTrail boxTrail = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxTrail].AllocateGameObject<BoxTrail>(transform);
            BoxTrails.Add(boxTrail);
            boxTrail.Play();
            boxTrail.transform.localPosition = offset;
        }
    }

    public void HideTrails()
    {
        foreach (BoxTrail boxTrail in BoxTrails)
        {
            boxTrail.Stop();
            boxTrail.PoolRecycle();
        }

        BoxTrails.Clear();
    }
}