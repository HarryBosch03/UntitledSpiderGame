using UnityEngine;

namespace Crabs.Player
{
    public class SpiderBodyPart
    {
        private Transform target;
        private Vector3 basis;

        private Renderer renderer;

        public bool flip = true;
        public bool color = true;
        
        public SpiderBodyPart(Transform target)
        {
            this.target = target;
            basis = target.localPosition;

            renderer = target.GetComponentInChildren<Renderer>();
        }

        public void FixedUpdate(int direction, float smoothing)
        {
            Flip(direction, smoothing);
        }

        private void Flip(int direction, float smoothing)
        {
            if (!flip) return;
            direction = direction >= 0 ? 1 : -1;
            target.localPosition = Vector3.Lerp(new Vector3(basis.x * direction, basis.y, basis.z), target.localPosition, smoothing);
        }

        public void SetMaterialPropertyBlock(MaterialPropertyBlock propertyBlock)
        {
            if (!color) return;
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}