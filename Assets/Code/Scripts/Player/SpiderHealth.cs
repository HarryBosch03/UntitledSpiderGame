using System;
using UnityEngine;
using UntitledSpiderGame.Runtime.Utility;

namespace UntitledSpiderGame.Runtime.Player
{
    public class SpiderHealth : MonoBehaviour, IDamagable
    {
        public int currentHealth;
        public int maxHealth;

        private Rigidbody2D body;

        private ParticleSystem hitEffect;
        private ParticleSystem deathEffect;

        public GameObject LastDamageInvoker { get; private set; }
        public static event DamageEvent DamagedEvent;
        public static event DamageEvent DiedEvent;

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

        public void Damage(DamageArgs args, GameObject invoker, Vector2 point, Vector2 direction)
        {
            currentHealth -= args.damage;
            body.AddForce(direction.normalized * args.knockback, ForceMode2D.Impulse);

            if (invoker) LastDamageInvoker = invoker;
            
            HealthChangedEvent?.Invoke();
            DamagedEvent?.Invoke(this, LastDamageInvoker, args);
            
            if (currentHealth <= 0)
            {
                deathEffect.gameObject.SetActive(true);
                deathEffect.transform.SetParent(null);
                
                gameObject.SetActive(false);
                CameraController.Shake(1.0f);
                
                DiedEvent?.Invoke(this, LastDamageInvoker, args);
            }
            else
            {
                hitEffect.Play();
                CameraController.Shake(0.2f);
            }
        }

        public delegate void DamageEvent(SpiderHealth spider, GameObject invoker, DamageArgs args);
    }
}
