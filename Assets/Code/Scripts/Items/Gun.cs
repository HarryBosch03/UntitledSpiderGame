using UnityEngine;
using UntitledSpiderGame.Runtime.Extras;
using UntitledSpiderGame.Runtime.Utility;

namespace UntitledSpiderGame.Runtime.Items
{
    public class Gun : Item
    {
        public Projectile prefab;
        public Transform muzzle;
        public DamageArgs damage;
        public float bulletLifetime;
        public int maxAmmo;
        public int projectilesPerShot;
        public float spreadTangent;
        public float bulletSpeed;
        public float attackSpeed;
        public bool automatic;
        public float recoilResponse;
        public float recoilDecay;
        public float recoilForce;

        private float lastFireTime;
        private int ammo;

        protected override void Awake()
        {
            base.Awake();
            ammo = maxAmmo;
        }

        protected override void Use()
        {
            if (Time.time - lastFireTime < 1.0f / attackSpeed) return;
            if (ammo == 0) return;

            var shots = Mathf.Max(1, projectilesPerShot);
            for (var i = 0; i < shots; i++)
            {
                var direction = CalculateBulletDirection();
                var speed = CalculateBulletSpeed();
                SpawnProjectile(direction, speed);
            }

            AddRecoilForce();

            if (!automatic) Input = false;

            ammo--;
            lastFireTime = Time.time;
        }
        
        private void AddRecoilForce() { bindingBody.AddForce(-transform.right * recoilForce, ForceMode2D.Impulse); }

        private float CalculateBulletSpeed()
        {
            var speedVariance = Mathf.Pow(2.0f, spreadTangent);
            var speed = bulletSpeed * Random.Range(1.0f / speedVariance, speedVariance);
            return speed;
        }

        private Vector2 CalculateBulletDirection()
        {
            var normal = (Vector2)transform.right;
            var tangent = normal.Tangent();
            var spread = Random.Range(-1.0f, 1.0f) * Mathf.Max(0.0f, spreadTangent) * 0.5f;
            var direction = (normal + tangent * spread).normalized;
            return direction;
        }

        private void SpawnProjectile(Vector2 direction, float speed)
        {
            prefab.Spawn
            (
                gameObject,
                muzzle.position,
                direction,
                new Projectile.ProjectileSpawnArgs
                {
                    speed = speed,
                    damage = damage,
                    lifetime = bulletLifetime,
                }
            );
        }
    }
}