using System.Collections.Generic;
using UnityEngine;

public class ActorLaunchArcRendererHelper : ActorHelper
{
    private List<Marker> Markers = new List<Marker>();

    private float Velocity;
    private float Angle;
    private float TimeStep;
    private int MarkerCount;

    private float gravity => Physics.gravity.y;
    private float radianAngle;

    private bool Shown;

    public void SetShown(bool shown)
    {
        Shown = shown;
        foreach (Marker mk in Markers)
        {
            mk.SetShown(shown);
        }
    }

    public void Initialize(float velocity, float angle, int resolutionPerUnit, float simulateSeconds)
    {
        Velocity = velocity;
        Angle = angle;
        float resolutionPerSecond = velocity * resolutionPerUnit;
        TimeStep = 1 / resolutionPerSecond;
        MarkerCount = Mathf.CeilToInt(resolutionPerSecond * simulateSeconds);
        while (Markers.Count > MarkerCount)
        {
            Marker removedMarker = Markers[Markers.Count - 1];
            Markers.Remove(removedMarker);
            removedMarker.PoolRecycle();
        }

        while (Markers.Count < MarkerCount)
        {
            Marker marker = Marker.BaseInitialize(MarkerType.LaunchArcMarker, transform);
            Markers.Add(marker);
        }

        if (Shown) RenderArc();
    }

    private void RenderArc()
    {
        Vector3[] points = CalculateArcArray();
        for (int i = 0; i < points.Length; i++)
        {
            Markers[i].transform.localPosition = points[i];
        }
    }

    private Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[MarkerCount];

        radianAngle = Mathf.Deg2Rad * Angle;
        for (int i = 0; i < MarkerCount; i++)
        {
            float t = i * TimeStep;
            arcArray[i] = CalculateArcPoint(t);
        }

        return arcArray;
    }

    private Vector3 CalculateArcPoint(float t)
    {
        float z = Velocity * Mathf.Cos(radianAngle) * t;
        float y = Velocity * Mathf.Sin(radianAngle) * t + 0.5f * gravity * t * t;
        Vector3 pos = new Vector3(0, y, z);
        return pos;
    }
}