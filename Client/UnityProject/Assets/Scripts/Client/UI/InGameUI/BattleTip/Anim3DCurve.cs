using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class AnimCurve3D
{
    public enum AnimDirection
    {
        Y,
        X,
        XY
    }

    public AnimDirection direction;

    [HideIf("direction", AnimDirection.Y)]
    public AnimationCurve curveX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f));

    [HideIf("direction", AnimDirection.X)]
    public AnimationCurve curveY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1.0f));

    public float interval = 0.2f;
    public float duration = 1;
    public float delay = 0f;

    [HideIf("direction", AnimDirection.Y)]
    public float maxOffsetX = 5;

    [HideIf("direction", AnimDirection.X)]
    public float maxOffsetY = 5;

    Action onAnimFinish;

    bool isAnimating = false;
    float animateTime = 0;
    float animateTimeTotal = 0;

    public bool IsAnimating
    {
        get { return isAnimating; }
    }

    public void StartAnim(int unitNum, Action onAnimFinish)
    {
        this.onAnimFinish = onAnimFinish;
        isAnimating = true;
        animateTime = 0;
        animateTimeTotal = delay + duration + (unitNum - 1) * interval;
    }

    public void Update(float deltaTime)
    {
        if (!isAnimating)
            return;

        animateTime += deltaTime;
        if (animateTime >= animateTimeTotal)
        {
            isAnimating = false;
            onAnimFinish?.Invoke();
        }
    }

    public float GetCurveValueX(int index)
    {
        if (direction == AnimDirection.Y)
            return 0;
        float percent = Mathf.Clamp01((animateTime - index * interval - delay) / duration);
        return curveX != null ? curveX.Evaluate(percent) * maxOffsetX : 0;
    }

    public float GetCurveValueY(int index)
    {
        if (direction == AnimDirection.X)
            return 0;
        float percent = Mathf.Clamp01((animateTime - index * interval - delay) / duration);
        return curveY != null ? curveY.Evaluate(percent) * maxOffsetY : 0;
    }
}