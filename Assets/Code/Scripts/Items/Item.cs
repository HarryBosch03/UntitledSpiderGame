using UnityEngine;
using UntitledSpiderGame.Runtime.Extras;
using UntitledSpiderGame.Runtime.Utility;

namespace UntitledSpiderGame.Runtime.Items
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Item : MonoBehaviour
    {
        private Rigidbody2D body;
        private Vector2 velocity;
        
        public bool Input { get; set; }

        protected Rigidbody2D bindingBody;
        protected Transform bindingTarget;

        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (bindingTarget)
            {
                var lastPosition = transform.position;
                
                transform.position = bindingTarget.position;
                transform.rotation = bindingTarget.rotation;

                velocity = (bindingTarget.position - lastPosition) / Time.deltaTime;
            }

            if (Input)
            {
                Use();
            }
        }

        protected abstract void Use();
        
        public void Bind(Rigidbody2D other, Transform target)
        {
            bindingTarget = target;
            bindingBody = other;
            body.simulated = false;
        }
        
        public void Unbind()
        {
            body.simulated = true;
            body.velocity = velocity;
            body.angularVelocity = 0.0f;
            
            bindingTarget = null;
            bindingBody = null;
        }
    }
}