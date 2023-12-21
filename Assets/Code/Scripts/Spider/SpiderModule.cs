using UnityEngine;
using UntitledSpiderGame.Runtime.Player;

namespace UntitledSpiderGame.Runtime.Spider
{
    public abstract class SpiderModule : MonoBehaviour
    {
        public SpiderController Spider { get; private set; }
        public SpiderStatSheet Stats => Spider.stats.finalStats;

        protected virtual void Awake()
        {
            Spider = GetComponent<SpiderController>();
        }
    }
}