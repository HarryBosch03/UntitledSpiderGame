using System.Collections.Generic;
using UnityEngine;

namespace Crabs.Generation
{
    [RequireComponent(typeof(IslandGenerator))]
    public class IslandChunkDistributor : MonoBehaviour
    {
        public IslandChunk chunkPrefab;
        public int chunkSize;

        private IslandGenerator generator;
        private Dictionary<Vector2Int, IslandChunk> chunks = new();

        public IslandChunk Chunk(Vector2Int key) => chunks.ContainsKey(key) ? chunks[key] : null;

        private void OnEnable()
        {
            generator = GetComponent<IslandGenerator>();
            var mapData = generator.GenerateMap();

            for (var y = 0; y < generator.height / chunkSize; y++)
            for (var x = 0; x < generator.width / chunkSize; x++)
            {
                var key = new Vector2Int(x, y);
                var chunk = Instantiate(chunkPrefab, transform);
                chunks.Add(key, chunk);

                var min = new Vector2Int(x, y) * chunkSize;
                var max = min + Vector2Int.one * chunkSize;
                chunk.Generate(this, key, min, max, mapData);
            }
        }
        
        private void OnDisable()
        {
            foreach (var e in chunks) Destroy(e.Value.gameObject);
            chunks.Clear();
        }
    }
}