using System;
using System.Collections.Generic;
using RiggedGrandPrix.Karts;
using UnityEngine;

namespace RiggedGrandPrix.Race
{
    /// <summary>
    /// Trigger que los karts deben atravesar en orden. El índice 0 es la línea de meta.
    /// Cuando un kart lo cruza lanza el evento KartPassed (Observer): en la C.2
    /// el RaceManager se suscribirá para contar vueltas y posiciones.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Checkpoint : MonoBehaviour
    {
        public int index;
        public int segment;
        public bool isFinishLine;

        /// <summary>Se lanza cuando un kart cruza cualquier checkpoint.</summary>
        public static event Action<KartController, Checkpoint> KartPassed;

        /// <summary>Todos los checkpoints activos de la escena (se registran solos).</summary>
        public static IReadOnlyList<Checkpoint> All => all;
        private static readonly List<Checkpoint> all = new List<Checkpoint>();

        [Tooltip("Escribe en la consola cada vez que un kart lo cruza (para probar la C.1).")]
        [SerializeField] private bool logPasses = true;

        private void OnEnable() { if (!all.Contains(this)) all.Add(this); }
        private void OnDisable() => all.Remove(this);

        public void Setup(int index, int segment, bool isFinishLine)
        {
            this.index = index;
            this.segment = segment;
            this.isFinishLine = isFinishLine;
        }

        private void OnTriggerEnter(Collider other)
        {
            var kart = other.GetComponentInParent<KartController>();
            if (kart == null) return;
            if (logPasses) Debug.Log($"{kart.name} cruza {name}");
            KartPassed?.Invoke(kart, this);
        }

        /// <summary>Unity puede no recargar los estáticos al darle a Play: los limpiamos.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearStatics()
        {
            KartPassed = null;
            all.Clear();
        }

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider>();
            Gizmos.color = isFinishLine ? new Color(1f, 1f, 1f, 0.5f) : new Color(0f, 1f, 0.4f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}
