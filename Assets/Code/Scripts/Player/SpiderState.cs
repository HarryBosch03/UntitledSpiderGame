namespace Crabs.Player
{
    public abstract class SpiderState
    {
        public virtual void Enter(SpiderController spider) { }
        public virtual void FixedUpdate(SpiderController spider) { }
        public virtual void Exit(SpiderController spider) { }
    }
}