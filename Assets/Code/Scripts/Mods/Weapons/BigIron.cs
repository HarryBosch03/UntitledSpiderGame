using UntitledSpiderGame.Runtime.Player;

namespace UntitledSpiderGame.Runtime.Mods.Weapons
{
    public class BigIron : Mod
    {
        protected override void ModifyStats(ref SpiderStatSheet stats)
        {
            stats.ammo--;
            stats.damage *= 3.0f;
            stats.attackSpeed *= 0.25f;
            stats.bulletSpeed *= 2.5f;
        }
    }
}