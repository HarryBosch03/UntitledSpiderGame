using System;
using Crabs.Extras;
using UnityEngine;

namespace Crabs.Player
{
    [RequireComponent(typeof(SpiderController))]
    public class Gun : MonoBehaviour
    {
        public Projectile prefab;
        public Transform prefabSpawnPoint;
        public int damage;
        public float muzzleSpeed;
        public float fireDelay;
        public int maxAmmo;
        public float reloadTime;

        [Space]
        public float recoilResponse;
        public float recoilDecay;
        
        [HideInInspector] public int ammo;
        
        private SpiderController controller;
        private float lastFireTime;

        public bool Shoot { get; set; }
        
        private void Awake()
        {
            controller = GetComponent<SpiderController>();
        }

        private void OnEnable()
        {
            ammo = maxAmmo;
        }

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

            var position = controller.Body.position + Vector2.ClampMagnitude(reach, leg.lengthTotal);
            position -= reach.normalized * Mathf.Pow(2.0f, -(Time.time - lastFireTime) * recoilDecay) * recoilResponse; 
            
            leg.OverridePosition = position;
            leg.OverrideDirection = reach;
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
            if (Shoot && Time.time - lastFireTime > fireDelay && ammo > 0)
            {
                prefab.Spawn(gameObject, prefabSpawnPoint.position, controller.ReachVector.normalized, muzzleSpeed, damage);
                ammo--;
                lastFireTime = Time.time;
            }
        }
    }
}