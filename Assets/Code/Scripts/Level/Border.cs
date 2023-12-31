using System;
using System.Collections.Generic;
using UnityEngine;
using UntitledSpiderGame.Runtime.Player;
using UntitledSpiderGame.Runtime.Spider;

namespace UntitledSpiderGame.Runtime.Level
{
    public class Border : MonoBehaviour
    {
        public Vector2 size;
        public float padding;
        public DamageArgs damage;

        private void Awake()
        {
            gameObject.layer = 10;
            
            var colliders = new List<BoxCollider2D>();
            colliders.AddRange(GetComponents<BoxCollider2D>());

            while (colliders.Count < 4)
            {
                var collider = gameObject.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                colliders.Add(collider);
            }

            colliders[0].offset = new Vector2(0.0f, -padding * 0.5f);
            colliders[0].size = new Vector2(size.x + padding * 2.0f, padding);
            
            colliders[1].offset = new Vector2(0.0f, size.y + padding * 0.5f);
            colliders[1].size = new Vector2(size.x + padding * 2.0f, padding);
            
            colliders[2].offset = new Vector2((size.x + padding) * 0.5f, size.y * 0.5f);
            colliders[2].size = new Vector2(padding, size.y);
            
            colliders[3].offset = new Vector2(-(size.x + padding) * 0.5f, size.y * 0.5f);
            colliders[3].size = new Vector2(padding, size.y);
        }

        private void FixedUpdate()
        {
            var spiders = SpiderController.All.ToArray();
            
            foreach (var spider in spiders)
            {
                CheckSpider(spider);
            }
        }

        private void CheckSpider(SpiderController spider)
        {
            CheckEdge(spider, Vector2.left);
            CheckEdge(spider, Vector2.right);
            CheckEdge(spider, Vector2.up);
            CheckEdge(spider, Vector2.down);
        }

        private void CheckEdge(SpiderController spider, Vector2 normal)
        {
            var width = Mathf.Abs(Vector2.Dot(normal, size)) * 0.5f;
            var position = (Vector2)spider.transform.position - (normal * width + Vector2.up * size.y * 0.5f);
            var dot = Vector2.Dot(position, normal);

            if (dot < 0.0f) return;

            spider.Body.velocity = Vector2.zero;
            
            var health = spider.GetComponent<IDamagable>();
            health.Damage(damage, gameObject, spider.transform.position, -normal);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector2(0.0f, size.y * 0.5f), size);
            Gizmos.DrawWireCube(new Vector2(0.0f, size.y * 0.5f), size + Vector2.one * padding * 2.0f);
        }
    }
}
