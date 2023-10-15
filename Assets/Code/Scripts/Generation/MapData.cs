using System;

namespace Crabs.Generation
{
    public class MapData
    {
        public readonly float unitScale;
        public readonly int width;
        public readonly int height;

        public float[,] weights;

        public MapData(int width, int height, float unitScale)
        {
            this.width = width;
            this.height = height;
            this.unitScale = unitScale;

            weights = new float[width, height];
        }

        public float this[int x, int y]
        {
            get => weights[x, y];
            set => weights[x, y] = value;
        }

        public void Enumerate(Action<int, int, float> callback)
        {
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                callback(x, y, weights[x, y]);
            }
        }
    }
}