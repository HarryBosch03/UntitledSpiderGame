using System;
using UnityEngine;

namespace Crabs.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class FlipWeapon : MonoBehaviour
    {
        private Transform root;

        private void Awake()
        {
            root = GetComponentInParent<Rigidbody2D>().transform;
        }

        private void LateUpdate()
        {
            transform.localScale = new Vector3(1.0f, Vector2.Dot(transform.right, root.right) > 0.0f ? 1.0f : -1.0f, 1.0f);
        }
    }
}
