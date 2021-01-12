using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using UnityEngine;

public class WorldDeadZoneTrigger : PoolObject
{
    private BoxCollider BoxCollider;

    void Awake()
    {
        BoxCollider = GetComponent<BoxCollider>();
    }

    public void Initialize(GridPos3D gp)
    {
        transform.position = gp.ToVector3() * WorldModule.MODULE_SIZE;
        BoxCollider.size = Vector3.one * (WorldModule.MODULE_SIZE - 0.5f);
        BoxCollider.center = 0.5f * Vector3.one * (WorldModule.MODULE_SIZE - 1);
    }

    void OnTriggerEnter(Collider collider)
    {
        CheckObject(collider);
    }

    void OnTriggerStay(Collider collider)
    {
        //CheckObject(collider);
    }

    private void CheckObject(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Box || collider.gameObject.layer == LayerManager.Instance.Layer_BoxOnlyDynamicCollider)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box)
            {
                box.PlayCollideFX();
                box.DestroyBox();
            }
        }

        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            ActorFaceHelper actorFaceHelper = collider.gameObject.GetComponent<ActorFaceHelper>();
            Actor actor = collider.gameObject.GetComponentInParent<Actor>();
            if (actor && !actorFaceHelper)
            {
                Debug.Log("Actor die in WorldDeadZone:" + name);
                actor.ActorBattleHelper.LoseLife();
            }
        }
    }
}