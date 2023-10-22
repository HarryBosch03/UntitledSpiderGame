using Crabs.Items;
using UnityEngine;

namespace Crabs.Player
{
    public class SpiderLeg
    {
        private const int IkIterations = 20;

        public Transform upperLeg, lowerLeg, tipTransform;
        public ParticleSystem stepDust;

        public SpiderController Spider { get; private set; }
        public Vector2? OverrideTargetLocal { get; set; }
        public bool Locked { get; set; }

        public Vector2 root, mid, tip;
        public Vector2 target;
        public Vector2 actual;
        public Vector2 restPosition;
        public Vector2 anchoredPosition;
        public bool anchored;

        public bool controlled;
        public bool stepped;
        public Item heldItem;

        public SpiderLeg(Transform upperLeg, Transform lowerLeg, int i, bool controlled)
        {
            this.upperLeg = upperLeg;
            this.lowerLeg = lowerLeg;
            this.controlled = controlled;

            tipTransform = lowerLeg.GetChild(0);
            stepDust = tipTransform.GetComponentInChildren<ParticleSystem>();

            var a = (i * 90.0f + 45.0f) * Mathf.Deg2Rad;
            restPosition = new Vector2(Mathf.Cos(a) * 2.0f, Mathf.Sin(a)).normalized;
        }

        public void FixedUpdate(SpiderController spider)
        {
            this.Spider = spider;
            root = spider.Body.position;

            actual = Vector3.Lerp(target, actual, spider.legSmoothing);
            if ((target - actual).magnitude < 0.02f && !stepped)
            {
                stepDust.Play();
                stepped = true;
            }

            UpdateAnchor();
            UpdateTarget();
            UpdateHeldItem();

            IK();
            UpdateTransforms();
        }

        private void UpdateTarget()
        {
            if (OverrideTargetLocal.HasValue)
            {
                anchored = false;
                target = Spider.Body.position + OverrideTargetLocal.Value * Spider.LegTotalLength;
                return;
            }

            if (controlled && Spider.Reaching)
            {
                anchored = false;
                target = Spider.Body.position + Spider.Input.ReachVector * Spider.LegTotalLength;
                Collide(ref target);
            }
            else
            {
                target = anchoredPosition;
            }
        }

        private void Collide(ref Vector2 end)
        {
            var hit = Physics2D.Linecast(root, end);
            if (hit)
            {
                end = hit.point;
            }
        }

        private void UpdateAnchor()
        {
            if (Locked) return;
            if (anchored)
            {
                if ((anchoredPosition - Spider.Body.position).magnitude > Spider.LegTotalLength)
                {
                    anchored = false;
                }

                return;
            }

            anchoredPosition = Spider.Body.position + restPosition * Spider.LegTotalLength * 0.75f;

            var best = 0.2f;
            foreach (var cast in Spider.wallCasts)
            {
                var score = float.MaxValue;
                foreach (var leg in Spider.legs)
                {
                    if (leg == this) continue;
                    if (!leg.anchored) continue;

                    score = Mathf.Min(score, (leg.anchoredPosition - cast.point).magnitude);
                }

                if (score > best)
                {
                    best = score;
                    anchoredPosition = cast.point;
                    anchored = true;
                    stepped = false;
                }
            }
        }

        private void UpdateHeldItem()
        {
            if (!controlled) return;
            if (!Spider.Reaching) return;

            stepped = true;
            
            if (heldItem)
            {
                target = heldItem.ModifyReachPosition(target) ?? target;

                if (Spider.Input.PrimaryUse)
                {
                    heldItem.PrimaryUse();
                    Spider.Input.PrimaryUse = false;
                }

                if (Spider.Input.SecondaryUse)
                {
                    heldItem.SecondaryUse();
                    Spider.Input.SecondaryUse = false;
                }

                if (Spider.Input.Drop)
                {
                    heldItem = heldItem.Dispose();
                    Spider.Input.Drop = false;
                }
            }
            else if (Spider.Input.Drop)
            {
                if (Spider.spawnItem) heldItem = Spider.spawnItem.Instantiate(this);
                Spider.Input.Drop = false;
            }
        }

        private void UpdateTransforms()
        {
            upperLeg.position = new Vector3(root.x, root.y, upperLeg.position.z);
            upperLeg.right = mid - root;

            lowerLeg.position = new Vector3(mid.x, mid.y, lowerLeg.position.z);
            lowerLeg.right = tip - mid;
        }

        private void IK()
        {
            var upperLength = Spider.LegUpperLength;
            var lowerLength = Spider.LegLowerLength;
            var rootTarget = Spider.Body.position;

            var flipped = false;
            for (var i = 0; i < IkIterations; i++)
            {
                tip = actual;
                mid = (mid - tip).normalized * lowerLength + tip;
                root = (root - mid).normalized * upperLength + mid;

                flip();
            }

            if (flipped) flip();
            HintLeg();

            void flip()
            {
                var va = root;
                var vb = tip;

                root = vb;
                tip = va;

                va = rootTarget;
                vb = actual;

                rootTarget = vb;
                actual = va;

                var fa = upperLength;
                var fb = lowerLength;

                upperLength = fb;
                lowerLength = fa;

                flipped = !flipped;
            }
        }

        private void HintLeg()
        {
            if (Locked) return;

            var up = (Spider.Body.position - Spider.GroundPoint).normalized;

            var a = mid;

            var vector = (a - root);
            var normal = (tip - root).normalized;
            var tangent = new Vector2(-normal.y, normal.x);

            var dn = Vector2.Dot(normal, vector);
            var dt = Vector2.Dot(tangent, vector);

            var b = root + normal * dn - tangent * dt;

            var da = Mathf.Abs(Vector2.Dot(a - root, up));
            var db = Mathf.Abs(Vector2.Dot(b - root, up));

            if (db < da) mid = b;
        }
    }
}