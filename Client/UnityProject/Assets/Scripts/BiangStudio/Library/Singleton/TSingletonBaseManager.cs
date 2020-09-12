namespace BiangStudio.Singleton
{
    public abstract class TSingletonBaseManager<T> : TSingleton<T> where T : new()
    {
        public virtual void Awake()
        {
        }

        public virtual void Start()
        {
        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual void LateUpdate(float deltaTime)
        {
        }

        public virtual void FixedUpdate(float deltaTime)
        {
        }

        public virtual void ShutDown()
        {

        }
    }
}