using UnityEngine;

namespace UntitledSpiderGame.Runtime.Player
{
    [System.Serializable]
    public struct SpiderStatSheet
    {
        [Header("Weapon")]
        public int damage;
        public float knockback;
        public int ammo;
        public float reloadTime;
        public float bulletSpeed;
        public float attackSpeed;
        public bool automatic;
        public float recoilResponse;
        public float recoilDecay;
        public float recoilForce;
        [Header("Movement")]
        public float moveSpeed;
        [Header("Health")]
        public int health;
        [Header("Webs")]
        public float webRange;
        public float webForce;

        public static readonly SpiderStatSheet Defaults = new()
        {
            damage = 1,
            knockback = 10.0f,
            ammo = 7,
            reloadTime = 1.0f,
            bulletSpeed = 35,
            attackSpeed = 50.0f,
            automatic = false,
            recoilResponse = 1.2f,
            recoilDecay = 45.0f,
            recoilForce = 7.0f,
            moveSpeed = 10.0f,
            health = 3,
            webRange = 30.0f,
            webForce = 200.0f,
        };
    }
}