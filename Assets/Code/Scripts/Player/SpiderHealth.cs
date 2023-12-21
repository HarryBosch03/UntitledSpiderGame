using System;
using UnityEngine;
using UntitledSpiderGame.Runtime.Spider;
using UntitledSpiderGame.Runtime.Utility;

namespace UntitledSpiderGame.Runtime.Player
{
    public class SpiderHealth : SpiderModule, IDamagable
    {
        [Range(0.0f, 1.0f)]
        public float healthPercent;
        public int maxHealth;

        private ParticleSystem hitEffect;
        private ParticleSystem deathEffect;

        public GameObject LastDamageInvoker { get; private set; }
        public static event DamageEvent DamagedEvent;
        public static event DamageEvent DiedEvent;

        protected override void Awake()
        {
            base.Awake();
            
            deathEffect = transform.Find<ParticleSystem>("DeathFX");
            hitEffect = transform.Find<ParticleSystem>("HitFX");
        }

        private void OnEnable()
        {
            healthPercent = 1.0f;
            deathEffect.gameObject.SetActive(false);
        }

        public event Action HealthChangedEvent;
        public int CurrentHealth
        {
            get => Mathf.RoundToInt(healthPercent * MaxHealth);
            set => healthPercent = (float)value / MaxHealth;
        }

        public int MaxHealth => maxHealth;

        private void Update()
        {
            if (maxHealth != Stats.health)
            {
                maxHealth = Stats.health;
                HealthChangedEvent?.Invoke();
            }
        }

        public void Damage(DamageArgs args, GameObject invoker, Vector2 point, Vector2 direction)
        {
            CurrentHealth -= IDamagable.Round(args.damage);
            Spider.Body.AddForce(direction.normalized * args.knockback, ForceMode2D.Impulse);

            if (invoker) LastDamageInvoker = invoker;
            
            HealthChangedEvent?.Invoke();
            DamagedEvent?.Invoke(this, LastDamageInvoker, args);
            
            if (CurrentHealth <= 0)
            {
                if (deathEffect)
                {
                    var fx = Instantiate(deathEffect, transform.position, transform.rotation, null);
                    fx.gameObject.SetActive(true);
                }
                
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
