using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UntitledSpiderGame.Runtime.Player;
using UntitledSpiderGame.Runtime.Spider;

namespace UntitledSpiderGame.Runtime.Mods
{
    public abstract class Mod : MonoBehaviour
    {
        protected SpiderController spider;

        private static readonly List<Type> RegisteredMods = new List<Type>();

        static Mod() { RegisteredMods.AddRange(typeof(Mod).Assembly.GetTypes().Where(e => e.IsSubclassOf(typeof(Mod)) && !e.IsAbstract)); }

        public static Mod Find(string name)
        {
            foreach (var type in RegisteredMods)
            {
                if (type.Name.ToLower().Trim() == name.ToLower().Trim())
                {
                    var instance = new GameObject($"{type.Name}").AddComponent(type);
                    return (Mod)instance;
                }
            }

            return null;
        }

        public static void Attach(SpiderController spider, string name)
        {
            if (!spider) return;
            var mod = Find(name);
            if (!mod) return;

            mod.Attach(spider);
        }

        // Order Modification will be executed, lowest to highest.
        protected virtual uint Order => uint.MaxValue;
        protected virtual void ModifyStats(ref SpiderStatSheet stats) { }

        private void Start()
        {
            if (!spider)
            {
                spider = GetComponentInParent<SpiderController>();
                if (spider) Attach(spider);
            }
        }

        public virtual void Attach(SpiderController target)
        {
            if (spider)
            {
                Detach();
            }

            spider = target;
            transform.SetParent(spider.transform);

            spider.stats.statModifications.Add(new SpiderStats.StatModification(ModifyStats, Order));
            Debug.Log($"Attached {GetType().Name} to {spider.name}");
        }

        public virtual void Detach()
        {
            spider = null;
            transform.SetParent(null);
            gameObject.SetActive(false);
        }
    }
}