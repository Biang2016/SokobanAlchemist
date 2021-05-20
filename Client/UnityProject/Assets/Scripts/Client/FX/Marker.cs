using BiangLibrary.ObjectPool;
using UnityEngine;

public class Marker : PoolObject
{
    public Renderer Renderer;
    public MarkerType MarkerType;

    public override void OnRecycled()
    {
        base.OnRecycled();
        SetShown(true);
    }

    public static Marker BaseInitialize(MarkerType markerType, Transform parent)
    {
        Marker marker = GameObjectPoolManager.Instance.MarkerDict[markerType].AllocateGameObject<Marker>(parent);
        marker.Initialize(markerType);
        return marker;
    }

    private void Initialize(MarkerType LaunchArc)
    {
        MarkerType = LaunchArc;
    }

    public void SetShown(bool shown)
    {
        Renderer.enabled = shown;
    }
}

public enum MarkerType
{
    LaunchArcMarker,
    NavTrackMarker,
    NavTrackMarker_Final,
}