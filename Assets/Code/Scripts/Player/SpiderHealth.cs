using System;
using Crabs.Utility;
using UnityEngine;

namespace Crabs.Player
{
    public class SpiderHealth : MonoBehaviour, IDamagable
    {
        public int currentHealth;
        public int maxHealth;

        private Rigidbody2D body;

        private ParticleSystem hitEffect;
        private ParticleSystem deathEffect;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            
            deathEffect = transform.Find<ParticleSystem>("DeathFX");
            hitEffect = transform.Find<ParticleSystem>("HitFX");
        }

        private void OnEnable()
        {
            currentHealth = maxHealth;
            deathEffect.gameObject.SetActive(false);
        }

        public event Action HealthChangedEvent;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public void Damage(DamageArgs damage, Vector2 point, Vector2 direction)
        {
            if (damage.damage <= 0) return;
            
            currentHealth -= damage.damage;
            body.AddForce(direction.normalized * damage.knockback, ForceMode2D.Impulse);

            HealthChangedEvent?.Invoke();
            
            if (currentHealth <= 0)
            {
                deathEffect.gameObject.SetActive(true);
                deathEffect.transform.SetParent(null);
                
                gameObject.SetActive(false);
                CameraController.Shake(1.0f);
            }
            else
            {
                hitEffect.Play();
                CameraController.Shake(0.2f);
            }
        }
    }
}
