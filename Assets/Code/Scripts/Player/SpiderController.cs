using System;
using System.Collections.Generic;
using Crabs.Items;
using UnityEngine;

namespace Crabs.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [SelectionBase, DisallowMultipleComponent]
    public sealed class SpiderController : MonoBehaviour
    {
        private const float IgnoreGroundAfterJumpTime = 0.15f;
        
        [SerializeField] private float moveSpeed = 10.0f;
        [SerializeField] private float accelerationTime = 0.1f;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float airMovement = 0.2f;

        public Item spawnItem;

        [Space]
        [SerializeField] private float rotationSpring = 0.2f;
        [SerializeField] private float rotationDamping = 0.05f;
        [SerializeField] private float jumpForce = 25.0f;

        [Space]
        [Range(0.0f, 1.0f)] public float legSmoothing = 0.3f;
        [SerializeField] [Range(0.0f, 1.0f)] private float bodyPartSmoothing = 0.8f;

        private const int LegCastLayerMask = 0b001111111;
        
        private float lastJumpTime;
        public static event Action<SpiderController> DiedEvent;

        public static readonly List<SpiderController> All = new();

        #region Input
        public bool Reaching { get; private set; }
        public Vector2 ReachVector { get; set; }
        public Vector2 MoveDirection { get; set; }
        public bool Use { get; set; }
        public bool Drop { get; set; }
        public bool Jump { get; set; }
        #endregion
        
        public Vector2 GroundPoint { get; private set; }
        public Rigidbody2D Body { get; private set; }
        public float LegUpperLength { get; private set; }
        public float LegLowerLength { get; private set; }
        public float LegTotalLength { get; private set; }
        public int Direction { get; private set; } = 1;
        public bool Anchored { get; private set; }
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
            Body = GetComponent<Rigidbody2D>();

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
                bodyParts.Add(new SpiderBodyPart(legRoots[i])
                {
                    flip = false,
                });
                bodyParts.Add(new SpiderBodyPart(legMids[i])
                {
                    flip = false,
                });
            }

            LegUpperLength = (legRoots[0].position - legMids[0].position).magnitude;
            LegLowerLength = (legMids[0].position - legTips[0].position).magnitude;
            LegTotalLength = LegUpperLength + LegLowerLength;
        }

        private void OnEnable()
        {
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
            DiedEvent?.Invoke(this);

            foreach (var leg in legs)
            {
                leg.OnDisable();
            }
        }

        private void FixedUpdate()
        {
            Reaching = ReachVector.magnitude > 0.2f;
            
            Cast();
            Move();
            UpdateLegs();
            UpdateDirection();
            PerformJump();

            ResetFlags();
        }

        private void ResetFlags()
        {
            Use = false;
            Drop = false;
            Jump = false;
        }

        public void PerformJump()
        {
            if (!Jump) return;
            if (!Anchored) return;
            
            Vector2 direction;
            if (MoveDirection.magnitude > 0.5f) direction = MoveDirection;
            else direction = transform.up;

            var force = direction.normalized * jumpForce;
            Body.AddForce(force - Body.velocity, ForceMode2D.Impulse);
                
            lastJumpTime = Time.time;
        }

        private void UpdateDirection()
        {
            var groundVector = GroundPoint - Body.position;
            
            var facing = MoveDirection.x * -groundVector.y;
            if (Mathf.Abs(facing) > 0.2f) Direction = facing > 0.0f ? 1 : -1;
            foreach (var e in bodyParts)
            {
                e.FixedUpdate(Direction, bodyPartSmoothing);
            }
        }

        private void UpdateLegs()
        {
            var legsAnchored = 0;
            foreach (var leg in legs)
            {
                leg.FixedUpdate(this, Time.time - lastJumpTime < IgnoreGroundAfterJumpTime);
                if (leg.anchored) legsAnchored++;
            }

            Anchored = legsAnchored > 0;
        }

        public void Move()
        {
            var target = Vector2.ClampMagnitude(MoveDirection, 1.0f) * moveSpeed;
            
            if (Anchored)
            {
                var force = (target - Body.velocity) * 2.0f / accelerationTime;
                Body.AddForce((force - Physics2D.gravity * Body.gravityScale) * Body.mass);
                
                var groundVector = GroundPoint - Body.position;
                var targetAngle = Mathf.Atan2(groundVector.y, groundVector.x) * Mathf.Rad2Deg + 90.0f;
                var torque = Mathf.DeltaAngle(Body.rotation, targetAngle) * rotationSpring - Body.angularVelocity * rotationDamping;
                Body.AddTorque(torque);
            }
            else
            {
                var force = (target.x - Body.velocity.x) * (2.0f / accelerationTime) * airMovement * Mathf.Abs(MoveDirection.x);
                Body.AddForce(Vector2.right * force * Body.mass);
            }
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
            GroundPoint = groundPoint;
        }

        public void SetColor(Color color)
        {
            var propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_MainColor", color);
            foreach (var e in bodyParts) e.SetMaterialPropertyBlock(propertyBlock);

            foreach (var e in GetComponentsInChildren<ColorWithSpider>(true))
            {
                e.SetColor(color);
            }
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