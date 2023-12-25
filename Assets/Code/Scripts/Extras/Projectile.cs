using System;
using UnityEngine;
using UntitledSpiderGame.Runtime.Player;
using UntitledSpiderGame.Runtime.Utility;
using Object = UnityEngine.Object;

namespace UntitledSpiderGame.Runtime.Extras
{
    public class Projectile : MonoBehaviour
    {
        public const int ProjectileLayerMask = ~(1 << 8);

        public Vector2 velocity;
        private Vector2 force;

        public GameObject shooter;
        private Transform hitFX;
        private ParticleSystem trail;

        public ProjectileSpawnArgs args;

        public float age;
        private int ignoreShooterFrames = 3;

        public static event Action<Projectile, RaycastHit2D> HitEvent;

        public Projectile Spawn(GameObject shooter, Vector2 position, Vector2 direction, ProjectileSpawnArgs args)
        {
            var instance = Instantiate(this);
            instance.shooter = shooter;
            instance.transform.position = position;
            instance.transform.right = direction;

            instance.args = args;
            return instance;
        }

        private void Awake()
        {
            hitFX = transform.Find("HitFX");
            if (hitFX) hitFX.gameObject.SetActive(false);

            trail = transform.Find<ParticleSystem>("Trail");
        }

        private void Start()
        {
            velocity = transform.right * args.speed;
        }

        protected virtual void FixedUpdate()
        {
            if (age > args.lifetime)
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

            DestroyWithStyle();
        }

        protected void DamageHit(RaycastHit2D hit)
        {
            var damageable = hit.collider.GetComponentInParent<IDamagable>();
            if ((Object)damageable)
            {
                damageable.Damage(args.damage, shooter, hit.point, velocity.normalized);
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

        public struct ProjectileSpawnArgs
        {
            public DamageArgs damage;
            public float speed;
            public float lifetime;
        }
    }
}