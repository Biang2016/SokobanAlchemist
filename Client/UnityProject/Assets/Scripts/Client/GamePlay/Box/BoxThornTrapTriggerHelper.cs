using UnityEngine;
using System.Collections.Generic;

public class BoxThornTrapTriggerHelper : MonoBehaviour, IBoxHelper
{
    public Box Box;

    public void PoolRecycle()
    {
        actorStayTimeDict.Clear();
    }

    private Dictionary<uint, float> actorStayTimeDict = new Dictionary<uint, float>();

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Actor actor = collider.GetComponentInParent<Actor>();
            if (actor != null)
            {
                foreach (BoxFunctionBase bf in Box.BoxFunctions)
                {
                    switch (bf)
                    {
                        case BoxFunction_ThornDamage skill:
                        {
                            if (!actorStayTimeDict.ContainsKey(actor.GUID))
                            {
                                actor.ActorBattleHelper.Damage(null, skill.Damage);
                                actorStayTimeDict.Add(actor.GUID, 0);
                            }

                            break;
                        }
                    }
                }
            }
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Actor actor = collider.GetComponentInParent<Actor>();
            if (actor != null)
            {
                foreach (BoxFunctionBase bf in Box.BoxFunctions)
                {
                    switch (bf)
                    {
                        case BoxFunction_ThornDamage skill:
                        {
                            if (actorStayTimeDict.TryGetValue(actor.GUID, out float duration))
                            {
                                if (duration > skill.DamageInterval)
                                {
                                    actor.ActorBattleHelper.Damage(null, skill.Damage);
                                    actorStayTimeDict[actor.GUID] = 0;
                                }
                                else
                                {
                                    actorStayTimeDict[actor.GUID] += Time.fixedDeltaTime;
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Player || collider.gameObject.layer == LayerManager.Instance.Layer_HitBox_Enemy)
        {
            Actor actor = collider.GetComponentInParent<Actor>();
            if (actor != null)
            {
                foreach (BoxFunctionBase bf in Box.BoxFunctions)
                {
                    switch (bf)
                    {
                        case BoxFunction_ThornDamage skill:
                        {
                            if (actorStayTimeDict.ContainsKey(actor.GUID))
                            {
                                actorStayTimeDict.Remove(actor.GUID);
                            }

                            break;
                        }
                    }
                }
            }
        }
    }
}