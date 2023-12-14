using Crabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Crabs.Player
{
    public class HealthBar : MonoBehaviour
    {
        private Image fill;
        
        private IDamagable target;

        private void Awake()
        {
            target = GetComponentInParent<IDamagable>();
            fill = transform.Find<Image>("Fill");
        }

        private void OnEnable()
        {
            if (target == null)
            {
                enabled = false;
                return;
            }
            
            target.HealthChangedEvent += OnHealthChanged;
            OnHealthChanged();
        }

        private void OnDisable()
        {
            target.HealthChangedEvent -= OnHealthChanged;
        }

        private void OnHealthChanged()
        {
            fill.fillAmount = target.CurrentHealth / (float)target.MaxHealth;
        }
    }
}
