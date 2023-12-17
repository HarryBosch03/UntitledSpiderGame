using System;
using Crabs.Player;
using UnityEngine;

namespace Crabs.Extras
{
    [System.Serializable]
    public class Web
    {
        private const float AnimationDuration = 0.1f;
        private const float LineWidth = 0.1f;

        public float maxWebLength = 30.0f;
        public float webForce = 200.0f;

        private float animation;

        private RaycastHit2D hit;

        public State CurrentState { get; private set; } = State.Idle;
        public Vector2 End => hit.point;

        public void FixedUpdate(SpiderController spider, LineRenderer lines)
        {
            var body = spider.Body;

            if (CurrentState == State.Attached)
            {
                body.AddForce((hit.point - body.position).normalized * webForce);
                
                var distance = (hit.point - body.position).magnitude; 
                if (distance > maxWebLength)
                {
                    var direction = (body.position - hit.point).normalized;

                    body.position = hit.point + direction * maxWebLength;
                    body.velocity += direction * Mathf.Max(0.0f, -Vector2.Dot(direction, body.velocity));
                }
                
                if (distance < 2.0f)
                {
                    Detach();
                    animation = 0.0f;
                }
                if (Vector2.Dot(hit.normal, body.position - hit.point) < 0.0f)
                {
                    Detach();
                }
            }

            AnimateLines(spider, lines);

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

        private void AnimateLines(SpiderController spider, LineRenderer lines)
        {
            lines.useWorldSpace = true;
            lines.positionCount = 128;

            var distance = (spider.Body.position - hit.point).magnitude;
            lines.startWidth = maxWebLength * LineWidth / (distance + maxWebLength * 0.5f);
            lines.endWidth = lines.startWidth;

            var start = spider.Body.position;
            var end = hit.point;

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

        public void Input(SpiderController spider, Vector2 direction)
        {
            direction.Normalize();

            switch (CurrentState)
            {
                case State.Idle:
                {
                    var start = spider.Body.position;
                    hit = Physics2D.Raycast(start, direction, maxWebLength, 0b1);
                    if (hit) CurrentState = State.Casting;
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