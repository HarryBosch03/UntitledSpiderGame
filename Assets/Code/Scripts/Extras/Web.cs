using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crabs.Extras
{
    public class Web : MonoBehaviour
    {
        public const int WebLayer = 9;
        private const int SubFrames = 6;
        private const float NodeDistance = 0.2f;
        private const float WebWidth = 0.05f;

        private LineRenderer lines;

        [HideInInspector] public Rigidbody2D start;
        [HideInInspector] public Rigidbody2D end;

        public Node[] nodes;

        private void Awake()
        {
            lines = GetComponentInChildren<LineRenderer>();
            lines.useWorldSpace = true;
            lines.startWidth = WebWidth;
            lines.endWidth = WebWidth;

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            foreach (var t in GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = WebLayer;
            }
        }

        public void StartWeb(Vector2 position, Vector2 velocity, float totalLength)
        {
            var nodeCount = Mathf.Max(Mathf.CeilToInt(totalLength / NodeDistance), 2);
            nodes = new Node[nodeCount];

            for (var i = 0; i < nodes.Length; i++)
            {
                var p = i / (nodes.Length - 1.0f);

                nodes[i] = new Node(position + velocity * Time.fixedDeltaTime * p, velocity * p);
            }
        }

        private void FixedUpdate()
        {
            Constrain();
            Iterate();
        }

        private void Collide()
        {
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];

                var direction = node.position - node.lastPosition1;
                var length = direction.magnitude;
                direction /= length;

                if (length > WebWidth * 0.5f)
                {
                    var hit = Physics2D.CircleCast(node.lastPosition1, WebWidth * 0.5f, direction, length, 0b1);
                    if (hit)
                    {
                        node.position = hit.collider.ClosestPoint(hit.point);
                        node.lastPosition1 = node.position;
                        node.lastPosition2 = node.position;
                    }
                }

                var c = Physics2D.OverlapCircle(node.position, WebWidth * 0.5f, 0b1);
                if (c)
                {
                    node.position = c.ClosestPoint(node.position);
                    node.lastPosition1 = node.position;
                    node.lastPosition2 = node.position;
                }

                nodes[i] = node;
            }
        }

        private void Iterate()
        {
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node.anchored)
                {
                    node.lastPosition1 = node.position;
                }
                else
                {
                    node.lastPosition2 = nodes[i].lastPosition1 + Physics2D.gravity * Time.deltaTime;
                    node.lastPosition1 = nodes[i].position;

                    var velocity = nodes[i].position - nodes[i].lastPosition1;
                    var acceleration = velocity - (nodes[i].lastPosition1 - nodes[i].lastPosition2);

                    node.position = nodes[i].position + velocity + acceleration * Time.deltaTime * 0.5f;
                }

                nodes[i] = node;
            }
        }

        private void Constrain()
        {
            for (var i = 0; i < SubFrames; i++)
            {
                for (var j = 0; j < nodes.Length - 1; j++)
                {
                    var a = nodes[j];
                    var b = nodes[j + 1];

                    var diff = b.position - a.position;
                    var dir = diff.normalized;
                    var center = (a.position + b.position) * 0.5f;

                    if (diff.magnitude > NodeDistance)
                    {
                        a.position = center - dir * NodeDistance * 0.5f;
                        b.position = center + dir * NodeDistance * 0.5f;
                    }

                    if (!a.anchored) nodes[j] = a;
                    if (!b.anchored) nodes[j + 1] = b;
                }
                
                Collide();
            }
        }

        private void Update()
        {
            lines.positionCount = nodes.Length;

            for (var i = 0; i < nodes.Length; i++)
            {
                lines.SetPosition(i, nodes[i].position);
            }
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var n in nodes)
            {
                Gizmos.DrawSphere(n.position, WebWidth * 6.0f);
            }
        }

        public struct Node
        {
            public Vector2 lastPosition2;
            public Vector2 lastPosition1;
            public Vector2 position;
            public bool anchored;

            public Node(Vector2 position) : this()
            {
                lastPosition2 = position;
                lastPosition1 = position;
                this.position = position;
            }

            public Node(Vector2 position, Vector2 velocity) : this()
            {
                this.position = position;
                lastPosition1 = position - velocity * Time.fixedDeltaTime;
                lastPosition2 = lastPosition1 - velocity * Time.fixedDeltaTime;
            }
        }
    }
}