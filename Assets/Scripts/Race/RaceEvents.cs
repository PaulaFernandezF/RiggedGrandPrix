using System;
using System.Collections.Generic;
using RiggedGrandPrix.Karts;
using UnityEngine;

namespace RiggedGrandPrix.Race
{
    /// <summary>
    /// Subject del patrón Observer: canal de eventos de la carrera.
    /// El RaceManager publica; quien quiera enterarse se suscribe.
    /// Nadie que publica conoce a quien escucha.
    /// </summary>
    public static class RaceEvents
    {
        public static event Action<KartController, int> LapCompleted;          // kart, vueltas completadas
        public static event Action<KartController, int, float> SegmentTimed;   // kart, tramo, segundos que ha tardado
        public static event Action<KartController, KartController> Overtake;   // quien adelanta, adelantado
        public static event Action<IReadOnlyList<KartController>> StandingsChanged;
        public static event Action<RaceResult> RaceFinished;
        public static event Action<KartController> KartHit;                    // a un kart le ha dado algo

        public static void RaiseLapCompleted(KartController k, int lap) => LapCompleted?.Invoke(k, lap);
        public static void RaiseSegmentTimed(KartController k, int seg, float t) => SegmentTimed?.Invoke(k, seg, t);
        public static void RaiseOvertake(KartController a, KartController b) => Overtake?.Invoke(a, b);
        public static void RaiseStandingsChanged(IReadOnlyList<KartController> s) => StandingsChanged?.Invoke(s);
        public static void RaiseRaceFinished(RaceResult r) => RaceFinished?.Invoke(r);
        public static void RaiseKartHit(KartController k) => KartHit?.Invoke(k);

        /// <summary>Unity puede no recargar los estáticos al darle a Play: los limpiamos.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearAll()
        {
            LapCompleted = null; SegmentTimed = null; Overtake = null; StandingsChanged = null; RaceFinished = null; KartHit = null;
        }
    }
}