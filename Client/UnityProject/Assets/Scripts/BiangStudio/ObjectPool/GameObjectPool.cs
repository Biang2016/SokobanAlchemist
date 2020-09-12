using UnityEngine;
using UnityEngine.Profiling;

namespace BiangStudio.ObjectPool
{
    public class GameObjectPool : MonoBehaviour
    {
        PoolObject[] gameObjectPool; //对象池

        bool[] isUsed; //已使用的对象

        private int capacity; //对象池容量，根据场景中可能出现的最多数量的该对象预估一个容量
        private int used; //已使用多少个对象
        private int notUsed; //多少个对象已实例化但未使用
        private int empty; //对象池中未实例化的空位置的个数

        public PoolObject gameObjectPrefab;

        //记录对象原始的位置、旋转、缩放，以便还原
        Vector3 gameObjectDefaultPosition;
        Quaternion gameObjectDefaultRotation;
        Vector3 gameObjectDefaultScale;

        public static Vector3 GameObjectPoolPosition = new Vector3(-3000f, -3000f, 0f);

        private int InitialCapacity = 0;

        public void Initiate(PoolObject prefab, int initialCapacity)
        {
            InitialCapacity = initialCapacity;
            if (prefab != null)
            {
                transform.position = GameObjectPoolPosition;
                gameObjectPrefab = prefab;
                gameObjectDefaultPosition = gameObjectPrefab.transform.position;
                gameObjectDefaultRotation = gameObjectPrefab.transform.rotation;
                gameObjectDefaultScale = gameObjectPrefab.transform.localScale;
                gameObjectPool = new PoolObject[initialCapacity];
                isUsed = new bool[initialCapacity];
                capacity = initialCapacity;
                empty = capacity;
            }
            else
            {
                Debug.Log(name + " prefab == null");
            }
        }

        public T AllocateGameObject<T>(Transform parent) where T : PoolObject
        {
            if (gameObjectPrefab != null)
            {
                for (int i = 0; i < capacity; i++)
                {
                    if (!isUsed[i])
                    {
                        if (gameObjectPool[i])
                        {
                            gameObjectPool[i].gameObject.SetActive(true);
                            gameObjectPool[i].transform.SetParent(parent);
                            gameObjectPool[i].transform.localPosition = gameObjectDefaultPosition;
                            gameObjectPool[i].transform.localRotation = gameObjectDefaultRotation;
                            gameObjectPool[i].transform.localScale = gameObjectDefaultScale;
                            used++;
                            notUsed--;
                        }
                        else
                        {
                            Profiler.BeginSample($"PoolAlloc_{gameObjectPrefab.name}");
                            gameObjectPool[i] = Instantiate(gameObjectPrefab, parent);
                            gameObjectPool[i].name = gameObjectPrefab.name + "_" + i; //便于调试的时候分辨对象
                            gameObjectPool[i].SetObjectPool(this);
                            empty--;
                            used++;
                            Profiler.EndSample();
                        }

                        isUsed[i] = true;
                        gameObjectPool[i].OnUsed();
                        gameObjectPool[i].IsRecycled = false;
                        return (T) gameObjectPool[i];
                    }
                }

                expandCapacity();
                return AllocateGameObject<T>(parent);
            }

            return null;
        }

        public void OptimizePool()
        {
            int usedCount = 0;
            for (int i = 0; i < capacity; i++)
            {
                if (isUsed[i])
                {
                    usedCount++;
                }
            }

            if (usedCount < InitialCapacity && capacity > InitialCapacity)
            {
                PoolObject[] newGameObjectPool = new PoolObject[InitialCapacity];
                bool[] newIsUsed = new bool[InitialCapacity];

                int index = 0;
                for (int i = 0; i < capacity; i++)
                {
                    if (isUsed[i])
                    {
                        if (gameObjectPool[i])
                        {
                            newGameObjectPool[index] = gameObjectPool[i];
                            newGameObjectPool[index].PoolRecycle();
                            newIsUsed[index] = true;
                            index++;
                        }
                    }
                    else
                    {
                        if (gameObjectPool[i])
                        {
                            Destroy(gameObjectPool[i].gameObject);
                        }
                    }
                }

                capacity = InitialCapacity;
                used = usedCount;
                notUsed = 0;
                empty = capacity - used - notUsed;

                gameObjectPool = newGameObjectPool;
            }
        }

        public void RecycleGameObject(PoolObject recGameObject)
        {
            for (int i = 0; i < capacity; i++)
            {
                if (gameObjectPool[i] == recGameObject)
                {
                    isUsed[i] = false;
                    recGameObject.transform.SetParent(transform);
                    recGameObject.transform.localPosition = gameObjectDefaultPosition;
                    used--;
                    notUsed++;
                    return;
                }
            }

            Destroy(recGameObject.gameObject, 0.1f);
        }

        void expandCapacity()
        {
            PoolObject[] new_gameObjectPool = new PoolObject[capacity * 2];
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