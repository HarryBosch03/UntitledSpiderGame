using System;
using UnityEngine;

namespace UntitledSpiderGame.Runtime.Player
{
    public interface IDamagable
    {
        event Action HealthChangedEvent;
        
        int CurrentHealth { get; }
        int MaxHealth { get; }

        void Damage(DamageArgs args, GameObject invoker, Vector2 point, Vector2 direction);
    }
}