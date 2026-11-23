using System.Collections.Generic;
using RiggedGrandPrix.Karts;
using UnityEngine;

namespace RiggedGrandPrix.Race
{
    /// <summary>
    /// Árbitro de la carrera: cuenta vueltas con los checkpoints, calcula las posiciones,
    /// coloca la parrilla y decide cuándo termina. Todo lo publica en RaceEvents (Observer).
    /// </summary>
    public class RaceManager : MonoBehaviour
    {
        [SerializeField] private TrackPath track;
        [SerializeField] private int laps = 2;
        [Tooltip("Puesto mínimo para ganar (1 = hay que quedar primero).")]
        [SerializeField] private int requiredPosition = 3;
        [Tooltip("Empezar sola al darle a Play (más adelante la arrancará el GameManager).")]
        [SerializeField] private bool autoStart = true;

        [Header("Parrilla de salida")]
        [SerializeField] private float gridStartDistance = 3f;   // metros después de la meta
        [SerializeField] private float gridRowSpacing = 5f;
        [SerializeField] private float gridLateralOffset = 2.5f;

        [Tooltip("Si un kart cae por debajo de esta altura, vuelve a la pista.")]
        [SerializeField] private float fallHeight = -5f;

        private readonly List<LapTracker> trackers = new List<LapTracker>();
        private readonly Dictionary<KartController, LapTracker> byKart = new Dictionary<KartController, LapTracker>();
        private List<KartController> standings = new List<KartController>();
        private int checkpointCount;
        private bool running;
        private float raceStartTime;
        private RaceResult lastResult;

        public IReadOnlyList<KartController> Standings => standings;
        public int Laps => laps;
        public int RequiredPosition => requiredPosition;
        public bool IsRunning => running;
        public float RaceTime => running ? Time.time - raceStartTime : 0f;
        public TrackPath Track => track;

        public int GetPosition(KartController k) => standings.IndexOf(k) + 1;
        public int GetLap(KartController k) => byKart.TryGetValue(k, out var t) ? t.Lap : 0;

        private void Awake()
        {
            if (track == null) track = FindAnyObjectByType<TrackPath>();
        }

        private void OnEnable() => Checkpoint.KartPassed += OnCheckpointPassed;
        private void OnDisable() => Checkpoint.KartPassed -= OnCheckpointPassed;

        private void Start()
        {
            foreach (var k in KartController.All)
            {
                var t = new LapTracker(k, k.GetComponent<PlayerInput>() != null);
                trackers.Add(t);
                byKart[k] = t;
                standings.Add(k);
            }
            checkpointCount = Checkpoint.All.Count;
            if (checkpointCount == 0)
                Debug.LogError("[RaceManager] No hay checkpoints: en el Circuito → ⋮ → Generar checkpoints.");

            PlaceOnGrid();
            if (autoStart) StartRace();
        }

        // ---------------- Control de la carrera ----------------

        public void StartRace()
        {
            foreach (var t in trackers) t.Reset();
            lastResult = null;
            running = true;
            raceStartTime = Time.time;
        }

        public void StopRace() => running = false;

        /// <summary>Coloca los karts de dos en dos justo después de la meta. El jugador sale el último.</summary>
        private void PlaceOnGrid()
        {
            if (track == null) return;
            float length = Mathf.Max(track.Length, 1f);
            var ordered = new List<LapTracker>(trackers);
            ordered.Sort((a, b) => a.IsPlayer.CompareTo(b.IsPlayer)); // false (rivales) primero

            for (int i = 0; i < ordered.Count; i++)
            {
                int row = ordered.Count - 1 - i;   // los primeros de la lista, más adelante
                float t = TrackPath.Wrap((gridStartDistance + row / 2 * gridRowSpacing) / length);
                float side = row % 2 == 0 ? -gridLateralOffset : gridLateralOffset;
                Vector3 pos = track.PointAt(t) + track.RightAt(t) * side + Vector3.up * 0.6f;
                Quaternion rot = Quaternion.LookRotation(track.TangentAt(t), Vector3.up);
                ordered[i].Kart.SetSpawnPoint(pos, rot);
            }
        }

        // ---------------- Vueltas ----------------

