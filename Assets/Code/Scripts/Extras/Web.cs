using System.Collections.Generic;
using UnityEngine;

namespace Crabs.Extras
{
    public class Web : MonoBehaviour
    {
        public const int WebLayer = 9;
        private const int SubFrames = 60;
        private const float NodeDistance = 1.0f;
        private const float WebWidth = 0.05f;

        private LineRenderer lines;
        private new PolygonCollider2D collider;
        private Vector2 end;

        public List<Node> nodes = new();

        private void Awake()
        {
            lines = GetComponentInChildren<LineRenderer>();
            lines.useWorldSpace = true;
            lines.startWidth = WebWidth;
            lines.endWidth = WebWidth;

            collider = GetComponent<PolygonCollider2D>();

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            foreach (var t in GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = WebLayer;
            }
        }

        public void StartWeb(Vector2 position)
        {
            end = position;
            var node = new Node(position);
            node.anchored = true;
            nodes.Add(node);
        }

        public void Catchup(Vector2 target, Vector2 velocity)
        {
            while ((target - end).magnitude > NodeDistance)
            {
                nodes.Add(new Node(end, velocity));
                end += (target - end).normalized * NodeDistance;
            }
        }

        private void FixedUpdate()
        {
            Iterate();
            Constrain();
            Collide();
            BuildCollision();
        }

        private void Collide()
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                var hit = Physics2D.Linecast(node.lastPosition1, node.position, 0b1);
                if (hit)
                {
                    node.position = hit.point;
                    node.anchored = true;
                }

                nodes[i] = node;
            }
        }

        private void Iterate()
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node.anchored)
                {
                    node.lastPosition1 = node.position;
                }
                else
                {
                    node.lastPosition2 = nodes[i].lastPosition1;
                    node.lastPosition1 = nodes[i].position;

                    var velocity = nodes[i].position - nodes[i].lastPosition1;
                    var acceleration = velocity - (nodes[i].lastPosition1 - nodes[i].lastPosition2) + Physics2D.gravity * Time.deltaTime;
                    
                    node.position = nodes[i].position + velocity + acceleration * Time.deltaTime * 0.5f;
                }

                nodes[i] = node;
            }
        }

        private void Constrain()
        {
            for (var i = 0; i < SubFrames; i++)
            {
                for (var j = 0; j < nodes.Count - 1; j++)
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
            }
        }

        private void BuildCollision()
        {
            if (nodes.Count < 2)
            {
                collider.points = new Vector2[0];
                return;
            }

            var points = new Vector2[nodes.Count * 2];

            for (var i = 0; i < nodes.Count; i++)
            {
                var a = nodes[i];
                Node b;
                Vector2 dir;
                if (i == nodes.Count - 1)
                {
                    b = nodes[i - 1];
                    dir = (a.position - b.position).normalized;
                }
                else
                {
                    b = nodes[i + 1];
                    dir = (b.position - a.position).normalized;
                }

                var tangent = new Vector2(-dir.y, dir.x);

                points[i] = nodes[i].position - tangent * WebWidth;
                points[^(i + 1)] = nodes[i].position + tangent * WebWidth;
            }

            collider.points = points;
        }

        private void Update()
        {
            lines.positionCount = nodes.Count;

            for (var i = 0; i < nodes.Count; i++)
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