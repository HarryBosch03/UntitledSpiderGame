using UntitledSpiderGame.Runtime.Player;

namespace UntitledSpiderGame.Runtime.Mods.Weapons
{
    public class Fastball : Mod
    {
        protected override void ModifyStats(ref SpiderStatSheet stats)
        {
            stats.bulletSpeed *= 3.0f;
            stats.attackSpeed *= 0.8f;
        }
    }
}