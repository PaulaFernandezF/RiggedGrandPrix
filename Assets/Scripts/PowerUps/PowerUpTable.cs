using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RiggedGrandPrix.PowerUps
{
    /// <summary>
    /// Probabilidad de cada objeto según el puesto (0 = primero, 1 = último).
    /// Quien va detrás recibe objetos más útiles para remontar.
    /// </summary>
    [CreateAssetMenu(menuName = "Rigged Grand Prix/Power-up Table", fileName = "PowerUpTable")]
    public class PowerUpTable : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public PowerUpType type;
            public AnimationCurve weightByPosition = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        }

        public List<Entry> entries = new List<Entry>();

        public PowerUpType Sample(int position, int total)
        {
            float x = total > 1 ? (position - 1f) / (total - 1f) : 0f;
            float sum = 0f;
            foreach (var e in entries) sum += Mathf.Max(0f, e.weightByPosition.Evaluate(x));
            float roll = Random.value * sum;
            foreach (var e in entries)
            {
                roll -= Mathf.Max(0f, e.weightByPosition.Evaluate(x));
                if (roll <= 0f) return e.type;
            }
            return PowerUpType.Turbo;
        }

        /// <summary>Tabla por defecto si no se asigna ninguna en el Inspector.</summary>
        public static PowerUpTable CreateDefault()
        {
            var t = CreateInstance<PowerUpTable>();
            void Add(PowerUpType type, float first, float last) =>
                t.entries.Add(new Entry { type = type, weightByPosition = AnimationCurve.Linear(0f, first, 1f, last) });
            Add(PowerUpType.Turbo, 0.3f, 1.0f);
            Add(PowerUpType.OilSlick, 1.0f, 0.3f);
            Add(PowerUpType.HomingMissile, 0.1f, 1.0f);
            Add(PowerUpType.Shield, 0.8f, 0.4f);
            Add(PowerUpType.Pulse, 0.3f, 0.8f);
            return t;
        }
    }
}