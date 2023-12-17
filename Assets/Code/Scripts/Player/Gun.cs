using UnityEngine;
using UntitledSpiderGame.Runtime.Extras;

namespace UntitledSpiderGame.Runtime.Player
{
    [RequireComponent(typeof(SpiderController))]
    public class Gun : MonoBehaviour
    {
        public Projectile prefab;
        public Transform prefabSpawnPoint;
        public DamageArgs damage;
        public float muzzleSpeed;
        public float fireDelay;
        public int maxAmmo;
        public float reloadTime;

        [Space]
        public float recoilResponse;
        public float recoilDecay;
        public float recoilForce;

        [HideInInspector] public int ammo;

        private SpiderController controller;
        private float lastFireTime;

        public bool Shoot { get; set; }

        private void Awake() { controller = GetComponent<SpiderController>(); }

        private void OnEnable() { ammo = maxAmmo; }

        private void FixedUpdate()
        {
            TryShoot();
            Reload();
            UpdateLeg();

            ResetFlags();
        }

        private void UpdateLeg()
        {
            var leg = controller.ArmLeg;
            var reach = controller.ReachVector;

            if (controller.Reaching)
            {
                var position = controller.Body.position + Vector2.ClampMagnitude(reach, leg.lengthTotal);
                position -= reach.normalized * Mathf.Pow(2.0f, -(Time.time - lastFireTime) * recoilDecay) * recoilResponse;

                leg.OverridePosition = position;
                leg.OverrideDirection = reach;
            }
            else
            {
                leg.OverridePosition = null;
                leg.OverrideDirection = null;
            }
        }

        private void ResetFlags() { Shoot = false; }

        private void Reload()
        {
            if (Time.time - lastFireTime > reloadTime)
            {
                ammo = maxAmmo;
            }
        }

        private void TryShoot()
        {
            if (Shoot && Time.time - lastFireTime > fireDelay && ammo > 0 && controller.Reaching)
            {
                prefab.Spawn(gameObject, prefabSpawnPoint.position, controller.ReachVector.normalized, muzzleSpeed, damage);
                
                controller.Body.AddForce(-controller.ReachVector.normalized * recoilForce, ForceMode2D.Impulse);
                
                ammo--;
                lastFireTime = Time.time;
            }
        }
    }
}