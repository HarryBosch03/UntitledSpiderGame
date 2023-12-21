using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UntitledSpiderGame.Runtime.Player
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Spider Stats")]
    public class SpiderStats : ScriptableObject
    {
        public SpiderStatSheet baseStats = SpiderStatSheet.Defaults;
        public SpiderStatSheet finalStats;

        public readonly List<StatModification> statModifications = new();

        public void Rebuild()
        {
            finalStats = baseStats;

            foreach (var mod in statModifications.OrderBy(e => e.order))
            {
                mod.modification(ref finalStats);
            }
        }

        public class StatModification
        {
            public Modification modification;
            public uint order;
            
            public delegate void Modification(ref SpiderStatSheet stats);

            public StatModification(Modification modification, uint order)
            {
                this.modification = modification;
                this.order = order;
            }
        }
    }
}