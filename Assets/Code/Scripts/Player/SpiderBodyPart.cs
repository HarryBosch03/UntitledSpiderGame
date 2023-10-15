using UnityEngine;

namespace Crabs.Player
{
    public class SpiderBodyPart
    {
        private Transform target;
        private Vector3 basis;

        public SpiderBodyPart(Transform target)
        {
            this.target = target;
            basis = target.localPosition;
        }

        public void FixedUpdate(int direction, float smoothing)
        {
            direction = direction >= 0 ? 1 : -1;
            target.localPosition = Vector3.Lerp(new Vector3(basis.x * direction, basis.y, basis.z), target.localPosition, smoothing);
        }
    }
}