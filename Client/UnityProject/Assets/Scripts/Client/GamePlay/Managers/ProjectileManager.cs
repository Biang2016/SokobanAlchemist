using BiangStudio.Singleton;
using UnityEngine;

public class ProjectileManager : TSingletonBaseManager<ProjectileManager>
{
    public Transform Root;

    public void Init(Transform root)
    {
        Root = root;
    }

    public Projectile ShootProjectile(Vector3 from, Vector3 dir, Transform dummyPos, ProjectileType projectileType, float velocity, float projectileScale)
    {
        Projectile projectile = GameObjectPoolManager.Instance.ProjectileDict[projectileType].AllocateGameObject<Projectile>(Root);
        projectile.transform.position = from;
        projectile.transform.LookAt(from + dir);
        projectile.Initialize(velocity, dummyPos.forward, projectileScale);
        projectile.Launch(dummyPos);
        return projectile;
    }
}