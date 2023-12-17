using UnityEngine;

namespace UntitledSpiderGame.Runtime.Player
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
