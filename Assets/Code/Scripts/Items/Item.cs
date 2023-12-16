using Crabs.Player;
using Crabs.Utility;
using UnityEngine;

namespace Crabs.Items
{
    [RequireComponent(typeof(Rigidbody2D))]
    [SelectionBase, DisallowMultipleComponent]
    public abstract class Item : MonoBehaviour
    {
        private const float PhantomDespawnTime = 1.0f;
        
        [HideInInspector] public Vector2 position;
        [HideInInspector] public float rotation;
        [SerializeField] private float itemScale = 1.0f;

        private Transform model;
        private Transform poof;
        private Rigidbody2D body;
        public bool phantomItem;
        public float phantomDespawnTimer;

        public SpiderLeg Binding { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            poof = transform.Find("Poof");
            if (poof) poof.gameObject.SetActive(false);

            foreach (var t in GetComponentsInChildren<Transform>())
            {
                // 8: Item Layer
                t.gameObject.layer = 8;
            }
        }

        protected virtual void Start()
        {
            model = transform.Find("Model");
            UpdatePosition();
        }

        protected virtual void FixedUpdate()
        {
            UpdatePosition();

            var flip = transform.up.y < 0.0f;
            model.localScale = new Vector3(1.0f, flip ? -1.0f : 1.0f, 1.0f) * itemScale;

            if (Binding == null && phantomItem)
            {
                phantomDespawnTimer += Time.deltaTime;
                if (phantomDespawnTimer > PhantomDespawnTime)
                {
                    poof.gameObject.SetActive(true);
                    poof.SetParent(null);
                    Destroy(gameObject);
                }
            }
        }

        public virtual void Bind(SpiderLeg binding)
        {
            Binding = binding;
            body.isKinematic = binding != null;
            if (binding == null)
            {
                body.rotation = 0.0f;
            }
        }
        
        private void UpdatePosition()
        {
            if (Binding != null)
            {
                position = Binding.tip;
                rotation = (Binding.tip - Binding.mid).ToAngle();

                position = ModifyPosition(position) ?? position;
                rotation = ModifyRotation(rotation) ?? rotation;

                transform.position = position;
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotation * Mathf.Rad2Deg);
            }
        }

        public abstract void Use(GameObject user);

        public virtual Vector2? ModifyReachPosition(Vector2 reachPosition) => null;

        public Item Dispose()
        {
            Destroy(gameObject);
            return null;
        }

        public virtual Vector2? ModifyPosition(Vector2 position) => null;
        public virtual float? ModifyRotation(float rotation) => null;
    }
}