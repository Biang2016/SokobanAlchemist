using UnityEngine;
using DG.Tweening;

public class PushTrigger : MonoBehaviour
{
    public Actor Actor;

    public Collider Collider;
    public GameObject Model;
    internal Vector3 DefaultTriggerPos;
    internal Vector3 DefaultModelPos;

    void Awake()
    {
        DefaultTriggerPos = Collider.transform.localPosition;
        DefaultModelPos = Model.transform.localPosition;
    }

    public void PushTriggerOut()
    {
        Collider.transform.DOLocalMove(DefaultTriggerPos + Vector3.forward * 0.5f, 0.2f);
    }

    public void PushTriggerReset()
    {
        Collider.transform.DOPause();
        Collider.transform.DOLocalMove(DefaultTriggerPos, 0.2f);
    }

  
}