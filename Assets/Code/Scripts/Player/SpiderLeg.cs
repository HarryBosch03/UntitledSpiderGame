using Crabs.Items;
using UnityEngine;

namespace Crabs.Player
{
    public class SpiderLeg
    {
        public const int LegMask = 0b1 | (1 << 9);
        
        private const int IkIterations = 500;
        private const float PickupRange = 3.0f;
        private const float Smoothing = 0.4f;

        public Transform rootTransform, midTransform, tipTransform;
        public ParticleSystem stepDust;

        public SpiderController Spider { get; private set; }
        public Vector2? OverrideTargetLocal { get; set; }
        public bool Locked { get; set; }

        public Vector2 root, mid, tip;
        public Vector2 smoothedRoot, smoothedMid, smoothedTip;
        public Vector2 target;
        public Vector2 localRestPosition;
        public Vector2 anchoredPosition;
        public bool anchored;
        public Collider2D anchoredObject;

        public bool controlled;
        public Item heldItem;
        public bool forceNoAnchor;

        public SpiderLeg(SpiderController spider, Transform rootTransform, Transform midTransform, int i, bool controlled)
        {
            Spider = spider;
            this.rootTransform = rootTransform;
            this.midTransform = midTransform;
            this.controlled = controlled;

            tipTransform = midTransform.GetChild(0);
            stepDust = tipTransform.GetComponentInChildren<ParticleSystem>();

            var a = (i * 90.0f + 45.0f) * Mathf.Deg2Rad;
            localRestPosition = new Vector2(Mathf.Cos(a) * 2.0f, Mathf.Sin(a)).normalized;
        }

        public void OnEnable()
        {
            if (controlled)
            {
                heldItem = Object.Instantiate(Spider.spawnItem);
                heldItem.Bind(this);
            }
        }

        public void OnDisable()
        {
            if (heldItem)
            {
                heldItem.Bind(null);
                heldItem = null;
            }
        }
        
        public void FixedUpdate(bool forceNoAnchor)
        {
            this.forceNoAnchor = forceNoAnchor;
            root = Spider.Body.position;

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
                target = Spider.Body.position + Spider.ReachVector * Spider.LegTotalLength;
                Collide(ref target);
            }
            else
            {
                target = anchoredPosition;
            }
        }

        private void Collide(ref Vector2 end)
        {
            var hit = Physics2D.Linecast(root, end, LegMask);
            if (hit)
            {
                end = hit.point;
            }
        }

        private void UpdateAnchor()
        {
            var restPosition = Spider.Body.position + localRestPosition * Spider.LegTotalLength * 0.75f;

            if (Locked) return;
            if (forceNoAnchor)
            {
                anchored = false;
                anchoredPosition = restPosition;
                return;
            }
            
            if (anchored)
            {
                if ((anchoredPosition - Spider.Body.position).magnitude > Spider.LegTotalLength)
                {
                    anchored = false;
                }

                if (!Physics2D.OverlapPoint(anchoredPosition))
                {
                    anchored = false;
                }
                
                return;
            }

            anchoredPosition = restPosition;

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
                    stepDust.Play();
                    
                    best = score;
                    anchoredPosition = cast.point;
                    anchoredObject = cast.collider;
                    anchored = true;
                }
            }
        }

        private void UpdateHeldItem()
        {
            if (!controlled) return;
            if (!Spider.Reaching) return;

            if (heldItem)
            {
                target = heldItem.ModifyReachPosition(target) ?? target;

                if (Spider.Use)
                {
                    heldItem.Use(Spider.gameObject);
                }

                if (Spider.Drop)
                {
                    heldItem.Bind(null);
                    heldItem = null;
                }
            }
            else if (Spider.Drop)
            {
                PickupItem();
            }
        }

        private void PickupItem()
        {
            var center = (Vector2)Spider.transform.position;
            var best = (Item)null;
            foreach (var e in Physics2D.OverlapCircleAll(center, PickupRange))
            {
                var item = e.GetComponentInParent<Item>();
                if (!item) continue;

                if (!best)
                {
                    best = item;
                    continue;
                }

                if (((Vector2)item.transform.position - center).magnitude < ((Vector2)best.transform.position - center).magnitude)
                {
                    best = item;
                }
            }

            if (!best) return;

            heldItem = best;
            heldItem.Bind(this);
        }

        private void UpdateTransforms()
        {
            smoothedRoot = Vector2.Lerp(root, smoothedRoot, Smoothing);
            smoothedMid = Vector2.Lerp(mid, smoothedMid, Smoothing);
            smoothedTip = Vector2.Lerp(tip, smoothedTip, Smoothing);
            
            rootTransform.position = new Vector3(smoothedRoot.x, smoothedRoot.y, rootTransform.position.z);
            rootTransform.right = smoothedMid - smoothedRoot;

            midTransform.position = new Vector3(smoothedMid.x, smoothedMid.y, midTransform.position.z);
            midTransform.right = smoothedTip - smoothedMid;
        }

        private void IK()
        {
            var upperLength = Spider.LegUpperLength;
            var lowerLength = Spider.LegLowerLength;
            var rootTarget = Spider.Body.position;

            var flipped = false;
            for (var i = 0; i < IkIterations; i++)
            {
                tip = target;
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
                vb = target;

                rootTarget = vb;
                target = va;

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

            var up = Spider.transform.up;

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