using System;
using UnityEngine;

namespace Crabs.Player
{
    public interface IDamagable
    {
        event Action HealthChangedEvent;
        
        int CurrentHealth { get; }
        int MaxHealth { get; }
        
        void Damage(DamageArgs damage, Vector2 point, Vector2 direction);
    }
}