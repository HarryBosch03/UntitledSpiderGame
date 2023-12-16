using Crabs.Extras;
using UnityEngine;

namespace Crabs.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class WebProjectile : Projectile
    {
        public Web web;

        private Vector2 webPosition;

        private void Start()
        {
            web = Instantiate(web);
            web.StartWeb(transform.position);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            web.Catchup(transform.position, velocity);
        }
    }
}
