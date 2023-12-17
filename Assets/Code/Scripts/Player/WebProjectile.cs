using Crabs.Extras;
using UnityEngine;

namespace Crabs.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class WebProjectile : Projectile
    {
        public Web web;

        public Vector2? anchor;

        private void Start()
        {
            if (anchor.HasValue)
            {
                web = Instantiate(web);
                web.StartWeb(anchor.Value);
            }
        }


        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            web.Catchup(transform.position, velocity);
        }
    }
}