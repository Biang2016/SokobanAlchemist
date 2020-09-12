using System.Collections;
using UnityEngine;

namespace BiangStudio.ObjectPool
{
    public class PoolObject : MonoBehaviour
    {
        private GameObjectPool Pool { get; set; }

        public bool IsRecycled = false;

        internal void SetObjectPool(GameObjectPool pool)
        {
            Pool = pool;
        }

        private int usedTimes = 0;

        public void PoolRecycle()
        {
            if (IsRecycled) return;
            OnRecycled();
            Pool.RecycleGameObject(this);
            IsRecycled = true;
            usedTimes++;
        }

        public virtual void OnRecycled()
        {
        }

        public virtual void OnUsed()
        {
        }

        public virtual void PoolRecycleAtFrameEnd()
        {
            StartCoroutine(Co_PoolRecycleAtFrameEnd());
        }

        public virtual void PoolRecycle(float delay)
        {
            StartCoroutine(Co_PoolRecycle(delay, usedTimes));
        }

        private IEnumerator Co_PoolRecycle(float delay, int timeMark)
        {
            yield return new WaitForSeconds(delay);
            if (!IsRecycled && usedTimes == timeMark)
            {
                PoolRecycle();
            }
        }
        private IEnumerator Co_PoolRecycleAtFrameEnd()
        {
            yield return new WaitForEndOfFrame();
            if (!IsRecycled)
            {
                PoolRecycle();
            }
        }
    }
}