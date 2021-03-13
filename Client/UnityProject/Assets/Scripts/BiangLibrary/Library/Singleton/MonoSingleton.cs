using UnityEngine;

namespace BiangLibrary.Singleton
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        protected static bool hasInstance { get; private set; }

        public static T Instance
        {
            get
            {
                if (!hasInstance)
                {
                    instance = FindObjectOfType(typeof(T)) as T;
                    if (instance != null)
                    {
                        hasInstance = true;
                    }
                    else
                    {
                        hasInstance = false;
                    }
                }

                return instance;
            }
            set
            {
                instance = value;
                if (instance != null)
                {
                    hasInstance = true;
                }
                else
                {
                    hasInstance = false;
                }
            }
        }

        public void OnReloadScene()
        {
            hasInstance = false;
        }
    }
}