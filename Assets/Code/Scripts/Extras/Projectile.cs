using System;
using System.Linq;
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
        private TrailRenderer trail;

        public ProjectileSpawnArgs args;

        public float age;
        private int ignoreShooterFrames = 3;

        public static event Action<Projectile, RaycastHit2D> HitEvent;

        public Projectile Spawn(GameObject shooter, Vector2 position, Vector2 direction, ProjectileSpawnArgs args)
        {
            if (args.size < 0.1f) return null;

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

            trail = transform.Find<TrailRenderer>("Trail");
        }

        private void Start() { velocity = transform.right * args.speed; }

        protected virtual void FixedUpdate()
        {
            transform.localScale = Vector3.one * Mathf.Sqrt(args.size);

            if (age > args.lifetime)
            {
                Destroy(gameObject);
                return;
            }

            var stepDistance = velocity.magnitude * Time.deltaTime * 1.15f;
            var hits = Physics2D.RaycastAll(transform.position, velocity, stepDistance, ProjectileLayerMask);
            foreach (var hit in hits.OrderBy(e => e.distance))
            {
                if (!ValidateHit(hit)) continue;

                ProcessHit(hit);
                break;
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

            if (args.bounces > 0)
            {
                var args = this.args;
                args.bounces--;
                args.fractures--;
                args.speed = velocity.magnitude;
                
                for (var i = 0; i < (this.args.fractures > 0 ? 2 : 1); i++)
                {
                    var angle = this.args.fractures > 0 ? (i / (float)this.args.fractures * 2.0f - 1.0f) * 10.0f : 0.0f;
                    
                    var direction = Vector2.Reflect(velocity, hit.normal);
                    direction = (direction.ToAngle() + angle * Mathf.Deg2Rad).ToDirection();
                    var point = hit.point + direction * 0.05f;
                    var p = Spawn(shooter, point, direction, args);
                    p.ignoreShooterFrames = ignoreShooterFrames;
                }
            }

            DestroyWithStyle();
        }

        protected void DamageHit(RaycastHit2D hit)
        {
            var damageable = hit.collider.GetComponentInParent<IDamagable>();
            if ((Object)damageable)
            {
                var damage = args.damage;
                damage.damage *= args.size;
                damage.knockback *= args.size;

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
                trail.transform.position = transform.position;
                Destroy(trail.gameObject, trail.time);
            }

            Destroy(gameObject);
        }

        private void Integrate()
        {
            transform.position = (Vector2)transform.position + velocity * Time.deltaTime;
            velocity += force * Time.deltaTime;
            force = Physics2D.gravity * args.gravity;

            age += Time.deltaTime;
        }

        [Serializable]
        public struct ProjectileSpawnArgs
        {
            public DamageArgs damage;
            public int bounces;
            public int fractures;
            public bool pierce;
            public float size;
            public float speed;
            public float gravity;
            public float lifetime;
        }
    }
}