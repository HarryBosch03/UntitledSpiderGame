using System;
using UnityEngine;
using UntitledSpiderGame.Runtime.Utility;

namespace UntitledSpiderGame.Runtime.Spider
{
    public class SpiderWebController : SpiderModule
    {
        private const float AnimationDuration = 0.1f;
        private const float LineWidth = 0.1f;

        private new float animation;

        private RaycastHit2D hit;
        private Vector2 localHitPoint;
        private SpiderController spider;
        private LineRenderer lines;

        public State CurrentState { get; private set; } = State.Idle;
        public Vector2 End => hit.point;

        protected override void Awake()
        {
            base.Awake();
            
            spider = GetComponent<SpiderController>();
            lines = transform.Find<LineRenderer>("Webs");
        }

        public void FixedUpdate()
        {
            var body = spider.Body;

            if (spider.Web && spider.Reaching)
            {
                Input(spider.ReachVector);
                spider.Web = false;
            }

            if (CurrentState == State.Attached)
            {
                if (!hit)
                {
                    Detach();
                    return;
                }

                var point = (Vector2)hit.collider.transform.TransformPoint(localHitPoint);
                var force = (point - body.position).normalized * Stats.webForce;
                
                if (hit.rigidbody) hit.rigidbody.AddForce(-force);
                else body.AddForce(force);
                
                var distance = (point - body.position).magnitude; 
                if (distance > Stats.webRange)
                {
                    var direction = (body.position - point).normalized;

                    body.position = point + direction * Stats.webRange;
                    body.velocity += direction * Mathf.Max(0.0f, -Vector2.Dot(direction, body.velocity));
                }
                
                if (distance < 2.0f)
                {
                    Detach();
                    animation = 0.0f;
                }
                if (Vector2.Dot(hit.normal, body.position - point) < 0.0f)
                {
                    Detach();
                }
            }

            AnimateLines();

            animation += (CurrentState != State.Idle ? 1.0f : -1.0f) / AnimationDuration * Time.deltaTime;
            if (animation >= 1.0f)
            {
                if (CurrentState == State.Casting)
                {
                    CurrentState = State.Attached;
                }

                animation = 1.0f;
            }
            else if (animation <= 0.0f) animation = 0.0f;
        }

        private void AnimateLines()
        {
            if (!hit || CurrentState == State.Idle)
            {
                lines.enabled = false;
                return;
            }
            
            var point = (Vector2)hit.collider.transform.TransformPoint(localHitPoint);
            
            lines.enabled = true;
            lines.useWorldSpace = true;
            lines.positionCount = 128;

            var distance = (spider.Body.position - point).magnitude;
            lines.startWidth = Stats.webRange * LineWidth / (distance + Stats.webRange * 0.5f);
            lines.endWidth = lines.startWidth;

            var start = spider.Body.position;
            var end = point;

            var normal = (end - start).normalized;
            var tangent = new Vector2(-normal.y, normal.x);

            for (var i = 0; i < lines.positionCount; i++)
            {
                var p = i / (lines.positionCount - 1.0f);

                var a1 = Mathf.InverseLerp(0.0f, 0.6f, animation);
                var a2 = Mathf.InverseLerp(0.4f, 1.0f, animation);

                lines.SetPosition(i, Vector2.Lerp(start, end, p * a1) + tangent * (Mathf.PerlinNoise(p * 3.0f, 0.5f) * 2.0f - 1.0f) * 3.0f * (1.0f - a2) * Mathf.Clamp01(animation * 10.0f));
            }
        }

        public void Input(Vector2 direction)
        {
            direction.Normalize();

            switch (CurrentState)
            {
                case State.Idle:
                {
                    var start = spider.Body.position;
                    var hits = Physics2D.RaycastAll(start, direction, Stats.webRange, (1 << 0) | (1 << 7));
                    var didHit = false;
                    foreach (var hit in hits)
                    {
                        if (hit.collider.transform.IsChildOf(spider.transform)) continue;

                        if (!didHit)
                        {
                            this.hit = hit;
                            didHit = true;
                        }
                        else if (hit.distance < this.hit.distance)
                        {
                            this.hit = hit;
                        }
                    }
                    if (didHit)
                    {
                        CurrentState = State.Casting;
                        localHitPoint = hit.collider.transform.InverseTransformPoint(hit.point);
                    }
                    break;
                }
                case State.Casting:
                {
                    Detach();
                    break;
                }
                case State.Attached:
                {
                    Detach();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Detach() { CurrentState = State.Idle; }

        public enum State
        {
            Idle,
            Casting,
            Attached,
        }
    }
}