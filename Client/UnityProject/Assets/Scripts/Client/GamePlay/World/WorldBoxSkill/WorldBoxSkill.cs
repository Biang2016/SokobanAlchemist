using UnityEngine;

public static class WorldBoxSkill
{
    public static void ExplodePushBox(this Box m_Box, Vector3 center, float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(center, radius, LayerManager.Instance.LayerMask_Box);
        foreach (Collider collider in colliders)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box != null && box.Interactable && box != m_Box)
            {
                if (box.State == Box.States.Static || box.State == Box.States.BeingKicked || box.State == Box.States.BeingPushed || box.State == Box.States.PushingCanceling)
                {
                    Vector3 diff = box.transform.position - center;
                    if (diff.x > diff.z)
                    {
                        diff.z = 0;
                    }
                    else if (diff.z > diff.x)
                    {
                        diff.x = 0;
                    }

                    diff.y = 0;
                    box.Kick(diff, 15f, m_Box.LastTouchActor);
                }
            }
        }
    }

    public static void ExplodePullBox(this Box m_Box, Vector3 center, float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(center, radius, LayerManager.Instance.LayerMask_Box);
        foreach (Collider collider in colliders)
        {
            Box box = collider.gameObject.GetComponentInParent<Box>();
            if (box != null && box.Interactable && box != m_Box)
            {
                if (box.State == Box.States.Static || box.State == Box.States.BeingKicked || box.State == Box.States.BeingPushed || box.State == Box.States.PushingCanceling)
                {
                    Vector3 diff = center - box.transform.position;
                    box.Kick(diff, 30f, m_Box.LastTouchActor);
                }
            }
        }
    }
}