using UnityEngine;

namespace Crabs.Utility
{
    public static class Extensions
    {
        public static Vector2 ToDirection(this float angleRadians) => new (Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
        public static float ToAngle(this Vector2 vector) => Mathf.Atan2(vector.y, vector.x);
    }
}