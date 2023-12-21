using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UntitledSpiderGame.Runtime.Generation.Tiles;
using UntitledSpiderGame.Runtime.Player;
using Object = UnityEngine.Object;

namespace UntitledSpiderGame.Runtime.Generation
{
    public class MapData
    {
        public readonly float unitScale;
        public readonly int width;
        public readonly int height;

        public Tile[] tiles;
        public Texture2D texture;
        private static readonly int MapWeights = Shader.PropertyToID("_MapWeights");
        private static readonly int MapWidth = Shader.PropertyToID("_MapWidth");
        private static readonly int MapHeight = Shader.PropertyToID("_MapHeight");

        public MapData(int width, int height, float unitScale)
        {
            this.width = width;
            this.height = height;
            this.unitScale = unitScale;
            tiles = new Tile[width * height];
            
            texture = new Texture2D(width, height, DefaultFormat.HDR, TextureCreationFlags.None);
            texture.hideFlags = HideFlags.HideAndDontSave;
        }

        ~MapData()
        {
            if (texture) Object.Destroy(texture);
        }

        public MapData Apply()
        {
            texture.Apply();
            
            Shader.SetGlobalTexture(MapWeights, texture);
            Shader.SetGlobalInt(MapWidth, width);
            Shader.SetGlobalInt(MapHeight, height);
            return this;
        }

        public Tile this[int x, int y]
        {
            get => tiles[x + y * width];
            set
            {
                tiles[x + y * width] = value;
                texture.SetPixel(x, y, value?.color ?? Color.clear);
            }
        }

        public void Enumerate(Action<int, int, Tile> callback)
        {
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                callback(x, y, this[x, y]);
            }
        }

        public void Damage(float damage, int x, int y)
        {
            var tile = this[x, y];
            if (tile == null) return;
            
            tile.health -= IDamagable.Round(damage);
            if (tile.health <= 0) this[x, y] = null;
        }
    }
}