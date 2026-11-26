using System.Collections.Generic;
using UnityEngine;

namespace RiggedGrandPrix.AI
{
    /// <summary>
    /// Lo que el Director sabe del jugador, construido a partir de RaceEvents (Observer):
    /// puesto, distancias con los de delante y detrás, y en qué tramos pierde más tiempo.
    /// </summary>
    public class PlayerModel
    {
        public int Position { get; set; } = 1;
        public int TotalKarts { get; set; } = 1;
        public float GapAhead { get; set; } = float.MaxValue;   // m hasta el de delante
        public float GapBehind { get; set; } = float.MaxValue;  // m hasta el de detrás
        public int CurrentSegment { get; set; }
        public float TrackT { get; set; }
        public float Speed { get; set; }
        public bool UsesShortcuts { get; set; }

        private readonly Dictionary<int, float> timeLost = new Dictionary<int, float>();
        private readonly Dictionary<int, float> bestTime = new Dictionary<int, float>();

        /// <summary>Registra el tiempo de un kart en un tramo. Si es el jugador, acumula lo que pierde.</summary>
        public void RecordSegment(int seg, float time, bool isPlayer)
        {
            if (time <= 0.2f) return;
            if (!bestTime.TryGetValue(seg, out float best) || time < best) bestTime[seg] = best = time;
            if (!isPlayer) return;
            float lost = Mathf.Max(0f, time - best);
            timeLost[seg] = timeLost.TryGetValue(seg, out float prev) ? Mathf.Lerp(prev, lost, 0.5f) : lost;
        }

        /// <summary>Tramo de los próximos n (sin contar el actual) en el que más tiempo pierde el jugador.</summary>
        public int WeakestSegmentAhead(int n, int segmentCount)
        {
            int bestSeg = (CurrentSegment + 1) % segmentCount;
            float worst = -1f;
            for (int i = 1; i <= n; i++)
            {
                int s = (CurrentSegment + i) % segmentCount;
                float l = timeLost.TryGetValue(s, out float v) ? v : 0f;
                if (l > worst + 0.05f) { worst = l; bestSeg = s; }
            }
            return bestSeg;
        }

        public float TimeLostIn(int seg) => timeLost.TryGetValue(seg, out float v) ? v : 0f;

        public void Reset()
        {
            timeLost.Clear();
            bestTime.Clear();
            Position = 1; GapAhead = GapBehind = float.MaxValue;
        }
    }
}
