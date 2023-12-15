using UnityEngine;

namespace Crabs.Player
{
    public sealed class ColorWithSpider : MonoBehaviour
    {
        public void SetColor(Color color)
        {
            if (TryGetComponent(out ParticleSystem ps))
            {
                var main = ps.main;
                main.startColor = color;
            }
        }
    }
}
