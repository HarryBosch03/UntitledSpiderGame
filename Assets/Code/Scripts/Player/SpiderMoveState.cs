namespace Crabs.Player
{
    public class SpiderMoveState : SpiderState
    {
        public override void FixedUpdate(SpiderController spider)
        {
            spider.Move(spider.Input.MoveDirection);
            spider.Reaching = spider.Input.Reaching;
        }
    }
}