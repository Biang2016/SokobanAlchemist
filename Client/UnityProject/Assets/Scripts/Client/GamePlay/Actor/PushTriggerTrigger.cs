using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PushTriggerTrigger : MonoBehaviour
{
    public PushTrigger PushTrigger;

    void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_Box)
        {
            BoxBase box = collider.gameObject.GetComponent<BoxBase>();
            if (box)
            {
                if (box.Pushable())
                {
                    PushTrigger.Model.transform.DOPause();
                    PushTrigger.Model.transform.DOLocalMove(PushTrigger.DefaultModelPos + Vector3.forward * 0.5f, 0.2f);
                    box.Push(PushTrigger.Actor.CurMoveAttempt);
                }
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_Box)
        {
            BoxBase box = collider.gameObject.GetComponent<BoxBase>();
            if (box)
            {
                if (box.Pushable())
                {
                    PushTrigger.Model.transform.DOLocalMove(PushTrigger.DefaultModelPos, 0.2f);
                    box.PushCanceled();
                }
            }
        }
    }
}