using UnityEngine;

namespace UntitledSpiderGame.Runtime.Player
{
    [System.Serializable]
    public struct SpiderStatSheet
    {
        [Header("Weapon")]
        public float damage;
        public float knockback;
        public float bulletSize;
        public float bulletLifetime;
        public int bounces;
        public int fractures;
        public int ammo;
        public int projectilesPerShot;
        public float spreadTangent;
        public float reloadTime;
        public float bulletSpeed;
        public float attackSpeed;
        public int automatic;
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
            damage = 45,
            knockback = 10.0f,
            bulletSize = 1.0f,
            bulletLifetime = 60.0f,
            ammo = 7,
            bounces = 0,
            fractures = 0,
            projectilesPerShot = 1,
            spreadTangent = 0.0f,
            reloadTime = 1.0f,
            bulletSpeed = 35,
            attackSpeed = 10.0f,
            automatic = 0,
            recoilResponse = 1.2f,
            recoilDecay = 45.0f,
            recoilForce = 7.0f,
            moveSpeed = 10.0f,
            health = 100,
            webRange = 30.0f,
            webForce = 200.0f,
        };
    }
}