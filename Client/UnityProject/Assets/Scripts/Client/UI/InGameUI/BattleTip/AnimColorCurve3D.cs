using System;
using System.Collections.Generic;
using UnityEngine;

namespace Client
{
    [Serializable]
    public class AnimColorCurve3D
    {
        public enum Type
        {
            R,
            G,
            B,
            A
        }

        [Serializable]
        public class CurveDefine
        {
            public Type curveType;
            public AnimationCurve curve;
        }

        [SerializeField]
        private List<CurveDefine> curves;

        public float interval = 0.2f;
        public float duration = 1;
        public float delay = 0;

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
            animateTimeTotal = duration + (unitNum - 1) * interval + delay;
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

        public Color GetColor(int index, Color colorOrigin)
        {
            Color colorRet = colorOrigin;
            float percent = Mathf.Clamp01((animateTime - index * interval - delay) / duration);
            foreach (CurveDefine c in curves)
            {
                float value = 0;
                if (c.curve != null)
                {
                    value = c.curve.Evaluate(percent);
                }

                switch (c.curveType)
                {
                    case Type.R:
                        colorRet.r = value * 1f;
                        break;

                    case Type.G:
                        colorRet.g = value * 1f;
                        break;

                    case Type.B:
                        colorRet.b = value * 1f;
                        break;

                    case Type.A:
                        colorRet.a = value * 1f;
                        break;
                }
            }

            return colorRet;
        }
    }
}