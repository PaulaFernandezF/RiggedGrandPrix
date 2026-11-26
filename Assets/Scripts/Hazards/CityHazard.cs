using System;
using System.Collections;
using System.Collections.Generic;
using RiggedGrandPrix.Race;
using UnityEngine;

namespace RiggedGrandPrix.Hazards
{
    /// <summary>
    /// Template Method: todas las trampas siguen la misma secuencia
    ///   Avisar → esperar → Activar → esperar → Restaurar
    /// que fija Run(). Cada ciudad solo implementa qué se construye (Build),
    /// qué pasa al activarse (OnActivate) y cómo se deshace (OnRestore).
    /// El aviso previo está garantizado por diseño: así el juego es difícil pero justo.
    /// Orientación local: Z = sentido de la carrera, X = a lo ancho de la carretera.
    /// </summary>
    public abstract class CityHazard : MonoBehaviour
    {
        public enum HazardState { Idle, Warning, Active, Cooldown }

        public int segment;
        [SerializeField] protected float activeTime = 4f;
        [SerializeField] protected float roadWidth = 12f;
        [SerializeField] protected float cooldownTime = 4f;

        public static IReadOnlyList<CityHazard> All => all;
        private static readonly List<CityHazard> all = new List<CityHazard>();

        /// <summary>Se lanza al empezar el aviso (el HUD lo muestra).</summary>
        public static event Action<CityHazard> WarningStarted;

        private HazardState state = HazardState.Idle;
        private GameObject warningLight;
        private Renderer warningRenderer;

        public bool IsReady => state == HazardState.Idle;
        public float ActiveTime => activeTime;
        public HazardState State => state;
        public float TrackT { get; private set; }
        public abstract string DisplayName { get; }

        protected virtual void Awake()
        {
            BuildWarningSign();
            Build();
            OnRestore();
        }

        protected virtual void Start()
        {
            var track = FindAnyObjectByType<TrackPath>();
            if (track != null)
            {
                TrackT = track.NearestT(transform.position);
                segment = track.SegmentAt(TrackT);
            }
        }

        private void OnEnable() { if (!all.Contains(this)) all.Add(this); }
        private void OnDisable() => all.Remove(this);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearStatics() => WarningStarted = null;

        /// <summary>Lo usa el generador del circuito para ajustar la trampa al ancho de la carretera.</summary>
        public void SetRoadWidth(float width) => roadWidth = width;

        [ContextMenu("Probar trampa (aviso de 2 s)")]
        private void TestTrigger() => Trigger(2f);

        public void Trigger(float warningSeconds)
        {
            if (!IsReady) return;
            StartCoroutine(Run(warningSeconds));
        }

        /// <summary>El "método plantilla": la secuencia es igual para todas las trampas.</summary>
        private IEnumerator Run(float warningSeconds)
        {
            state = HazardState.Warning;
            OnWarn();
            WarningStarted?.Invoke(this);
            yield return new WaitForSeconds(warningSeconds);

            warningLight.SetActive(false);
            state = HazardState.Active;
            OnActivate();
            yield return new WaitForSeconds(activeTime);

            OnRestore();
            state = HazardState.Cooldown;
            yield return new WaitForSeconds(cooldownTime);
            state = HazardState.Idle;
        }

        // ---------- Pasos de la plantilla ----------

        /// <summary>Por defecto: enciende la luz de aviso intermitente. Se puede ampliar.</summary>
        protected virtual void OnWarn() => warningLight.SetActive(true);
        protected abstract void Build();
        protected abstract void OnActivate();
        protected abstract void OnRestore();

        /// <summary>Vuelve al estado inicial (en la sección G lo llamará el GameManager al reiniciar).</summary>
        public void ResetState()
        {
            StopAllCoroutines();
            state = HazardState.Idle;
            if (warningLight != null) warningLight.SetActive(false);
            OnRestore();
        }

        protected virtual void Update()
        {
            if (state == HazardState.Warning && warningRenderer != null)
                warningRenderer.material.color = (Time.time * 6f) % 2f < 1f ? Color.red : Color.yellow;
        }

        // ---------- Ayudas para construir las piezas ----------

        private void BuildWarningSign()
        {
            warningLight = MakeBox("Aviso", new Vector3(roadWidth * 0.5f + 1.2f, 3.2f, 0f), Vector3.one * 1.1f,
                                   Color.red, solid: false, PrimitiveType.Sphere);
            warningRenderer = warningLight.GetComponent<Renderer>();
            MakeBox("Poste", new Vector3(roadWidth * 0.5f + 1.2f, 1.3f, 0f), new Vector3(0.25f, 2.6f, 0.25f),
                    new Color(0.2f, 0.2f, 0.2f), solid: true);
            warningLight.SetActive(false);
        }

        /// <summary>Crea una pieza hija en coordenadas locales de la trampa.</summary>
        protected GameObject MakeBox(string name, Vector3 localPos, Vector3 size, Color color, bool solid,
                                     PrimitiveType shape = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(shape);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().material.color = color;
            var col = go.GetComponent<Collider>();
            if (!solid) Destroy(col);
            return go;
        }

        /// <summary>Hace que una pieza frene a los karts que la tocan.</summary>
        protected static void AddHit(GameObject go, float slowFactor, float duration, bool trigger)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = trigger;
            var hit = go.AddComponent<HazardHit>();
            hit.Setup(slowFactor, duration, continuous: trigger);
        }

        /// <summary>Hace que una pieza se pueda mover empujando karts (Rigidbody cinemático).</summary>
        protected static Rigidbody MakeKinematic(GameObject go)
        {
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            return rb;
        }
    }
}
