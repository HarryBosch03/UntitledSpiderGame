using UnityEngine;
using UntitledSpiderGame.Runtime.Extras;

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
            var leg = spider.ArmLeg;
            var reach = spider.ReachVector;

            if (spider.Reaching)
            {
                var position = spider.Body.position + Vector2.ClampMagnitude(reach, leg.lengthTotal);
                position -= reach.normalized * Mathf.Pow(2.0f, -(Time.time - lastFireTime) * Stats.recoilDecay) * Stats.recoilResponse;

                leg.OverridePosition = position;
                leg.OverrideDirection = reach;
            }
            else
            {
                leg.OverridePosition = null;
                leg.OverrideDirection = null;
            }
        }

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
            if (Shoot && Time.time - lastFireTime > 1.0f / Stats.attackSpeed && ammo > 0 && spider.Reaching)
            {
                var damage = new DamageArgs()
                {
                    damage = Stats.damage,
                    knockback = Stats.knockback,
                };

                var shots = Mathf.Max(1, Stats.projectilesPerShot);
                for (var i = 0; i < shots; i++)
                {
                    var normal = spider.ReachVector.normalized;
                    var tangent = new Vector2(-normal.y, normal.x);
                    var spread = Random.Range(-1.0f, 1.0f) * Mathf.Max(0.0f, Stats.spreadTangent) * 0.5f;
                    var direction = (normal + tangent * spread).normalized;

                    var speedVariance = Mathf.Pow(2.0f, Stats.spreadTangent);
                    var speed = Stats.bulletSpeed * Random.Range(1.0f / speedVariance, speedVariance);
                    prefab.Spawn(gameObject, prefabSpawnPoint.position, direction, speed, damage, Stats.bulletLifetime, Stats.bounces, Stats.bulletSize);
                }

                spider.Body.AddForce(-spider.ReachVector.normalized * Stats.recoilForce, ForceMode2D.Impulse);

                ammo--;
                lastFireTime = Time.time;
            }
        }
    }
}