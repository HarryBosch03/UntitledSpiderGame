using UnityEngine;

namespace Crabs.Utility
{
    public static class Noise
    {
        public static int seed;
        
        public static float Raw(float x) => Raw(new Vector3(x, 0.0f, 0.0f));
        public static float Raw(float x, float y) => Raw(new Vector3(x, y, 0.0f));
        public static float Raw(Vector3 p)
        {
            var smallValue = new Vector3(Mathf.Sin(p.x), Mathf.Sin(p.y) ,Mathf.Sin(p.z));
            var random = Vector3.Dot(smallValue, new Vector3(12.9898f, 78.233f, 37.719f));
            random += seed;
            random = (Mathf.Sin(random) * 143758.5453f) % 1.0f;
            return random;
        }

        public static float Perlin(float x)
        {
            var x0 = Mathf.Floor(x);
            var x1 = x0 + 1;
            var xp = x - x0;

            var n0 = Raw(x0);
            var n1 = Raw(x1);

            return Mathf.Lerp(n0, n1, Smootherstep(xp));
        }

        private static float Smootherstep(float x)
        {
            return x * x * x * (x * (6.0f * x - 15.0f) + 10.0f);
        }
    }
}