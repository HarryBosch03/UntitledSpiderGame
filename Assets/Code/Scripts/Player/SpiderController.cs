using System.Collections.Generic;
using Crabs.Items;
using UnityEngine;

namespace Crabs.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpiderInput))]
    [SelectionBase, DisallowMultipleComponent]
    public sealed class SpiderController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 10.0f;
        [SerializeField] private float accelerationTime = 0.1f;

        [Space]
        public Item spawnItem;
        
        [Space]
        [SerializeField] private float rotationSpring = 0.2f;
        [SerializeField] private float rotationDamping = 0.05f;

        [Space]
        [Range(0.0f, 1.0f)] public float legSmoothing = 0.3f;
        [SerializeField] [Range(0.0f, 1.0f)] private float bodyPartSmoothing = 0.8f;
        [SerializeField] [Range(0.0f, 1.0f)] private float cameraSmoothing = 0.1f;

        private const int LegCastLayerMask = 0b011111111;
        
        private Camera mainCam;
        private Vector2 groundVector = Vector2.down;

        public SpiderInput Input { get; private set; }
        public Rigidbody2D Body { get; private set; }
        public float LegUpperLength { get; private set; }
        public float LegLowerLength { get; private set; }
        public float LegTotalLength { get; private set; }
        public Vector2 ReachVector { get; private set; }
        public int Direction { get; private set; } = 1;
        public SpiderLeg ArmLeg => legs[0];

        private Transform butt, head;
        private Transform[] legRoots = new Transform[4];
        private Transform[] legMids = new Transform[4];
        private Transform[] legTips = new Transform[4];

        public readonly List<RaycastHit2D> wallCasts = new();
        public readonly List<SpiderLeg> legs = new();
        public readonly List<SpiderBodyPart> bodyParts = new();

        private void Awake()
        {
            Input = GetComponent<SpiderInput>();
            Body = GetComponent<Rigidbody2D>();

            mainCam = Camera.main;

            var model = transform.Find("Model");
            butt = model.Find("Butt");
            head = model.Find("Head");
            
            bodyParts.Add(new SpiderBodyPart(butt));
            bodyParts.Add(new SpiderBodyPart(head));

            for (var i = 0; i < 4; i++)
            {
                legRoots[i] = model.Find($"Leg.Root.{i}");
                legMids[i] = legRoots[i].GetChild(0);
                legTips[i] = legMids[i].GetChild(0);

                legs.Add(new SpiderLeg(legRoots[i], legMids[i], i, i == 0));
            }

            LegUpperLength = (legRoots[0].position - legMids[0].position).magnitude;
            LegLowerLength = (legMids[0].position - legTips[0].position).magnitude;
            LegTotalLength = LegUpperLength + LegLowerLength;
        }

        private void FixedUpdate()
        {
            Cast();

            ReachVector = Input.ReachDirection * LegTotalLength;
            
            var legsAnchored = 0;
            foreach (var leg in legs)
            {
                leg.FixedUpdate(this);
                if (leg.anchored) legsAnchored++;
            }

            var facing = Input.MoveDirection.x * -groundVector.y;
            if (Mathf.Abs(facing) > 0.2f) Direction = facing > 0.0f ? 1 : -1;
            foreach (var e in bodyParts)
            {
                e.FixedUpdate(Direction, bodyPartSmoothing);
            }
            
            var anchored = legsAnchored > 0;

            if (anchored)
            {
                var target = Input.MoveDirection * moveSpeed;
                var force = (target - Body.velocity) * 2.0f / accelerationTime;
                Body.AddForce((force - Physics2D.gravity * Body.gravityScale) * Body.mass);

                var targetAngle = Mathf.Atan2(groundVector.y, groundVector.x) * Mathf.Rad2Deg + 90.0f;
                var torque = (targetAngle - Body.rotation) * rotationSpring - Body.angularVelocity * rotationDamping;
                Body.AddTorque(torque);
            }

            UpdateCamera();
            Input.ResetTriggers();
        }

        private void Cast()
        {
            const int iterations = 64;

            var groundPoint = Vector2.zero;
            wallCasts.Clear();
            var hits = 0;
            
            for (var i = 0; i < iterations; i++)
            {
                var a = (i / (float)iterations) * Mathf.PI * 2.0f;
                var v = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                var hit = Physics2D.Raycast(Body.position, v, LegTotalLength, LegCastLayerMask);
                if (!hit) continue;
                
                wallCasts.Add(hit);
                groundPoint += hit.point;
                hits++;
            }

            if (hits == 0) return;
            
            groundPoint /= hits;
            groundVector = (groundPoint - Body.position).normalized;
        }

        private void UpdateCamera()
        {
            var position = new Vector3
            {
                x = Body.position.x,
                y = Body.position.y,
                z = -10.0f,
            };
            mainCam.transform.position = Vector3.Lerp(position, mainCam.transform.position, cameraSmoothing);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            foreach (var hit in wallCasts)
            {
                Gizmos.DrawSphere(hit.point, 0.05f);
            }

            for (var i = 0; i < legs.Count; i++)
            {
                Gizmos.color = new []
                {
                    Color.green,
                    Color.yellow,
                    Color.cyan,
                    Color.magenta,
                }[i];

                var leg = legs[i];
                Gizmos.DrawLine(leg.root, leg.mid);
                Gizmos.DrawLine(leg.mid, leg.tip);
                Gizmos.DrawSphere(leg.target, 0.05f);
            }
        }
    }
}