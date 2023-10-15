using UnityEngine;
using UnityEngine.Serialization;

namespace Crabs.Generation
{
    [System.Serializable]
    public class FractalNoise
    {
        public float scale = 16.0f;
        public int octaves = 4;
        public float lacunarity = 2.0f;
        public float persistence = 0.5f;

        public float Sample(float x, float y)
        {
            var v = 0.0f;
            var m = 0.0f;

            octaves = Mathf.Max(octaves, 1);
            for (var i = 0; i < octaves; i++)
            {
                var f = Mathf.Pow(lacunarity, i) / scale;
                var a = Mathf.Pow(persistence, i);

                v += SampleSingle(x * f, y * f) * a;
                m += a;
            }

            return v / m;
        }

        public float SampleSingle(float x, float y)
        {
            var x0 = Mathf.FloorToInt(x);
            var y0 = Mathf.FloorToInt(y);

            var x1 = x0 + 1;
            var y1 = y0 + 1;

            var px = x - x0;
            var py = y - y0;
            
            var d0 = RandomVectorDot(x0, y0, x, y);
            var d1 = RandomVectorDot(x1, y0, x, y);
            var d2 = RandomVectorDot(x0, y1, x, y);
            var d3 = RandomVectorDot(x1, y1, x, y);

            return Lerp(Lerp(d0, d1, px), Lerp(d2, d3, px), py);
        }

        public float Lerp(float a, float b, float t)
        {
            return (b - a) * ((t * (t * 6.0f - 15.0f) + 10.0f) * t * t * t) + a;
        }

        public float Random(int x, int y)
        {
            var smallValue = new Vector2(Mathf.Sin(x), Mathf.Sin(y));
            var random = (smallValue.x * 12.9898f + smallValue.y * 78.233f);
            random = (Mathf.Sin(random) * 143758.5453f) % 1.0f;
            return random;
        }

        public Vector2 RandomVector(int x0, int y0)
        {
            var a = Random(x0, y0) * Mathf.PI * 2.0f;
            return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        }

        public float RandomVectorDot(int x0, int y0, float x, float y)
        {
            var v = RandomVector(x0, y0);
            
            var px = x - x0;
            var py = y - y0;

            return px * v.x + py * v.y;
        }
    }
}