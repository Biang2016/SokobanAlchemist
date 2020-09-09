using BiangStudio.GameDataFormat.Grid;
using UnityEngine;
using BiangStudio.ObjectPool;

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

    private static void CheckObject(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_Box)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box)
            {
                WorldManager.Instance.CurrentWorld.RemoveBox(box);
                box.PlayDestroyFX();
                box.PoolRecycle();
            }
        }

        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            ActorFaceHelper actorFaceHelper = collider.gameObject.GetComponent<ActorFaceHelper>();
            Actor actor = collider.gameObject.GetComponentInParent<Actor>();
            if (actor && !actorFaceHelper)
            {
                actor.ActorBattleHelper.Die();
            }
        }

        if (collider.gameObject.layer == LayerManager.Instance.Layer_ItemDropped)
        {
            // todo
        }
    }
}