using UnityEngine;

namespace Crabs.Player
{
    public class SpiderDanceState : SpiderState
    {
        private static readonly Vector2 DanceStrength = new Vector2(0.0f, 1.0f) * 0.2f;
        private const float DanceSpeed = 1.0f;
        
        private Vector2 dancePosition;
        private Vector2 danceNormal;
        private float timer;

        public override void Enter(SpiderController spider)
        {
            danceNormal = (spider.Body.position - spider.GroundPoint).normalized;
            dancePosition = spider.GroundPoint + danceNormal * 0.6f;
            spider.Reaching = false;

            for (var i = 2; i < spider.legs.Count; i++)
            {
                var leg = spider.legs[i];
                leg.Locked = true;
            }
        }

        public override void FixedUpdate(SpiderController spider)
        {
            var x = timer * DanceSpeed * Mathf.PI * 2.0f;
            var basis = new Vector2(Mathf.Sin(x), Mathf.Sin(2.0f * x)) * DanceStrength;

            var danceTangent = new Vector2(-danceNormal.y, danceNormal.x);
            var target = dancePosition + fromLocal(basis);

            spider.Move(Vector2.zero);
            
            spider.legs[0].OverrideTargetLocal = fromLocal(new Vector2(-1.0f, 1.0f)).normalized * 0.8f;
            spider.legs[1].OverrideTargetLocal = fromLocal(new Vector2(1.0f, 1.0f)).normalized * 0.8f;
            
            timer += Time.deltaTime;

            if (!spider.Input.Dance)
            {
                spider.ChangeState(new SpiderMoveState());
            }

            Vector2 fromLocal(Vector2 vector) => danceTangent * vector.x + danceNormal * vector.y;
        }

        public override void Exit(SpiderController spider)
        {
            spider.legs[0].OverrideTargetLocal = null;
            spider.legs[1].OverrideTargetLocal = null;
            
            foreach (var leg in spider.legs)
            {
                leg.Locked = false;
            }
        }
    }
}