using System.Collections.Generic;
using BiangStudio.Singleton;
using UnityEngine;

public class ProjectileManager : TSingletonBaseManager<ProjectileManager>
{
    public Transform Root;

    public void Init(Transform root)
    {
        Root = root;
    }

    public void EmitProjectile(Transform shooterDummy, ProjectileType projectileType, PlayerNumber playerNumber, float velocity, float projectileScale)
    {
        ShootProjectile(shooterDummy.position, shooterDummy.forward, shooterDummy, projectileType, playerNumber, velocity, projectileScale);
    }

    public Projectile ShootProjectile(Vector3 from, Vector3 dir, Transform dummyPos, ProjectileType projectileType, PlayerNumber playerNumber, float velocity, float projectileScale)
    {
        Projectile projectile = GameObjectPoolManager.Instance.ProjectileDict[projectileType].AllocateGameObject<Projectile>(Root);
        projectile.transform.position = from;
        projectile.transform.LookAt(from + dir);
        projectile.Initialize(velocity, dummyPos.forward, projectileType, playerNumber, projectileScale);
        projectile.Launch(dummyPos);
        return projectile;
    }
}