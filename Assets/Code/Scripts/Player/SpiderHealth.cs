using System;
using UnityEngine;

namespace Crabs.Player
{
    public class SpiderHealth : MonoBehaviour, IDamagable
    {
        public int currentHealth;
        public int maxHealth;

        private void OnEnable()
        {
            currentHealth = maxHealth;
        }

        public event Action HealthChangedEvent;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public void Damage(int damage, Vector2 point, Vector2 direction)
        {
            currentHealth -= damage;

            HealthChangedEvent?.Invoke();
            
            if (currentHealth <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
