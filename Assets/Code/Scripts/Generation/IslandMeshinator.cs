using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Crabs.Generation
{
    [RequireComponent(typeof(IslandGenerator))]
    [RequireComponent(typeof(MeshFilter))]
    public class IslandMeshinator : MonoBehaviour
    {
        [SerializeField] private bool generateEditor;

        private IslandGenerator generator;
        private MapData mapData;
        private MeshFilter meshFilter;
        private CustomCollider2D collider;
        private Mesh mesh;
        private PhysicsShapeGroup2D shape;

        private List<Vector3> vertices;
        private List<Color> vertexColors;
        private List<Vector2> uvs;
        private List<int> tris;

        private void Awake()
        {
            GenerateMesh();
        }

        private void OnValidate()
        {
            if (!generateEditor) return;

            generateEditor = false;
            GenerateMesh();
        }

        private void Initialize()
        {
            generator = GetComponent<IslandGenerator>();
            mapData = generator.GenerateMap();

            meshFilter = GetComponent<MeshFilter>();

            if (!mesh)
            {
                mesh = new Mesh();
                mesh.name = "[PROC] IslandMeshinator.Mesh";
                mesh.hideFlags = HideFlags.HideAndDontSave;
            }

            meshFilter.sharedMesh = mesh;
            vertices = new List<Vector3>();
            vertexColors = new List<Color>();
            tris = new List<int>();
            uvs = new List<Vector2>();

            shape = new PhysicsShapeGroup2D();
        }

        private void GenerateMesh()
        {
            Initialize();

            mapData.Enumerate((x, y, _) =>
            {
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

                var triTable = MarchingSquaresIndices[config];
                var basis = vertices.Count;
                for (var i0 = triTable.Length - 1; i0 >= 0; i0--)
                {
                    var i1 = triTable[i0];
                    var interpolation = MarchingSquaresVertices[i1];
                    var vertex = new Vector2(x, y) + interpolation;
                    vertices.Add(vertex * mapData.unitScale);

                    var weight = InterpolateOverCell(weights[0], weights[1], weights[3], weights[2], interpolation, Mathf.Lerp);
                    vertexColors.Add(WeightToColor(weight));

                    uvs.Add(new Vector2(vertex.x / mapData.width, vertex.y / mapData.height));
                }

                for (var i = 0; i < triTable.Length - 2; i++)
                {
                    tris.Add(basis);
                    tris.Add(basis + i + 1);
                    tris.Add(basis + i + 2);
                }

                var edgeTable = MarchingSquaresEdges[config];
                if (edgeTable.Length >= 2)
                {
                    var edges = new List<Vector2>();
                    foreach (var i in edgeTable)
                    {
                        var v = new Vector2(x, y) + MarchingSquaresVertices[i];
                        edges.Add(v * mapData.unitScale);
                    }

                    shape.AddEdges(edges);
                }
            });

            mesh.Clear();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(tris, 0);
            mesh.SetColors(vertexColors);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();

            SetCollider();
        }

        private Color WeightToColor(float weight)
        {
            var value = Mathf.Atan(weight) / Mathf.PI + 0.5f;
            return new Color(value, value, value, 1.0f);
        }

        private T InterpolateOverCell<T>(T a, T b, T c, T d, Vector2 interpolation, Func<T, T, float, T> lerp)
        {
            return lerp(lerp(a, b, interpolation.x), lerp(c, d, interpolation.x), interpolation.y);
        }
        
        private void SetCollider()
        {
            collider = GetComponent<CustomCollider2D>();
            if (!collider) return;

            collider.SetCustomShapes(shape);
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
    }
}