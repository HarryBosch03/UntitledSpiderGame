using UnityEngine;

namespace Crabs.Generation.Tiles
{
    [System.Serializable]
    public class WebTile : Tile
    {
        private int crumble = -1;

        public override bool Damage(int damage)
        {
            var res = base.Damage(damage);
            if (!res) return false;

            Crumble(data, 0, 1);
            Crumble(data, 0, -1);
            Crumble(data, 1, 0);
            Crumble(data, -1, 0);

            return true;
        }

        private void Crumble(MapData data, int xo, int yo)
        {
            if (Random.value > 0.5f) return;

            var x = this.x + xo;
            var y = this.y + yo;

            var tile = data[x, y];
            if (tile is not WebTile webTile) return;

            webTile.crumble = 1;
        }

        public override void Tick()
        {
            if (crumble == 0)
            {
                data.Damage(1, x, y);
            }
            else
            {
                crumble--;
            }
        }
    }
}