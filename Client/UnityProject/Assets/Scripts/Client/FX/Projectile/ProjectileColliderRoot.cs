using UnityEngine;
using Sirenix.OdinInspector;

public class ProjectileColliderRoot : MonoBehaviour
{
    [LabelText("碰撞半径")]
    [OnValueChanged("OnRadiusChanged")]
    public float Radius;

    [SerializeField]
    private SphereCollider PlayerCollider;

    [SerializeField]
    private SphereCollider EnemyCollider;

    [SerializeField]
    private SphereCollider SurroundingCollider;

    private void OnRadiusChanged()
    {
        PlayerCollider.radius = Radius;
        EnemyCollider.radius = Radius;
        SurroundingCollider.radius = Radius;
    }

    public void Init()
    {
        PlayerCollider.radius = Radius;
        EnemyCollider.radius = Radius;
        SurroundingCollider.radius = Radius;

        PlayerCollider.enabled = true;
        EnemyCollider.enabled = true;
        SurroundingCollider.enabled = true;
        SurroundingCollider.isTrigger = false;
    }

    public void OnRecycled()
    {
        PlayerCollider.enabled = false;
        EnemyCollider.enabled = false;
        SurroundingCollider.enabled = false;
    }
}