using System.Collections.Generic;
using Crabs.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace Crabs.Generation
{
    [RequireComponent(typeof(MeshFilter))]
    public class IslandMeshinator : MonoBehaviour, IDamagable
    {
        [SerializeField] private bool generateEditor;
        [SerializeField] private float terrainSoftness = 1.0f;

        private IslandGenerator generator;
        private MapData mapData;
        private MeshFilter meshFilter;
        private CustomCollider2D collider;
        private Mesh mesh;
        private PhysicsShapeGroup2D shape;
        private bool dirty;

        private Texture2D mapTexture;

        private List<Tile> tiles = new();

        private void Awake()
        {
            Initialize();
            GenerateMesh();
        }

        private void OnValidate()
        {
            if (!generateEditor) return;

            generateEditor = false;
            Initialize();
            GenerateMesh();
        }

        private void Initialize()
        {
            generator = GetComponentInParent<IslandGenerator>();
            mapData = generator.GenerateMap();

            meshFilter = GetComponent<MeshFilter>();

            if (mesh) DestroyImmediate(mesh);

            mesh = new Mesh();
            mesh.name = "[PROC] IslandMeshinator.Mesh";

            meshFilter.sharedMesh = mesh;
        }

        private void GenerateMesh()
        {
            shape = new PhysicsShapeGroup2D();
            mapData.Enumerate((x, y, _) =>
            {
                var tile = new Tile(x, y, 0);
                UpdateTile(tile);
                tiles.Add(tile);
            });
            Recompile();
        }

        private void Recompile()
        {
            var vertexCount = 0;
            var triCount = 0;
            shape = new PhysicsShapeGroup2D();

            foreach (var e in tiles)
            {
                var c = MarchingSquaresIndices[e.config].Length;
                vertexCount += c;
                if (c != 0) triCount += (c - 2) * 3;
            }

            var vertices = new Vector3[vertexCount];
            var uvs = new Vector2[vertexCount];
            var colors = new Color[vertexCount];

            var tris = new int[triCount];

            var vertexBasis = 0;
            var trisHead = 0;

            for (var i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                var table = MarchingSquaresIndices[tile.config];
                for (var j = 0; j < table.Length; j++)
                {
                    var tableIndex = table[^(1 + j)];
                    var interpolation = MarchingSquaresVertices[tableIndex];
                    var vertex = (Vector3)(new Vector2(tile.x, tile.y) + interpolation) * mapData.unitScale;
                    vertices[vertexBasis + j] = vertex;
                    uvs[vertexBasis + j] = interpolation;
                    colors[vertexBasis + j] = new Color(tile.x / (mapData.width - 1.0f), tile.y / (mapData.height - 1.0f), 0, 0);
                }

                for (var j = 0; j < table.Length - 2; j++)
                {
                    tris[trisHead++] = vertexBasis;
                    tris[trisHead++] = vertexBasis + j + 1;
                    tris[trisHead++] = vertexBasis + j + 2;
                }

                var edgeTable = MarchingSquaresEdges[tile.config];
                if (edgeTable.Length >= 2)
                {
                    var edges = new List<Vector2>();
                    foreach (var e in edgeTable)
                    {
                        var v = new Vector2(tile.x, tile.y) + MarchingSquaresVertices[e];
                        edges.Add(v * mapData.unitScale);
                    }

                    shape.AddEdges(edges);
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

        private Tile FindTile(int x, int y)
        {
            foreach (var e in tiles)
            {
                if (e.x == x && e.y == y) return e;
            }

            return null;
        }

        private void UpdateTiles(int xl, int yl, int xu, int yu)
        {
            for (var x = xl; x <= xu; x++)
            for (var y = yl; y <= yu; y++)
            {
                UpdateTile(FindTile(x, y));
            }
            dirty = true;
        }

        private void UpdateTile(Tile tile)
        {
            if (tile == null) return;
            
            var x = tile.x;
            var y = tile.y;

            if (x == mapData.width - 1) return;
            if (y == mapData.height - 1) return;

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

            tile.config = config;
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
                Recompile();
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
            UpdateTiles(x - 1, y - 1, x + 1, y + 1);
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
            new int [] { },             // 0
            new int [] { 1, 7 },        // 1
            new int [] { 3, 1 },        // 2
            new int [] { 3, 7 },        // 3
            new int [] { 3, 5 },        // 4
            new int [] { 5, 7, 1, 3 },  // 5
            new int [] { 5, 1 },        // 6
            new int [] { 5, 7 },        // 7
            new int [] { 5, 7 },        // 8
            new int [] { 5, 1 },        // 9
            new int [] { 5, 3, 1, 7 },  // 10
            new int [] { 5, 3 },        // 11
            new int [] { 7, 3 },        // 12
            new int [] { 3, 1 },        // 13
            new int [] { 7, 1 },        // 14
            new int [] { },             // 14
        };

        public class Tile
        {
            public int x, y;
            public int config;

            public Tile(int x, int y, int config)
            {
                this.x = x;
                this.y = y;
                this.config = config;
            }
        }
    }
}