using UnityEngine;

namespace BiangStudio.ObjectPool
{
    public class ClassObjectPool<T> where T : IClassPoolObject<T>, new()
    {
        T[] gameObjectPool; //对象池

        bool[] isUsed; //已使用的对象

        private int capacity; //对象池容量，根据场景中可能出现的最多数量的该对象预估一个容量
        private int used; //已使用多少个对象
        private int notUsed; //多少个对象已实例化但未使用
        private int empty; //对象池中未实例化的空位置的个数

        private int InitialCapacity = 0;

        public ClassObjectPool(int initialCapacity)
        {
            InitialCapacity = initialCapacity;
            gameObjectPool = new T[initialCapacity];
            isUsed = new bool[initialCapacity];
            capacity = initialCapacity;
            empty = capacity;
            used = 0;
            notUsed = initialCapacity;
            for (int i = 0; i < initialCapacity; i++)
            {
                Alloc();
            }

            for (int i = 0; i < initialCapacity; i++)
            {
                Release(gameObjectPool[i]);
            }
        }

        public T Alloc()
        {
            for (int i = 0; i < capacity; i++)
            {
                if (!isUsed[i])
                {
                    if (gameObjectPool[i] != null)
                    {
                        used++;
                        notUsed--;
                    }
                    else
                    {
                        gameObjectPool[i] = new T();
                        gameObjectPool[i].SetPoolIndex(i);
                        empty--;
                        used++;
                    }

                    //Debug.Log($"{nameof(T)}, {used}");
                    isUsed[i] = true;
                    gameObjectPool[i].OnUsed();
                    return gameObjectPool[i];
                }
            }

            expandCapacity();
            return Alloc();
        }

        public void Release(T recObject)
        {
            int index = recObject.GetPoolIndex();
            recObject.OnRelease();
            isUsed[index] = false;
            used--;
            notUsed++;
        }

        void expandCapacity()
        {
            T[] new_gameObjectPool = new T[capacity * 2];
            bool[] new_isUsed = new bool[capacity * 2];

            for (int i = 0; i < capacity; i++)
            {
                new_gameObjectPool[i] = gameObjectPool[i];
                new_isUsed[i] = isUsed[i];
            }

            capacity *= 2;
            empty = capacity - used - notUsed;
            gameObjectPool = new_gameObjectPool;
            isUsed = new_isUsed;
        }
    }
}