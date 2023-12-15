using System;
using Crabs.Player;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Crabs.Extras
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float maxDistance = 200.0f;
        
        public int damage;

        private Vector2 velocity;
        private Vector2 force;
        private GameObject hitFX;

        private float distanceTraveled;
        private int ignoreShooterFrames = 3;

        public GameObject Shooter;

        public Projectile Spawn(GameObject shooter, Transform muzzle, float muzzleSpeed, int damage)
        {
            var instance = Instantiate(this);
            instance.Shooter = shooter;
            instance.damage = damage;
            instance.velocity = muzzle.right.normalized * muzzleSpeed;
            instance.transform.position = muzzle.position;
            return instance;
        }

        private void Awake()
        {
            hitFX = transform.Find("HitFX").gameObject;
            hitFX.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (distanceTraveled > maxDistance)
            {
                Destroy(gameObject);
                return;
            }
            
            var stepDistance = velocity.magnitude * Time.deltaTime * 1.05f;
            var hit = Physics2D.Raycast(transform.position, velocity, stepDistance);
            if (ValidateHit(hit))
            {
                transform.position = hit.point;

                var damageable = hit.collider.GetComponentInParent<IDamagable>();
                if ((Object)damageable)
                {
                    damageable.Damage(damage, hit.point, velocity.normalized);
                }
                
                hitFX.SetActive(true);
                hitFX.transform.SetParent(null);
                Destroy(gameObject);
            }
            
            Integrate();
            transform.right = velocity;
            ignoreShooterFrames--;
        }

        private bool ValidateHit(RaycastHit2D hit)
        {
            if (!hit) return false;
            if (ignoreShooterFrames > 0 && hit.collider.transform.IsChildOf(Shooter.transform)) return false;
            
            return true;
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