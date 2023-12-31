using UnityEngine;
using UntitledSpiderGame.Runtime.Generation.Tiles;

namespace UntitledSpiderGame.Runtime.Generation
{
    [SelectionBase, DisallowMultipleComponent]
    public sealed class IslandGenerator : MonoBehaviour
    {
        public int width;
        public int height;
        public Tile tile;
        [SerializeField] private float unitScale = 1.0f;
        [SerializeField] private float noiseAmplitude;
        [SerializeField] private AnimationCurve heightCurve = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);
        [SerializeField] private float verticalOffset = 0.0f;
        [SerializeField] private AnimationCurve deadzone = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);
        [SerializeField] private FractalNoise noise;

        [HideInInspector] public bool isDirty;

        private void OnValidate()
        {
            isDirty = true;
        }

        public MapData GenerateMap()
        {
            var map = new MapData(width, height, unitScale);

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var px = x / (width - 1.0f);
                var py = y / (height - 1.0f);

                var verticalOffset = heightCurve.Evaluate(px) + this.verticalOffset;

                var value = (py - verticalOffset) / noiseAmplitude;
                value += noise.Sample(x, y) * noiseAmplitude;

                var deadzone = this.deadzone.Evaluate(px);
                value = Mathf.Lerp(0.1f, value, deadzone);

                if (value > 0.0f) continue;
                map[x, y] = new Tile(tile);
            }

            return map.Apply();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            var size = new Vector2(width, height) * unitScale;
            Gizmos.DrawWireCube(size / 2.0f, size);
        }
    }
}