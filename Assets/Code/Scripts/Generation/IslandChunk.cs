using System.Collections.Generic;
using Crabs.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace Crabs.Generation
{
    [RequireComponent(typeof(MeshFilter))]
    public class IslandChunk : MonoBehaviour, IDamagable
    {
        [SerializeField] private float terrainSoftness = 1.0f;
        public Vector2Int min = Vector2Int.one;
        public Vector2Int max = Vector2Int.one * 64;

        private IslandGenerator generator;
        private MapData mapData;
        private MeshFilter meshFilter;
        private CustomCollider2D collider;
        private Mesh mesh;
        private PhysicsShapeGroup2D shape;
        private bool dirty;

        private Dictionary<Vector2Int, Tile> tiles = new();

        public void Generate(MapData mapData)
        {
            this.mapData = mapData;
            
            Initialize();
            GenerateTerrain();
        }

        private void Initialize()
        {
            tiles.Clear();

            meshFilter = GetComponent<MeshFilter>();

            if (mesh) DestroyImmediate(mesh);

            mesh = new Mesh();
            mesh.name = "[PROC] IslandMeshinator.Mesh";

            meshFilter.sharedMesh = mesh;
        }

        private void GenerateTerrain()
        {
            for (var x = min.x; x < max.x; x++)
            for (var y = min.y; y < max.y; y++)
            {
                if (x < 0 || x >= mapData.width) continue;
                if (y < 0 || y >= mapData.height) continue;
                
                var v = mapData[x, y];
                if (v > 0.0f) continue;   
                var tile = new Tile(x, y);
                tiles.Add(new Vector2Int(x, y), tile);
            }
            GenerateMesh();
        }

        private void GenerateMesh()
        {
            var vertexCount = 0;
            var triCount = 0;
            shape = new PhysicsShapeGroup2D();

            foreach (var e in tiles)
            {
                var config = GetTileConfig(e.Key);
                var c = MarchingSquaresIndices[config].Length;
                vertexCount += c;
                if (c != 0) triCount += (c - 2) * 3;
            }

            var vertices = new Vector3[vertexCount];
            var uvs = new Vector2[vertexCount];
            var colors = new Color[vertexCount];

            var tris = new int[triCount];

            var vertexBasis = 0;
            var trisHead = 0;

            foreach (var keyValuePair in tiles)
            {
                var key = keyValuePair.Key;
                
                var config = GetTileConfig(key);
                var table = MarchingSquaresIndices[config];
                for (var j = 0; j < table.Length; j++)
                {
                    var tableIndex = table[^(1 + j)];
                    var interpolation = MarchingSquaresVertices[tableIndex];
                    var vertex = (Vector3)(new Vector2(key.x, key.y) + interpolation) * mapData.unitScale;
                    vertices[vertexBasis + j] = vertex;
                    uvs[vertexBasis + j] = interpolation;
                    colors[vertexBasis + j] = new Color(key.x / (mapData.width - 1.0f), key.y / (mapData.height - 1.0f), 0, 0);
                }

                for (var j = 0; j < table.Length - 2; j++)
                {
                    tris[trisHead++] = vertexBasis;
                    tris[trisHead++] = vertexBasis + j + 1;
                    tris[trisHead++] = vertexBasis + j + 2;
                }

                var edgeTable = MarchingSquaresEdges[config];
                if (edgeTable.Length >= 2)
                {
                    var polygon = new List<Vector2>();
                    foreach (var e in edgeTable)
                    {
                        var v = new Vector2(key.x, key.y) + MarchingSquaresVertices[e];
                        polygon.Add(v * mapData.unitScale);
                    }

                    shape.AddPolygon(polygon);
                }

                vertexBasis += table.Length;
            }

            mesh.Clear();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();

            SetCollider();
        }

        private int GetTileConfig(Vector2Int key)
        {
            var x = key.x;
            var y = key.y;

            if (x == mapData.width - 1) return 0;
            if (y == mapData.height - 1) return 0;

            var weights = new[]
            {
                mapData[x, y],
                mapData[x + 1, y],
                mapData[x + 1, y + 1],
                mapData[x, y + 1],
            };

            var config = 0;
            for (var i = 0; i < 4; i++)
            {
                if (weights[i] < 0.0f) config |= 0b1 << i;
            }

            return config;
        }

        private void SetCollider()
        {
            collider = GetComponent<CustomCollider2D>();
            if (!collider) return;

            collider.SetCustomShapes(shape);
        }

        private void FixedUpdate()
        {
            if (dirty)
            {
                dirty = false;
                mapData.Apply();
                GenerateMesh();
            }
        }

        public void Damage(int damage, Vector2 point, Vector2 direction)
        {
            point /= mapData.unitScale;
            Damage(damage, Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y));
            Damage(damage, Mathf.CeilToInt(point.x), Mathf.FloorToInt(point.y));
            Damage(damage, Mathf.FloorToInt(point.x), Mathf.CeilToInt(point.y));
            Damage(damage, Mathf.CeilToInt(point.x), Mathf.CeilToInt(point.y));
        }

        public void Damage(int damage, int x, int y)
        {
            mapData[x, y] += damage * terrainSoftness;
            dirty = true;
        }

        private static readonly Vector2[] MarchingSquaresVertices =
        {
            new(0.0f, 0.0f), // 0
            new(0.5f, 0.0f), // 1
            new(1.0f, 0.0f), // 2
            new(1.0f, 0.5f), // 3
            new(1.0f, 1.0f), // 4
            new(0.5f, 1.0f), // 5
            new(0.0f, 1.0f), // 6
            new(0.0f, 0.5f), // 7
        };

        private static readonly int[][] MarchingSquaresIndices =
        {
            new int [] { },                     // 0
            new int [] { 0, 1, 7 },             // 1
            new int [] { 3, 1, 2 },             // 2
            new int [] { 3, 7, 0, 2 },          // 3
            new int [] { 4, 5, 3 },             // 4
            new int [] { 4, 5, 7, 0, 1, 3 },    // 5
            new int [] { 4, 5, 1, 2 },          // 6
            new int [] { 4, 5, 7, 0, 2 },       // 7
            new int [] { 5, 6, 7 },             // 8
            new int [] { 5, 6, 0, 1 },          // 9
            new int [] { 5, 6, 7, 1, 2, 3 },    // 10
            new int [] { 5, 6, 0, 2, 3 },       // 11
            new int [] { 4, 6, 7, 3 },          // 12
            new int [] { 4, 6, 0, 1, 3 },       // 13
            new int [] { 4, 6, 7, 1, 2 },       // 14
            new int [] { 0, 2, 4, 6 },          // 14
        };

        private static readonly int[][] MarchingSquaresEdges =
        {
            new int [] { },                  // 0
            new int [] { 0, 1, 7 },          // 1
            new int [] { 1, 2, 3 },          // 2
            new int [] { 0, 2, 3, 7 },       // 3
            new int [] { 3, 4, 5 },          // 4
            new int [] { 0, 1, 3, 4, 5, 7 }, // 5
            new int [] { 1, 2, 4, 5 },       // 6
            new int [] { 0, 2, 4, 5, 7 },    // 7
            new int [] { 7, 5, 6 },          // 8
            new int [] { 0, 1, 5, 6 },       // 9
            new int [] { 1, 2, 3, 5, 6, 7 }, // 10
            new int [] { 0, 2, 3, 5, 6 },    // 11
            new int [] { 7, 3, 4, 6 },       // 12
            new int [] { 0, 1, 3, 4, 6 },    // 13
            new int [] { 1, 2, 4, 6, 7 },    // 14
            new int [] { 0, 2, 4, 6 },       // 15
        };

        public class Tile
        {
            public int x, y;

            public Tile(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
    }
}