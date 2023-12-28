using System.Collections.Generic;
using UnityEngine;
using UntitledSpiderGame.Runtime.Player;

namespace UntitledSpiderGame.Runtime.Spider
{
    public class SpiderMovement : SpiderModule
    {
        private const float IgnoreGroundAfterJumpTime = 0.15f;

        [SerializeField] private float accelerationTime = 0.1f;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float airMovement = 0.2f;

        [Space]
        [SerializeField] private float rotationSpring = 0.2f;
        [SerializeField] private float rotationDamping = 0.05f;
        [SerializeField] private float jumpForce = 25.0f;

        private float lastJumpTime;
        
        public float LegUpperLength { get; private set; }
        public float LegLowerLength { get; private set; }
        public float LegTotalLength { get; private set; }

        private Transform[] legRoots = new Transform[4];
        private Transform[] legMids = new Transform[4];
        private Transform[] legTips = new Transform[4];

        public readonly List<RaycastHit2D> wallCasts = new();

        protected override void Awake()
        {
            base.Awake();
            
            var model = transform.Find("Model");

            for (var i = 0; i < 4; i++)
            {
                legRoots[i] = model.Find($"Leg.Root.{i}");
                legMids[i] = legRoots[i].GetChild(0);
                legTips[i] = legMids[i].GetChild(0);

                Spider.Legs.Add(new SpiderLeg(this, legRoots[i], legMids[i], i));
            }

            LegUpperLength = (legRoots[0].position - legMids[0].position).magnitude;
            LegLowerLength = (legMids[0].position - legTips[0].position).magnitude;
            LegTotalLength = LegUpperLength + LegLowerLength;
        }
        
        private void FixedUpdate()
        {
            Spider.Reaching = Spider.ReachVector.magnitude > 0.2f;

            Cast();
            Move();
            UpdateLegs();
            UpdateDirection();
            PerformJump();

            ResetFlags();
        }

        private void ResetFlags()
        {
            Spider.Jump = false;
        }

        public void PerformJump()
        {
            if (!Spider.Jump) return;
            if (!Spider.Anchored) return;

            Vector2 direction;
            if (Spider.MoveDirection.magnitude > 0.5f) direction = Spider.MoveDirection;
            else direction = transform.up;

            var force = direction.normalized * jumpForce;
            Spider.Body.AddForce(force - Spider.Body.velocity, ForceMode2D.Impulse);

            lastJumpTime = Time.time;
        }

        private void UpdateDirection()
        {
            var facing = Spider.Reaching ? Vector2.Dot(transform.right, Spider.ReachVector) : Vector2.Dot(transform.right, Spider.MoveDirection);
            if (Mathf.Abs(facing) > 0.2f) Spider.Direction = facing > 0.0f ? 1 : -1;
        }

        private void UpdateLegs()
        {
            var legsAnchored = 0;
            foreach (var leg in Spider.Legs)
            {
                leg.FixedUpdate(Time.time - lastJumpTime < IgnoreGroundAfterJumpTime);
                if (leg.anchored) legsAnchored++;
            }

            Spider.Anchored = legsAnchored > 0;
        }

        public void Move()
        {
            var target = Vector2.ClampMagnitude(Spider.MoveDirection, 1.0f) * Stats.moveSpeed;

            var force = Vector2.zero;
            var up = -transform.up;
            
            if (Spider.Anchored)
            {
                force = (target - Spider.Body.velocity) * 2.0f / accelerationTime;
                force -= Physics2D.gravity * Spider.Body.gravityScale;

                up = Spider.GroundPoint - Spider.Body.position;
            }
            else
            {
                force = Vector2.right * (target.x - Spider.Body.velocity.x) * (2.0f / accelerationTime) * airMovement * Mathf.Abs(Spider.MoveDirection.x);
            }

            var targetAngle = Mathf.Atan2(up.y, up.x) * Mathf.Rad2Deg + 90.0f;
            var torque = Mathf.DeltaAngle(Spider.Body.rotation, targetAngle) * rotationSpring - Spider.Body.angularVelocity * rotationDamping;
            Spider.Body.AddTorque(torque);

            Spider.Body.AddForce(force * Spider.Body.mass);
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
                var hit = Physics2D.Raycast(Spider.Body.position, v, LegTotalLength, SpiderLeg.LegMask);
                if (!hit) continue;

                wallCasts.Add(hit);
                groundPoint += hit.point;
                hits++;
            }

            if (hits == 0) return;

            groundPoint /= hits;
            Spider.GroundPoint = groundPoint;
        }

        private void OnDrawGizmos()
        {
            var spider = GetComponent<SpiderController>();
            if (!spider) return;
            
            Gizmos.color = Color.green;
            foreach (var hit in wallCasts)
            {
                Gizmos.DrawSphere(hit.point, 0.05f);
            }

            for (var i = 0; i < spider.Legs.Count; i++)
            {
                Gizmos.color = new[]
                {
                    Color.green,
                    Color.yellow,
                    Color.cyan,
                    Color.magenta,
                }[i];

                var leg = spider.Legs[i];
                Gizmos.DrawLine(leg.root, leg.mid);
                Gizmos.DrawLine(leg.mid, leg.tip);
                Gizmos.DrawSphere(leg.target, 0.05f);
            }
        }
    }
}