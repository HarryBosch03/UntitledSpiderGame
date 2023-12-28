using UntitledSpiderGame.Runtime.Player;

namespace UntitledSpiderGame.Runtime.Mods.Weapons
{
    public class Shrapnel : Mod
    {
        protected override void ModifyStats(ref SpiderStatSheet stats)
        {
            stats.bounces += 2;
            stats.fractures++;
        }
    }
}