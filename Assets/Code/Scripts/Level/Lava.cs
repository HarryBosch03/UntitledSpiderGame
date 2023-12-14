using System.Collections.Generic;
using Crabs.Player;
using UnityEngine;

namespace Crabs.Level
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
                damageable.Damage(1, e.transform.position, Vector2.up);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(-500.0f, height), new Vector3(500.0f, height));
        }
    }
}
