using UnityEngine;

namespace Crabs.Generation.Tiles
{
    [System.Serializable]
    public class Tile
    {
        public Color color;
        private int health;
        public int x, y;
        public MapData data;

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

        public virtual void Tick() { }

        public virtual bool Damage(int damage)
        {
            if (health <= 0) return true;
            
            health -= damage;
            return health <= 0;
        }

        public Tile Clone() => (Tile)MemberwiseClone();
    }
}