        private void OnCheckpointPassed(KartController kart, Checkpoint cp)
        {
            if (!running || checkpointCount == 0) return;
            if (!byKart.TryGetValue(kart, out var t) || t.Finished) return;
            if (cp.index != t.NextCheckpoint) return; // hay que cruzarlos en orden (no vale atajar)

            // Cronometra el tramo recorrido desde el checkpoint anterior (lo usa el Director)
            if (t.LastCheckpointTime >= 0f)
                RaceEvents.RaiseSegmentTimed(kart, t.LastSegment, RaceTime - t.LastCheckpointTime);
            t.LastCheckpointTime = RaceTime;
            t.LastSegment = cp.segment;

            if (cp.index == 0) // cruza la meta habiendo pasado por todos
            {
                t.Lap++;
                RaceEvents.RaiseLapCompleted(kart, t.Lap);
                Debug.Log($"{kart.name} completa la vuelta {t.Lap}/{laps}");

                if (t.Lap >= laps)
                {
                    t.Finished = true;
                    t.FinishTime = RaceTime;
                    if (t.IsPlayer) FinishForPlayer(t);
                }
            }
            t.NextCheckpoint = (cp.index + 1) % checkpointCount;
        }

        private void FinishForPlayer(LapTracker player)
        {
            UpdateStandings();
            var times = new List<float>();
            foreach (var t in trackers) if (t.Finished) times.Add(t.FinishTime);

            lastResult = new RaceResult(GetPosition(player.Kart), requiredPosition, times);
            running = false;
            Debug.Log($"Carrera terminada: puesto {lastResult.PlayerPosition} (exigido {requiredPosition}) → " +
                      (lastResult.IsVictory ? "VICTORIA" : "DERROTA"));
            RaceEvents.RaiseRaceFinished(lastResult);
        }

        // ---------------- Posiciones ----------------

        private void Update()
        {
            if (!running) return;
            foreach (var t in trackers)
                if (t.Kart.transform.position.y < fallHeight) RespawnOnTrack(t.Kart);
            UpdateStandings();
        }

        /// <summary>Cuánto lleva recorrido un kart: vueltas + fracción de la vuelta actual.</summary>
        private float Progress(LapTracker t)
        {
            if (t.Finished) return laps + 10f - t.FinishTime * 0.001f; // los que acaban, delante y por orden de llegada
            float frac = track.NearestT(t.Kart.transform.position);
            if (t.NextCheckpoint == 1 && frac > 0.5f) frac -= 1f;       // aún no ha cruzado la meta (está justo detrás)
            return t.Lap + frac;
        }

        private void UpdateStandings()
        {
            var old = standings;
            var sorted = new List<LapTracker>(trackers);
            sorted.Sort((a, b) => Progress(b).CompareTo(Progress(a)));
            var now = new List<KartController>();
            foreach (var t in sorted) now.Add(t.Kart);

            bool changed = false;
            for (int i = 0; i < now.Count; i++)
                if (i >= old.Count || old[i] != now[i]) { changed = true; break; }
            if (!changed) return;

            // Adelantamiento: a va ahora delante de b y antes iba detrás
            for (int a = 0; a < now.Count; a++)
                for (int b = a + 1; b < now.Count; b++)
                    if (old.IndexOf(now[a]) > old.IndexOf(now[b]))
                        RaceEvents.RaiseOvertake(now[a], now[b]);

            standings = now;
            RaceEvents.RaiseStandingsChanged(standings);
        }

        /// <summary>Devuelve el kart al punto de la pista más cercano, mirando hacia delante.</summary>
        public void RespawnOnTrack(KartController kart)
        {
            float t = track.NearestT(kart.transform.position);
            kart.Teleport(track.PointAt(t) + Vector3.up * 1f, Quaternion.LookRotation(track.TangentAt(t), Vector3.up));
        }

        // ---------------- Marcador de prueba (lo sustituirá el HUD) ----------------

        private void OnGUI()
        {
            KartController player = null;
            foreach (var t in trackers) if (t.IsPlayer) player = t.Kart;
            if (player == null) return;

            string text = lastResult != null
                ? $"¡Carrera terminada! Puesto {lastResult.PlayerPosition} → {(lastResult.IsVictory ? "VICTORIA" : "DERROTA")}"
                : $"Vuelta {Mathf.Min(GetLap(player) + 1, laps)}/{laps}   ·   Puesto {GetPosition(player)}/{standings.Count}   ·   {RaceTime:F1} s";
            GUI.Label(new Rect(10, 160, 900, 30), text);
        }
    }
}