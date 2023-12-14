using System;
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

        public float[] weights;
        public Texture2D texture;
        private static readonly int MapWeights = Shader.PropertyToID("_MapWeights");
        private static readonly int MapWidth = Shader.PropertyToID("_MapWidth");
        private static readonly int MapHeight = Shader.PropertyToID("_MapHeight");

        public MapData(int width, int height, float unitScale)
        {
            this.width = width;
            this.height = height;
            this.unitScale = unitScale;

            weights = new float[width * height];
            
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

        public float this[int x, int y]
        {
            get => weights[x + y * width];
            set
            {
                weights[x + y * width] = value;
                texture.SetPixel(x, y, Color.white * -value);
            }
        }

        public void Enumerate(Action<int, int, float> callback)
        {
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                callback(x, y, this[x, y]);
            }
        }
    }
}