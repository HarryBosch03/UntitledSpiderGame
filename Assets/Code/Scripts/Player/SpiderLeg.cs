using UnityEngine;

namespace UntitledSpiderGame.Runtime.Player
{
    public class SpiderLeg
    {
        public const int LegMask = 0b1 | (1 << 9);

        private const int IkIterations = 500;
        private const float Smoothing = 0.4f;

        public Transform rootTransform, midTransform, tipTransform;
        public ParticleSystem stepDust;

        public SpiderController Spider { get; }
        public Vector2? OverridePosition { get; set; }
        public Vector2? OverrideDirection { get; set; }
        public bool Locked { get; set; }

        public Vector2 root, mid, tip;
        public Vector2 smoothedRoot, smoothedMid, smoothedTip;
        public Vector2 target;
        public Vector2 localRestPosition;
        public Vector2 anchoredPosition;
        public bool anchored;
        public float length0, length1, lengthTotal;

        public bool forceNoAnchor;

        public SpiderLeg(SpiderController spider, Transform rootTransform, Transform midTransform, int i, bool controlled)
        {
            Spider = spider;
            this.rootTransform = rootTransform;
            this.midTransform = midTransform;

            tipTransform = midTransform.GetChild(0);
            stepDust = tipTransform.GetComponentInChildren<ParticleSystem>();

            var a = (i * 90.0f + 45.0f) * Mathf.Deg2Rad;
            localRestPosition = new Vector2(Mathf.Cos(a) * 2.0f, Mathf.Sin(a)).normalized;

            length0 = (rootTransform.position - midTransform.position).magnitude;
            length1 = (midTransform.position - tipTransform.position).magnitude;
            lengthTotal = length0 + length1;
        }

        public void FixedUpdate(bool forceNoAnchor)
        {
            this.forceNoAnchor = forceNoAnchor;
            root = Spider.Body.position;

            UpdateAnchor();
            UpdateTarget();

            IK();
            UpdateTransforms();
        }

        private void UpdateTarget()
        {
            if (OverridePosition.HasValue)
            {
                anchored = false;
                target = OverridePosition.Value;
                return;
            }

            target = anchoredPosition;
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
            if (Locked) return;

            var restPosition = Spider.Body.position + localRestPosition * Spider.LegTotalLength * 0.75f;

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
                    best = score;
                    anchoredPosition = cast.point;
                    anchored = true;
                }
            }

            if (anchored)
            {
                stepDust.Play();
            }
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

            tipTransform.right = OverrideDirection ?? midTransform.right;
        }

        private void IK()
        {
            var upperLength = Spider.LegUpperLength;
            var lowerLength = Spider.LegLowerLength;
            var rootTarget = Spider.Body.position;

            mid = Spider.transform.position + Spider.transform.up;

            var flipped = false;
            for (var i = 0; i < IkIterations; i++)
            {
                tip = target;
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
    }
}