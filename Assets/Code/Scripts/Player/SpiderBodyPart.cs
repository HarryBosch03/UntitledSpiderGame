using UnityEngine;
using UntitledSpiderGame.Runtime.Spider;

namespace UntitledSpiderGame.Runtime.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class SpiderBodyPart : MonoBehaviour
    {
        public Vector3 leftPosition;
        public Vector3 leftRotation;
        
        [Space]
        public Vector3 rightPosition;
        public Vector3 rightRotation;
        
        [Space]
        [Range(0.0f, 1.0f)] public float smoothing;

        [Range(-1.0f, 1.0f)] public float lean = 1.0f;
        
        
        private SpiderController controller;

        private void Awake() { controller = GetComponentInParent<SpiderController>(); }

        private void FixedUpdate()
        {
            var direction = controller.Direction;
            lean = Mathf.Lerp(direction, lean, smoothing);

            ApplyLean();
        }

        private void ApplyLean()
        {
            var t = Mathf.InverseLerp(-1.0f, 1.0f, lean);
            transform.localPosition = Vector3.Lerp(leftPosition, rightPosition, t);
            transform.localRotation = Quaternion.Slerp(Quaternion.Euler(leftRotation), Quaternion.Euler(rightRotation), t);
        }

        private void Reset()
        {
            rightPosition = transform.localPosition;
            leftPosition = new Vector3(-rightPosition.x, rightPosition.y, rightPosition.z);

            rightRotation = transform.eulerAngles;
            leftRotation = rightRotation;
        }

        private void OnValidate()
        {
            ApplyLean();
        }
    }
}
