using UnityEngine;
using DG.Tweening;

public class PushTrigger : MonoBehaviour
{
    public Actor Actor;
    private Vector3 DefaultPushTriggerPos;

    void Awake()
    {
        DefaultPushTriggerPos = transform.localPosition;
    }

    public void PushTriggerOut()
    {
        transform.DOLocalMove(DefaultPushTriggerPos + Vector3.forward * 0.5f, 0.2f);
    }

    public void PushTriggerReset()
    {
        transform.DOPause();
        transform.DOLocalMove(DefaultPushTriggerPos, 0.2f);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_Box)
        {
            BoxBase box = collider.gameObject.GetComponent<BoxBase>();
            if (box)
            {
                if (box.BoxType == BoxType.WoodenBox)
                {
                    box.ResetPush();
                }
            }
        }
    }

    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_Box)
        {
            BoxBase box = collider.gameObject.GetComponent<BoxBase>();
            if (box)
            {
                if (box.BoxType == BoxType.WoodenBox)
                {
                    box.Push(Time.deltaTime, Actor.CurMoveAttempt);
                }
            }
        }
    }
}