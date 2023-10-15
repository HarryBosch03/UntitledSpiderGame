using Crabs.Player;
using Crabs.Utility;
using UnityEngine;

namespace Crabs.Items
{
    [SelectionBase, DisallowMultipleComponent]
    public abstract class Item : MonoBehaviour
    {
        [HideInInspector] public Vector2 position;
        [HideInInspector] public float rotation;

        private Transform model;

        public SpiderLeg Binding { get; set; }

        protected virtual void Start()
        {
            model = transform.Find("Model");
            UpdatePosition();
        }

        protected virtual void FixedUpdate()
        {
            UpdatePosition();

            var flip = transform.up.y < 0.0f;
            model.localScale = new Vector3(1.0f, flip ? -1.0f : 1.0f, 1.0f);
        }

        private void UpdatePosition()
        {
            position = Binding.tip;
            rotation = (Binding.tip - Binding.mid).ToAngle();

            position = ModifyPosition(position) ?? position;
            rotation = ModifyRotation(rotation) ?? rotation;
            
            transform.position = position;
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotation * Mathf.Rad2Deg);
        }
        
        public abstract void PrimaryUse();
        public abstract void SecondaryUse();

        public virtual Vector2? ModifyReachPosition(Vector2 reachPosition) => null;
        
        public Item Instantiate(SpiderLeg binding)
        {
            var instance = Instantiate(this);
            instance.Binding = binding;
            return instance;
        }

        public Item Dispose()
        {
            Destroy(gameObject);
            return null;
        }

        public virtual Vector2? ModifyPosition(Vector2 position) => null;
        public virtual float? ModifyRotation(float rotation) => null;
    }
}