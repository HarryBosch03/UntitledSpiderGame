using Crabs.Items;
using UnityEngine;

namespace Crabs.Player
{
    public class SpiderLeg
    {
        private const int IkIterations = 20;

        public Transform upperLeg, lowerLeg;

        public SpiderController Spider { get; private set; }

        public Vector2 root, mid, tip;
        public Vector2 target;
        public Vector2 actual;
        public Vector2 restPosition;
        public Vector2 anchoredPosition;
        public bool anchored;

        public bool controlled;
        public Item heldItem;

        public SpiderLeg(Transform upperLeg, Transform lowerLeg, int i, bool controlled)
        {
            this.upperLeg = upperLeg;
            this.lowerLeg = lowerLeg;
            this.controlled = controlled;

            var a = (i * 90.0f + 45.0f) * Mathf.Deg2Rad;
            restPosition = new Vector2(Mathf.Cos(a) * 2.0f, Mathf.Sin(a)).normalized;
        }

        public void FixedUpdate(SpiderController spider)
        {
            this.Spider = spider;
            root = spider.Body.position;

            actual = Vector3.Lerp(target, actual, spider.legSmoothing);

            UpdateAnchor();
            UpdateTarget();
            UpdateHeldItem();

            IK();
            UpdateTransforms();
        }

        private void UpdateTarget()
        {
            if (controlled && Spider.Input.Reaching)
            {
                anchored = false;
                target = Spider.Body.position + Spider.Input.ReachDirection * Spider.LegTotalLength;
            }
            else
            {
                target = anchoredPosition;
            }
        }

        private void UpdateAnchor()
        {
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
                }
            }
        }

        private void UpdateHeldItem()
        {
            if (!controlled) return;
            if (!Spider.Input.Reaching) return;
            
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
            upperLeg.position = root;
            upperLeg.right = mid - root;

            lowerLeg.position = mid;
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
    }
}