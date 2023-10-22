using UnityEngine;

namespace Crabs.Player
{
    public interface IDamagable
    {
        void Damage(int damage, Vector2 point, Vector2 direction);
    }
}