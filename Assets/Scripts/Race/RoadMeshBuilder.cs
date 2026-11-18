using System.Collections.Generic;
using UnityEngine;

namespace RiggedGrandPrix.Race
{
    /// <summary>
    /// Genera una carretera plana (malla) siguiendo la spline del TrackPath.
    /// Solo es visual: el kart se apoya en el suelo de debajo.
    /// Clic derecho (⋮) en el componente → "Construir carretera".
    /// </summary>
    [RequireComponent(typeof(TrackPath), typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoadMeshBuilder : MonoBehaviour
    {
        [SerializeField] private float width = 12f;
        [SerializeField] private int samples = 300;
        [SerializeField] private float heightOffset = 0.03f;   // un pelín por encima del suelo para que no parpadee
        [SerializeField] private float uvTiling = 0.1f;
        [SerializeField] private Color color = new Color(0.22f, 0.22f, 0.25f);

        public float Width => width;
        public void SetWidth(float w) => width = w;

        private void Awake()
        {
            if (GetComponent<MeshFilter>().sharedMesh == null) Build();
        }

        [ContextMenu("Construir carretera")]
        public void Build()
        {
            var track = GetComponent<TrackPath>();
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            float dist = 0f;
            Vector3 prev = track.PointAt(0f);

            // Por cada muestra de la spline ponemos dos vértices (borde izquierdo y derecho)
            for (int i = 0; i <= samples; i++)
            {
                float t = (float)i / samples;
                Vector3 p = track.PointAt(t);
                Vector3 r = track.RightAt(t) * (width * 0.5f);
                dist += Vector3.Distance(prev, p);
                prev = p;

                verts.Add(transform.InverseTransformPoint(p - r + Vector3.up * heightOffset));
                verts.Add(transform.InverseTransformPoint(p + r + Vector3.up * heightOffset));
                uvs.Add(new Vector2(0f, dist * uvTiling));
                uvs.Add(new Vector2(1f, dist * uvTiling));

                // Dos triángulos entre esta pareja de vértices y la siguiente
                if (i < samples)
                {
                    int a = i * 2;
                    tris.AddRange(new[] { a, a + 2, a + 1, a + 1, a + 2, a + 3 });
                }
            }

            var mesh = new Mesh { name = "Carretera" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = mesh;

            // Si no tiene material, le ponemos uno gris oscuro (si no, se vería rosa)
            var mr = GetComponent<MeshRenderer>();
            if (mr.sharedMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mr.sharedMaterial = new Material(shader) { name = "Asfalto", color = color };
            }
        }
    }
}
