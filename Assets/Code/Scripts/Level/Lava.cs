using System.Collections.Generic;
using UnityEngine;
using UntitledSpiderGame.Runtime.Player;
using UntitledSpiderGame.Runtime.Spider;

namespace UntitledSpiderGame.Runtime.Level
{
    public class Lava : MonoBehaviour
    {
        public float height;
        
        private void FixedUpdate()
        {
            var players = new List<SpiderController>(SpiderController.All);
            foreach (var e in players)
            {
                if (e.transform.position.y > height) continue;

                var damageable = e.GetComponent<IDamagable>();
                damageable.Damage(new DamageArgs
                {
                    damage = 1
                }, gameObject, e.transform.position, Vector2.up);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(-500.0f, height), new Vector3(500.0f, height));
        }
    }
}
