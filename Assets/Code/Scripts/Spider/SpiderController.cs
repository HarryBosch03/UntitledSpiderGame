using System;
using System.Collections.Generic;
using UnityEngine;
using UntitledSpiderGame.Runtime.Items;
using UntitledSpiderGame.Runtime.Player;

namespace UntitledSpiderGame.Runtime.Spider
{
    [RequireComponent(typeof(Rigidbody2D))]
    [SelectionBase, DisallowMultipleComponent]
    public sealed class SpiderController : MonoBehaviour
    {
        public SpiderStats stats;
        public Item startingItem;

        public readonly List<Renderer> coloredBodyParts = new();
        
        public static readonly List<SpiderController> All = new();

        #region Input

        public bool Reaching { get; set; }
        public Vector2 ReachVector { get; set; }
        public Vector2 MoveDirection { get; set; }
        public bool Jump { get; set; }
        public bool Web { get; set; }

        #endregion

        public List<SpiderLeg> Legs { get; } = new();
        public SpiderLeg ArmLeg => Legs[0];
        public Vector2 GroundPoint { get; set; }
        public Rigidbody2D Body { get; private set; }
        public int Direction { get; set; } = 1;
        public bool Anchored { get; set; }

        private void Awake()
        {
            Body = GetComponent<Rigidbody2D>(); 
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                if (!r.CompareTag("SpiderColoredRenderer")) continue;
                coloredBodyParts.Add(r);
            }
            
            if (!stats)
            {
                stats = ScriptableObject.CreateInstance<SpiderStats>();
            }
        }

        private void OnEnable() { All.Add(this); }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void Start()
        {
            var item = Instantiate(startingItem);
            ArmLeg.SetItem(item);
        }

        private void FixedUpdate()
        {
            stats.Rebuild();
        }

        public void SetColor(Color color)
        {
            var propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_MainColor", color);
            foreach (var e in coloredBodyParts) e.SetPropertyBlock(propertyBlock);

            foreach (var e in GetComponentsInChildren<ColorWithSpider>(true))
            {
                e.SetColor(color);
            }
        }

    }
}