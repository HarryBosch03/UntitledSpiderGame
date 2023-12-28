using UntitledSpiderGame.Runtime.Player;

namespace UntitledSpiderGame.Runtime.Mods.Weapons
{
    public class Spray : Mod
    {
        protected override void ModifyStats(ref SpiderStatSheet stats)
        {
            stats.automatic++;
            stats.ammo += 16;
            stats.attackSpeed *= 2.0f;
            stats.damage *= 0.4f;
            stats.bulletSize *= 0.5f;
            stats.reloadTime += 0.5f;
            stats.spreadTangent += 0.15f;
            stats.recoilForce *= 0.2f;
        }
    }
}