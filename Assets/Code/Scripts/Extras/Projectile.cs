using Crabs.Player;
using Crabs.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Crabs.Extras
{
    public class Projectile : MonoBehaviour
    {
        public const int ProjectileLayerMask = ~(1 << 8);
        
        [SerializeField] private float maxDistance = 200.0f;

        public int damage;

        public Vector2 velocity;
        private Vector2 force;
        private Transform hitFX;
        private ParticleSystem trail;

        private float distanceTraveled;
        private int ignoreShooterFrames = 3;

        private GameObject Shooter;

        public Projectile Spawn(GameObject shooter, Vector2 position, Vector2 direction, float muzzleSpeed, int damage)
        {
            var instance = Instantiate(this);
            instance.Shooter = shooter;
            instance.damage = damage;
            instance.velocity = direction.normalized * muzzleSpeed;
            instance.transform.position = position;
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
            if (distanceTraveled > maxDistance)
            {
                Destroy();
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
            if (ignoreShooterFrames > 0 && hit.collider.transform.IsChildOf(Shooter.transform)) return false;

            return true;
        }

        protected virtual void ProcessHit(RaycastHit2D hit)
        {
            transform.position = hit.point;
            DamageHit(hit);
            Destroy();
        }

        protected void DamageHit(RaycastHit2D hit)
        {
            var damageable = hit.collider.GetComponentInParent<IDamagable>();
            if ((Object)damageable)
            {
                damageable.Damage(damage, hit.point, velocity.normalized);
            }
        }

        protected void Destroy()
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

            distanceTraveled += velocity.magnitude * Time.deltaTime;
        }
    }
}