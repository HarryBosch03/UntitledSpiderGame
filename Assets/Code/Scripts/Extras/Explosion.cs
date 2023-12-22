using System.Collections;
using UnityEngine;
using UntitledSpiderGame.Runtime.Player;

namespace UntitledSpiderGame.Runtime.Extras
{
    public class Explosion : MonoBehaviour
    {
        public float delay;
        public float radius = 4.0f;
        public float damage = 200.0f;
        public float knockback = 10.0f;

        private Transform vfx;

        private void Awake()
        {
            vfx = transform.Find("vfx");
            if (vfx)
            {
                vfx.gameObject.SetActive(false);
            }
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(delay);
            At(gameObject, transform.position, radius, damage, knockback);

            if (vfx)
            {
                vfx.gameObject.SetActive(true);
                vfx.transform.SetParent(null);
            }

            Destroy(gameObject);
        }

        private void OnDisable()
        {
            Destroy(gameObject);
        }

        public static void At(GameObject invoker, Vector2 position, float radius, float damage, float knockback)
        {
            for (var i = 0; i < 36; i++)
            {
                var a = i / 18.0f * Mathf.PI;
                var direction = new Vector2(Mathf.Cos(a), Mathf.Sin(a));

                var covered = 0.0f;
                while (covered < radius)
                {
                    var hit = Physics2D.Raycast(position + direction * covered, direction, radius - covered);
                    if (!hit) break;

                    var damageable = hit.collider.GetComponentInParent<IDamagable>();
                    if (damageable == null) break;

                    var v = 1.0f - (hit.point - position).magnitude / radius;
                    var args = new DamageArgs()
                    {
                        damage = damage * v,
                        knockback = knockback * v,
                    };

                    damageable.Damage(args, invoker, hit.point, direction);

                    covered += hit.distance + 0.05f;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}