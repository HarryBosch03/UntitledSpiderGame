using Crabs.Extras;
using Crabs.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Crabs.Items
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class Gun : Item
    {
        
        [SerializeField] private Projectile projectile;
        [SerializeField] private int damage = 60;
        [SerializeField] private float muzzleSpeed = 200;
        [SerializeField] private int ammo = 4;
        [SerializeField] private float fireDelay = 0.2f;
        [SerializeField][Range(0.0f, 1.0f)] private float armBrace = 0.9f;
        [SerializeField] private float recoilImpulse = 40.0f;
        [SerializeField] private float recoilSpring = 300.0f;
        [SerializeField] private float recoilDamper = 25.0f;
        [SerializeField] private float shootForce = 10.0f;
        [SerializeField] private Transform muzzle;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private ParticleSystem deadFlash;

        private float lastFireTime;

        private Vector2 recoilPosition;
        private Vector2 recoilVelocity;
        private Vector2 recoilForce;
        
        public static bool InfiniteAmmo = true;

        protected override void FixedUpdate()
        {
            recoilForce += -recoilPosition * recoilSpring - recoilVelocity * recoilDamper;

            recoilPosition += recoilVelocity * Time.deltaTime;
            recoilVelocity += recoilForce * Time.deltaTime;
            recoilForce = Vector2.zero;
            
            base.FixedUpdate();
        }

        public override void PrimaryUse()
        {
            if (Time.time - lastFireTime < fireDelay) return;
            lastFireTime = Time.time;

            if (ammo == 0 && !InfiniteAmmo)
            {
                Instantiate(deadFlash, muzzle.position, muzzle.rotation);
                return;
            }

            projectile.Spawn(transform, muzzleSpeed, damage);

            var recoilDirection = (Binding.mid - Binding.tip).normalized;
            recoilForce += recoilDirection * recoilImpulse / Time.fixedDeltaTime;
            ammo--;
            
            Binding.Spider.Body.AddForce(recoilDirection * shootForce, ForceMode2D.Impulse);

            Instantiate(muzzleFlash, muzzle.position, muzzle.rotation);
        }

        public override void SecondaryUse()
        {
            
        }

        public override Vector2? ModifyReachPosition(Vector2 reachPosition) => (reachPosition - Binding.root) * armBrace + Binding.root + recoilPosition;
        public override float? ModifyRotation(float rotation) => (Binding.tip - Binding.root).ToAngle();
    }
}
