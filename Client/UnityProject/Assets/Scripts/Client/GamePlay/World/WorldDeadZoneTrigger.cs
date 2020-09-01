using UnityEngine;
using System.Collections;

public class WorldDeadZoneTrigger : MonoBehaviour
{
    private BoxCollider BoxCollider;

    void Awake()
    {
        BoxCollider = GetComponent<BoxCollider>();
    }

    public void Initialize(Vector3 position)
    {
        transform.position = position;
        BoxCollider.size = Vector3.one * WorldModule.MODULE_SIZE;
    }

    void OnTriggerEnter(Collider collider)
    {
        CheckObject(collider);
    }

    void OnTriggerStay(Collider collider)
    {
        CheckObject(collider);
    }

    private static void CheckObject(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_Box)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box)
            {
                WorldManager.Instance.CurrentWorld.RemoveBoxForPhysics(box);
                if (box.Rigidbody)
                {
                    DestroyImmediate(box.Rigidbody);
                }

                box.PoolRecycle();
            }
        }

        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Actor actor = collider.gameObject.GetComponentInParent<Actor>();
            if (actor)
            {
                Debug.LogError($"玩家出界 {actor}");
            }
        }

        if (collider.gameObject.layer == LayerManager.Instance.Layer_ItemDropped)
        {
            // todo
        }
    }
}