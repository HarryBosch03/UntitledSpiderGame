using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace UntitledSpiderGame.Runtime.Utility
{
    [RequireComponent(typeof(ShadowCaster2D))]
    public sealed class BoxShadowCaster : MonoBehaviour
    {
        public Vector2 offset;
        public Vector2 size;

        private void Reset()
        {
            var collider = GetComponent<BoxCollider2D>();
            if (collider)
            {
                size = collider.size;
                offset = collider.offset;
            }
            OnValidate();
        }

        private void OnValidate()
        {
            var caster = GetComponent<ShadowCaster2D>();

            var path = new[]
            {
                (Vector3)offset + new Vector3(-size.x, -size.y) * 0.5f,
                (Vector3)offset + new Vector3(size.x, -size.y) * 0.5f,
                (Vector3)offset + new Vector3(size.x, size.y) * 0.5f,
                (Vector3)offset + new Vector3(-size.x, size.y) * 0.5f,
            };

            var field = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            field.SetValue(caster, path);
        }
    }
}
