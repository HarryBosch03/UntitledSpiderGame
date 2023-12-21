using System;
using UnityEngine;
using UntitledSpiderGame.Runtime.Extras;
using UntitledSpiderGame.Runtime.Player;
using UntitledSpiderGame.Runtime.Spider;

namespace UntitledSpiderGame.Runtime.Mods.Weapons
{
    public class Shrapnel : Mod
    {
        private void Awake()
        {
            Projectile.HitEvent += OnProjectileHit;
        }

        private void OnDestroy()
        {
            Projectile.HitEvent -= OnProjectileHit;
        }

        protected override void ModifyStats(ref SpiderStatSheet stats)
        {
            stats.bounces += 2;
        }

        private void OnProjectileHit(Projectile projectile, RaycastHit2D hit)
        {
            if (projectile.shooter != spider.gameObject) return;
            if (projectile.bounces <= 0) return;

            var bounces = projectile.bounces;
            projectile.bounces = 0;

            var velocity = projectile.velocity;
            var normal = hit.normal;

            velocity = Vector2.Reflect(velocity, normal);
            var v1 = Quaternion.Euler(0.0f, 0.0f, -45.0f) * velocity;
            var v2 = Quaternion.Euler(0.0f, 0.0f, 45.0f) * velocity;

            projectile.Spawn(projectile.shooter, hit.point, v1, v1.magnitude, projectile.damage, projectile.lifetime - projectile.age, bounces - 1, projectile.size * 0.5f);
            projectile.Spawn(projectile.shooter, hit.point, v2, v2.magnitude, projectile.damage, projectile.lifetime - projectile.age, bounces - 1, projectile.size * 0.5f);
        }
    }
}