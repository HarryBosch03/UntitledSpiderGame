using UnityEngine;
using UntitledSpiderGame.Runtime.Extras;
using UntitledSpiderGame.Runtime.Utility;

namespace UntitledSpiderGame.Runtime.Spider
{
    [RequireComponent(typeof(SpiderController))]
    public class SpiderWeapon : SpiderModule
    {
        public int ammo;

        public Projectile prefab;
        public Transform prefabSpawnPoint;

        private SpiderController spider;
        private float lastFireTime;

        public bool Shoot { get; set; }

        protected override void Awake()
        {
            base.Awake();
            spider = GetComponent<SpiderController>();
        }

        private void OnEnable() { ammo = Stats.ammo; }

        private void FixedUpdate()
        {
            TryShoot();
            Reload();
            UpdateLeg();

            ResetFlags();
        }

        private void UpdateLeg()
        {
            if (spider.Reaching) OverrideSpiderArmPosition();
            else ClearSpiderArmOverride();
        }

        private void ClearSpiderArmOverride()
        {
            spider.ArmLeg.OverridePosition = null;
            spider.ArmLeg.OverrideDirection = null;
        }

        private void OverrideSpiderArmPosition()
        {
            var reach = spider.ReachVector;
            var leg = spider.ArmLeg;

            var position = spider.Body.position + Vector2.ClampMagnitude(reach, leg.lengthTotal);
            position -= reach.normalized * CalculateRecoil();

            leg.OverridePosition = position;
            leg.OverrideDirection = reach;
        }

        private float CalculateRecoil() => Mathf.Pow(2.0f, -(Time.time - lastFireTime) * Stats.recoilDecay) * Stats.recoilResponse;

        private void ResetFlags()
        {
            if (Stats.automatic <= 0) Shoot = false;
        }

        private void Reload()
        {
            if (Time.time - lastFireTime > Stats.reloadTime)
            {
                ammo = Mathf.Max(Stats.ammo, 1);
            }
        }

        private void TryShoot()
        {
            if (!Shoot) return;
            if (Time.time - lastFireTime < 1.0f / Stats.attackSpeed) return;
            if (ammo == 0) return;
            if (!spider.Reaching) return;

            var shots = Mathf.Max(1, Stats.projectilesPerShot);
            for (var i = 0; i < shots; i++)
            {
                var direction = CalculateBulletDirection();
                var speed = CalculateBulletSpeed();
                SpawnProjectile(direction, speed);
            }

            AddRecoilForce();

            ammo--;
            lastFireTime = Time.time;
        }

        private void AddRecoilForce() { spider.Body.AddForce(-spider.ReachVector.normalized * Stats.recoilForce, ForceMode2D.Impulse); }

        private float CalculateBulletSpeed()
        {
            var speedVariance = Mathf.Pow(2.0f, Stats.spreadTangent);
            var speed = Stats.bulletSpeed * Random.Range(1.0f / speedVariance, speedVariance);
            return speed;
        }

        private Vector2 CalculateBulletDirection()
        {
            var normal = spider.ReachVector.normalized;
            var tangent = normal.Tangent();
            var spread = Random.Range(-1.0f, 1.0f) * Mathf.Max(0.0f, Stats.spreadTangent) * 0.5f;
            var direction = (normal + tangent * spread).normalized;
            return direction;
        }

        private void SpawnProjectile(Vector2 direction, float speed)
        {
            var damage = new DamageArgs()
            {
                damage = Stats.damage,
                knockback = Stats.knockback,
            };
            
            prefab.Spawn
            (
                gameObject,
                prefabSpawnPoint.position,
                direction,
                new Projectile.ProjectileSpawnArgs
                {
                    speed = speed,
                    damage = damage,
                    lifetime = Stats.bulletLifetime,
                    bounces = Stats.bounces,
                    size = Stats.bulletSize,
                    fractures = Stats.fractures,
                    gravity = Stats.gravity,
                    pierce = Stats.pierce,
                }
            );
        }
    }
}