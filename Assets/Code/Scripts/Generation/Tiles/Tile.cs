using UnityEngine;

namespace UntitledSpiderGame.Runtime.Generation.Tiles
{
    [System.Serializable]
    public class Tile
    {
        public Color color;
        public int health;

        public Tile()
        {
            color = Color.magenta;
            health = 1;
        }
        
        public Tile(Color color, int health)
        {
            this.color = color;
            this.health = health;
        }
        
        public Tile(Tile tile) : this(tile.color, tile.health) { }
    }
}