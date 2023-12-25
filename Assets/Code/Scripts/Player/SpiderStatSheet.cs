using UnityEngine;

namespace UntitledSpiderGame.Runtime.Player
{
    [System.Serializable]
    public struct SpiderStatSheet
    {
        [Header("Movement")]
        public float moveSpeed;
        [Header("Health")]
        public int health;
        [Header("Webs")]
        public float webRange;
        public float webForce;

        public static readonly SpiderStatSheet Defaults = new()
        {
            moveSpeed = 10.0f,
            health = 100,
            webRange = 1000.0f,
            webForce = 200.0f,
        };
    }
}