using Crabs.Extras;
using Crabs.Generation;
using Crabs.Generation.Tiles;
using UnityEngine;

namespace Crabs.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class WebProjectile : Projectile
    {
        public WebTile webTile;

        protected override void ProcessHit(RaycastHit2D hit)
        {
            base.ProcessHit(hit);

            var island = hit.collider.GetComponentInParent<IslandChunk>();
            if (!island) return;
            
            island.SetTile(webTile, hit.point);
        }
    }
}
