using System;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UntitledSpiderGame.Runtime.Player;
using UntitledSpiderGame.Runtime.Utility;
using Object = UnityEngine.Object;

namespace UntitledSpiderGame.Runtime.Extras
{
    public class Projectile : MonoBehaviour
    {
        public const int ProjectileLayerMask = ~(1 << 8);
        
        public DamageArgs damage;

        public Vector2 velocity;
        public float size;
        public int bounces;
        public float lifetime;
        private Vector2 force;
        private Transform hitFX;
        private ParticleSystem trail;

        public float age;
        private int ignoreShooterFrames = 3;

        public GameObject shooter;

        public static event Action<Projectile, RaycastHit2D> HitEvent;

        public Projectile Spawn(GameObject shooter, Vector2 position, Vector2 direction, float muzzleSpeed, DamageArgs damage, float lifetime, int bounces, float size)
        {
            if (size < 0.1f) return null;

            var instance = Instantiate(this);
            instance.shooter = shooter;
            instance.damage = damage;
            instance.velocity = direction.normalized * muzzleSpeed;
            instance.transform.position = position;
            instance.transform.right = direction;
            instance.lifetime = lifetime;
            instance.bounces = bounces;
            instance.size = size;
            return instance;
        }

        private void Awake()
        {
            hitFX = transform.Find("HitFX");
            if (hitFX) hitFX.gameObject.SetActive(false);
            
            trail = transform.Find<ParticleSystem>("Trail");
        }

        protected virtual void FixedUpdate()
        {
            transform.localScale = Vector3.one * Mathf.Sqrt(size);
            
            if (age > lifetime)
            {
                Destroy(gameObject);
                return;
            }

            var stepDistance = velocity.magnitude * Time.deltaTime * 1.05f;
            var hit = Physics2D.Raycast(transform.position, velocity, stepDistance, ProjectileLayerMask);
            if (ValidateHit(hit))
            {
                ProcessHit(hit);
            }

            Integrate();
            transform.right = velocity;
            ignoreShooterFrames--;
        }

        protected virtual bool ValidateHit(RaycastHit2D hit)
        {
            if (!hit) return false;
            if (ignoreShooterFrames > 0 && hit.collider.transform.IsChildOf(shooter.transform)) return false;

            return true;
        }

        protected virtual void ProcessHit(RaycastHit2D hit)
        {
            HitEvent?.Invoke(this, hit);
            
            transform.position = hit.point;
            DamageHit(hit);

            if (bounces > 0)
            {
                var reflect = Vector2.Reflect(velocity, hit.normal);
                Spawn(shooter, hit.point, reflect, velocity.magnitude, damage, lifetime - age, bounces - 1, size);
            }
            
            DestroyWithStyle();
        }

        protected void DamageHit(RaycastHit2D hit)
        {
            var damageable = hit.collider.GetComponentInParent<IDamagable>();
            if ((Object)damageable)
            {
                var damage = this.damage;
                damage.damage *= size;
                damage.knockback *= size;
                
                damageable.Damage(damage, shooter, hit.point, velocity.normalized);
            }
        }

        protected void DestroyWithStyle()
        {
            if (hitFX)
            {
                hitFX.gameObject.SetActive(true);
                hitFX.SetParent(null);
            }

            if (trail)
            {
                trail.transform.SetParent(null);
                trail.Stop();
            }

            Destroy(gameObject);
        }

        private void Integrate()
        {
            transform.position = (Vector2)transform.position + velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;
            force = Physics2D.gravity;

            age += Time.deltaTime;
        }
    }
}