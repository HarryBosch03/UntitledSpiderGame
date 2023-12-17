using System;
using Crabs.Generation.Tiles;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace Crabs.Generation
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

        public static event Action<MapData, int, int> ChangeEvent;
        
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
            get
            {
                if (x < 0 || x >= width) return null;
                if (y < 0 || y >= height) return null;

                return tiles[x + y * width];
            }
            set
            {
                tiles[x + y * width] = value;
                texture.SetPixel(x, y, value?.color ?? Color.clear);

                if (value == null) return;
                
                value.x = x;
                value.y = y;
                value.data = this;
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

        public void Damage(int damage, int x, int y)
        {
            var tile = this[x, y];
            if (tile == null) return;

            if (tile.Damage(damage)) this[x, y] = null;
            ChangeEvent?.Invoke(this, x, y);
        }
    }
}