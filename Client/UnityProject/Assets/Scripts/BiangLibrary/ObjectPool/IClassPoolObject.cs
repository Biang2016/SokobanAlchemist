using UnityEngine;

namespace BiangLibrary.ObjectPool
{
    public interface IClassPoolObject<T>
    {
        void OnUsed();
        void OnRelease();
        void SetPoolIndex(int index);
        int GetPoolIndex();
    }
}