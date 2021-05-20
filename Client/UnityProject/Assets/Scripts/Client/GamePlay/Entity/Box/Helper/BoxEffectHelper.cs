using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;

public class BoxEffectHelper : BoxMonoHelper
{
    private List<BoxTrail> BoxTrails = new List<BoxTrail>();

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        IsShown = true;
        HideTrails();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        IsShown = true;
        HideTrails();
    }

    private bool IsShown = false;

    public void ShowTrails()
    {
        if (IsShown) return;
        foreach (GridPos3D offset in Box.GetEntityOccupationGPs_Rotated())
        {
            BoxTrail boxTrail = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxTrail].AllocateGameObject<BoxTrail>(transform);
            BoxTrails.Add(boxTrail);
            boxTrail.Play();
            boxTrail.transform.localPosition = offset;
        }

        IsShown = true;
    }

    public void HideTrails()
    {
        if (!IsShown) return;
        foreach (BoxTrail boxTrail in BoxTrails)
        {
            boxTrail.Stop();
            boxTrail.PoolRecycle();
        }

        BoxTrails.Clear();
        IsShown = false;
    }
}