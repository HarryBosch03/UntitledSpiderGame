using UntitledSpiderGame.Runtime.Player;

namespace UntitledSpiderGame.Runtime.Mods.Weapons
{
    public class Shotgun : Mod
    {
        protected override void ModifyStats(ref SpiderStatSheet stats)
        {
            stats.damage *= 0.4f;
            stats.knockback *= 2f;
            stats.bulletSize *= 0.5f;
            stats.bulletLifetime *= 0.01f / 60.0f;
            stats.ammo -= 5;
            stats.projectilesPerShot += 7;
            stats.spreadTangent += 0.6f;
            stats.bulletSpeed *= 2.0f;
            stats.attackSpeed *= 2.0f;
        }
    }
}