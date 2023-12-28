using UnityEngine;
using UntitledSpiderGame.Runtime.Extras;

namespace UntitledSpiderGame.Runtime.Mods.Weapons
{
    public class BombBullets : Mod
    {
        private GameObject bomb;

        private void Awake()
        {
            bomb = Resources.Load<GameObject>("ProjectileFX/Bomb");
            Projectile.HitEvent += OnProjectileHit;
        }

        private void OnDestroy()
        {
            Projectile.HitEvent -= OnProjectileHit;
        }

        private void OnProjectileHit(Projectile projectile, RaycastHit2D hit)
        {
            if (!spider) return;
            if (projectile.shooter != spider.gameObject) return;

            Instantiate(bomb, hit.point, Quaternion.identity, hit.collider.transform);
        }
    }
}