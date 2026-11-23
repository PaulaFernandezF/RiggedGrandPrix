using System.Collections.Generic;
using RiggedGrandPrix.Hazards;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace RiggedGrandPrix.Race
{
    /// <summary>
    /// Envoltorio del circuito (una spline cerrada). Da posiciones, direcciones,
    /// el punto más cercano y el tramo, para la carrera y, más adelante, para la IA.
    /// t va de 0 a 1 a lo largo de una vuelta; t = 0 es la línea de meta.
    /// Clic derecho (⋮) en el componente: "Crear circuito de prueba" y "Generar checkpoints".
    /// </summary>
    [RequireComponent(typeof(SplineContainer))]
    public class TrackPath : MonoBehaviour
    {
        public const string CheckpointsRootName = "Checkpoints (Circuito)";
        public const string HazardsRootName = "Trampas (Circuito)";

        [Tooltip("En cuántos tramos se divide la vuelta (lo usará el Director).")]
        [SerializeField] private int segmentCount = 12;

        [Header("Circuito de prueba")]
        [SerializeField] private float radiusX = 55f;
        [SerializeField] private float radiusZ = 38f;
        [SerializeField] private int knotCount = 16;

        [Header("Checkpoints")]
        [SerializeField] private int checkpointCount = 10;
        [SerializeField] private float checkpointWidth = 16f;
        [SerializeField] private float checkpointHeight = 4f;

        [Header("Trampas del Director")]
        [SerializeField] private HazardKind hazardKind = HazardKind.TrafficLight;
        [SerializeField] private int hazardCount = 4;

        private SplineContainer container;

        public int SegmentCount => segmentCount;
        public float Length => Container.CalculateLength();

        private SplineContainer Container
        {
            get
            {
                if (container == null) container = GetComponent<SplineContainer>();
                return container;
            }
        }

        // ---------------- Consultas ----------------

        /// <summary>Posición en la vuelta (0..1) del punto del circuito más cercano a worldPos.</summary>
        public float NearestT(Vector3 worldPos)
        {
            float3 local = Container.transform.InverseTransformPoint(worldPos);
            SplineUtility.GetNearestPoint(Container.Spline, local, out _, out float t, 6, 3);
            return t;
        }

        public Vector3 PointAt(float t) => (Vector3)Container.EvaluatePosition(Wrap(t));

        /// <summary>Dirección de avance en t (en horizontal).</summary>
        public Vector3 TangentAt(float t)
        {
            Vector3 tan = (Vector3)Container.EvaluateTangent(Wrap(t));
            tan.y = 0f;
            return tan.sqrMagnitude > 1e-6f ? tan.normalized : transform.forward;
        }

        /// <summary>Derecha del circuito en t (perpendicular a la tangente).</summary>
        public Vector3 RightAt(float t) => Vector3.Cross(Vector3.up, TangentAt(t)).normalized;

        public int SegmentAt(float t) => Mathf.Clamp(Mathf.FloorToInt(Wrap(t) * segmentCount), 0, segmentCount - 1);

        /// <summary>Deja t entre 0 y 1 (la vuelta se repite).</summary>
        public static float Wrap(float t) => t - Mathf.Floor(t);

        /// <summary>Cuánto hay que avanzar (en fracción de vuelta) para ir de "from" a "to".</summary>
        public static float ForwardDelta(float from, float to) => Wrap(to - from);

        // ---------------- Generación ----------------

        /// <summary>
        /// Crea una spline cerrada con forma de óvalo ondulado.
        /// Usa r(θ) = R·(1 + a·sin(2θ) + b·sin(3θ)): como el radio siempre es positivo,
        /// la curva nunca se cruza consigo misma.
        /// </summary>
        [ContextMenu("Crear circuito de prueba")]
        public void CreateTestCircuit()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(Container, "Crear circuito de prueba");
#endif
            transform.localScale = Vector3.one;
            var spline = Container.Spline;
            spline.Clear();
            for (int i = 0; i < knotCount; i++)
            {
                float theta = 2f * Mathf.PI * i / knotCount;
                float k = 1f + 0.18f * Mathf.Sin(2f * theta + 0.5f) + 0.08f * Mathf.Sin(3f * theta);
                var p = new Vector3(Mathf.Cos(theta) * radiusX * k, 0f, Mathf.Sin(theta) * radiusZ * k);
                spline.Add(new BezierKnot((float3)p), TangentMode.AutoSmooth);
            }
            spline.Closed = true;

            var road = GetComponent<RoadMeshBuilder>();
            if (road != null) road.Build();
        }

        /// <summary>Coloca checkpoints repartidos por la vuelta; el 0 es la meta.</summary>
        [ContextMenu("Generar checkpoints")]
        public GameObject CreateCheckpoints()
        {
            var old = GameObject.Find(CheckpointsRootName);
            if (old != null)
            {
                if (Application.isPlaying) Destroy(old);
                else DestroyImmediate(old);
            }

            var root = new GameObject(CheckpointsRootName);
            for (int i = 0; i < checkpointCount; i++)
            {
                float t = (float)i / checkpointCount;
                var go = new GameObject(i == 0 ? "Checkpoint 0 (Meta)" : $"Checkpoint {i}");
                go.transform.SetParent(root.transform, true);
                go.transform.SetPositionAndRotation(PointAt(t) + Vector3.up * checkpointHeight * 0.5f,
                                                    Quaternion.LookRotation(TangentAt(t), Vector3.up));
                var box = go.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(checkpointWidth, checkpointHeight, 1f);
                go.AddComponent<Checkpoint>().Setup(i, SegmentAt(t), i == 0);
            }
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.Undo.RegisterCreatedObjectUndo(root, CheckpointsRootName);
#endif
            return root;
        }

        /// <summary>Reparte 'count' trampas por la vuelta, alternando los tipos indicados.</summary>
        public GameObject CreateHazards(IList<HazardKind> kinds, int count, float roadWidth)
        {
            var old = GameObject.Find(HazardsRootName);
            if (old != null)
            {
                if (Application.isPlaying) { old.SetActive(false); Destroy(old); }
                else DestroyImmediate(old);
            }
            var root = new GameObject(HazardsRootName);
            if (kinds == null || kinds.Count == 0) return root;

            for (int i = 0; i < count; i++)
            {
                float t = 0.08f + (float)i / count + 0.3f / count;   // repartidas, lejos de la salida
                var kind = kinds[i % kinds.Count];
                var go = new GameObject($"{kind} {i + 1}");
                go.SetActive(false); // así su Awake se ejecuta cuando ya sabe el ancho de la carretera
                go.transform.SetParent(root.transform, true);
                go.transform.SetPositionAndRotation(PointAt(t), Quaternion.LookRotation(TangentAt(t), Vector3.up));
                CityHazard h = kind switch
                {
                    HazardKind.FloodGate => go.AddComponent<FloodGateHazard>(),
                    HazardKind.TrainCrossing => go.AddComponent<TrainCrossingHazard>(),
                    HazardKind.ShortcutGate => go.AddComponent<ShortcutGate>(),
                    HazardKind.TaxiTraffic => go.AddComponent<TaxiTrafficHazard>(),
                    HazardKind.Roadworks => go.AddComponent<RoadworksHazard>(),
                    HazardKind.Sandstorm => go.AddComponent<SandstormHazard>(),
                    _ => go.AddComponent<TrafficLightHazard>()
                };
                h.SetRoadWidth(roadWidth);
                go.SetActive(true);
            }
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.Undo.RegisterCreatedObjectUndo(root, HazardsRootName);
#endif
            return root;
        }

        [ContextMenu("Generar trampas")]
        private void GenerateHazardsMenu()
        {
            var road = GetComponent<RoadMeshBuilder>();
            CreateHazards(new[] { hazardKind }, hazardCount, road != null ? road.Width : 12f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Rayitas amarillas en el inicio de cada tramo
            Gizmos.color = Color.yellow;
            for (int s = 0; s < segmentCount; s++)
            {
                float t = (float)s / segmentCount;
                Gizmos.DrawLine(PointAt(t) - RightAt(t) * 3f, PointAt(t) + RightAt(t) * 3f);
            }
        }
#endif
    }
}
