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
        private List<IslandChunk> chunks = new();
        
        private void OnEnable()
        {
            generator = GetComponent<IslandGenerator>();
            var mapData = generator.GenerateMap();
            
            for (var x = 0; x < generator.width; x += chunkSize)
            for (var y = 0; y < generator.height; y += chunkSize)
            {
                var chunk = Instantiate(chunkPrefab, transform);
                chunks.Add(chunk);

                chunk.min = new Vector2Int(x, y);
                chunk.max = chunk.min + Vector2Int.one * chunkSize;
                chunk.Generate(mapData);
            }
        }

        private void OnDisable()
        {
            foreach (var e in chunks) Destroy(e.gameObject);
        }
    }
}
