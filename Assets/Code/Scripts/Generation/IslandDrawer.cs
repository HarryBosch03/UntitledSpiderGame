using UnityEngine;

namespace Crabs.Generation
{
    [RequireComponent(typeof(IslandGenerator))]
    public class IslandDrawer : MonoBehaviour
    {
        public bool color = false;
        public bool clip = true;
        public Color modulate = Color.white;
        
        private IslandGenerator generator;
        private MapData map;

        private static Material glDrawMaterial;

        private void OnDrawGizmos()
        {
            if (!generator)
            {
                generator = GetComponent<IslandGenerator>();
            }

            if (map == null || generator.isDirty)
            {
                map = generator.GenerateMap();
                generator.isDirty = false;
            }

            if (!glDrawMaterial)
            {
                glDrawMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                glDrawMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            glDrawMaterial.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.TRIANGLES);
            
            map.Enumerate((x, y, tile) =>
            {
                if (tile == null) return;

                var p = new Vector2(x, y);

                GL.Color(tile.color);
                GL.Vertex((p + new Vector2(-0.5f, 0.5f)) * map.unitScale);
                GL.Vertex((p + new Vector2(-0.5f, -0.5f)) * map.unitScale);
                GL.Vertex((p + new Vector2(0.5f, -0.5f)) * map.unitScale);

                GL.Vertex((p + new Vector2(0.5f, -0.5f)) * map.unitScale);
                GL.Vertex((p + new Vector2(0.5f, 0.5f)) * map.unitScale);
                GL.Vertex((p + new Vector2(-0.5f, 0.5f)) * map.unitScale); 
            });

            GL.End();
            GL.PopMatrix();
        }
    }
}