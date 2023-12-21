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

        public List<StatModification> statModifications = new();

        public void Rebuild()
        {
            finalStats = baseStats;

            foreach (var mod in statModifications.OrderBy(e => e.order))
            {
                baseStats = mod.modification(baseStats);
            }
        }

        public class StatModification
        {
            public Modification modification;
            public int order;
            
            public delegate SpiderStatSheet Modification(SpiderStatSheet input);

            public StatModification(Modification modification, int order)
            {
                this.modification = modification;
                this.order = order;
            }
        }
    }
}