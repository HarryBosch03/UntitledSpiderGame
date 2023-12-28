using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UntitledSpiderGame.Runtime.Utility
{
    [RequireComponent(typeof(ShadowCaster2D))]
    public sealed class CircleShadowCaster : MonoBehaviour
    {
        public Vector2 offset;
        public float radius = 0.5f;
        public int resolution = 32;

        private void Reset()
        {
            var collider = GetComponent<CircleCollider2D>();
            if (collider)
            {
                radius = collider.radius;
                offset = collider.offset;
            }
            OnValidate();
        }

        private void OnValidate()
        {
            var caster = GetComponent<ShadowCaster2D>();

            var path = new Vector3[resolution];
            for (var i = 0; i < resolution; i++)
            {
                var a = ((float)i / resolution) * Mathf.PI * 2.0f;
                path[i] = offset + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            }
            
            var field = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            field.SetValue(caster, path);
        }
    }
}
