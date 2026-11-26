using System.Collections.Generic;
using System.Linq;
using RiggedGrandPrix.Hazards;
using RiggedGrandPrix.Karts;
using RiggedGrandPrix.Race;
using UnityEngine;

namespace RiggedGrandPrix.AI
{
    /// <summary>
    /// La IA que manipula la ciudad para que el jugador pierda.
    /// Observer: escucha RaceEvents y mantiene un PlayerModel.
    /// Strategy: elige táctica (Presión / Contención / Pasiva) según la situación.
    /// Actúa sobre CityHazard (Template Method), siempre con aviso previo y con límites.
    /// </summary>
    public class Director : MonoBehaviour
    {
        [Header("Dificultad (en la sección H la pondrá cada ciudad)")]
        [SerializeField] private int interventionsPerLap = 2;
        [SerializeField] private float cooldown = 12f;
        [SerializeField] private float warningTime = 1.5f;

        [Header("Umbrales de las tácticas (m)")]
        [SerializeField] private float pressureMinLead = 10f;
        [SerializeField] private float containmentMaxGap = 40f;
        [SerializeField] private float passiveMaxDeficit = 90f;

        [SerializeField] private bool showDebug = true;

        private readonly List<IDirectorStrategy> strategies = new List<IDirectorStrategy>();
        private IDirectorStrategy current;
        private readonly PlayerModel model = new PlayerModel();
        private RaceManager race;
        private TrackPath track;
        private KartController player;
        private int interventionsLeft;
        private float cooldownLeft;
        private string lastDecision = "—";

        public PlayerModel Model => model;

        public void Configure(int perLap, float cooldownSeconds, float warningSeconds)
        {
            interventionsPerLap = perLap;
            cooldown = cooldownSeconds;
            warningTime = warningSeconds;
        }

        private void Start()
        {
            race = FindAnyObjectByType<RaceManager>();
            track = FindAnyObjectByType<TrackPath>();
            int segs = track != null ? track.SegmentCount : 12;
            strategies.Add(new PassiveStrategy(passiveMaxDeficit));
            strategies.Add(new PressureStrategy(pressureMinLead, segs));
            strategies.Add(new ContainmentStrategy(containmentMaxGap));
            ResetState();
        }

        private void OnEnable()
        {
            RaceEvents.SegmentTimed += OnSegmentTimed;
            RaceEvents.LapCompleted += OnLapCompleted;
            RaceEvents.StandingsChanged += OnStandingsChanged;
        }

        private void OnDisable()
        {
            RaceEvents.SegmentTimed -= OnSegmentTimed;
            RaceEvents.LapCompleted -= OnLapCompleted;
            RaceEvents.StandingsChanged -= OnStandingsChanged;
        }

        private KartController Player
        {
            get
            {
                if (player == null) player = KartController.All.FirstOrDefault(k => k.GetComponent<PlayerInput>() != null);
                return player;
            }
        }

        // ---------------- Observer ----------------

        private void OnSegmentTimed(KartController k, int seg, float t) => model.RecordSegment(seg, t, k == Player);

        private void OnLapCompleted(KartController k, int lap)
        {
            if (k == Player) interventionsLeft = interventionsPerLap; // presupuesto por vuelta
        }

        private void OnStandingsChanged(IReadOnlyList<KartController> s)
        {
            model.TotalKarts = s.Count;
            for (int i = 0; i < s.Count; i++) if (s[i] == Player) model.Position = i + 1;
        }

        // ---------------- Decisión ----------------

        private void Update()
        {
            if (race == null || track == null || Player == null || !race.IsRunning) return;
            UpdateModel();
            if (cooldownLeft > 0f) cooldownLeft -= Time.deltaTime;
            SelectStrategy();
            TryIntervene();
        }

        /// <summary>Distancias por pista con el kart de delante y el de detrás.</summary>
        private void UpdateModel()
        {
            float length = Mathf.Max(track.Length, 1f);
            model.TrackT = track.NearestT(Player.transform.position);
            model.CurrentSegment = track.SegmentAt(model.TrackT);
            model.Speed = Player.ForwardSpeed;

            var s = race.Standings;
            int i = model.Position - 1;
            model.GapAhead = i > 0 ? Gap(s[i - 1], length) : float.MaxValue;
            model.GapBehind = i + 1 < s.Count ? Gap(s[i + 1], length, behind: true) : float.MaxValue;
        }

        private float Gap(KartController other, float length, bool behind = false)
        {
            float t = track.NearestT(other.transform.position);
            float d = behind ? TrackPath.ForwardDelta(t, model.TrackT) : TrackPath.ForwardDelta(model.TrackT, t);
            return d * length;
        }

        private void SelectStrategy()
        {
            current = strategies.Where(st => st.IsApplicable(model))
                                .OrderByDescending(st => st.Priority)
                                .FirstOrDefault();
        }

        private void TryIntervene()
        {
            if (current == null || interventionsLeft <= 0 || cooldownLeft > 0f) return;

            var hazard = current.ChooseHazard(model, CityHazard.All, DistanceAheadOfPlayer);
            if (hazard == null) return;

            hazard.Trigger(warningTime);
            interventionsLeft--;
            cooldownLeft = cooldown;
            lastDecision = $"{current.Name} → {hazard.DisplayName} (tramo {hazard.segment})";
            Debug.Log($"[Director] {lastDecision}. Puesto {model.Position}, ventaja {model.GapBehind:F0} m, " +
                      $"tiempo perdido en ese tramo {model.TimeLostIn(hazard.segment):F2} s");
        }

        /// <summary>
        /// Metros que le faltan al jugador para llegar a la trampa, o −1 si no sirve.
        /// Solo valen las trampas a las que el jugador llegará mientras estén activas:
        /// ni tan cerca que no dé tiempo a ver el aviso (injusto), ni tan lejos que
        /// se desactiven antes de que llegue (inútil).
        /// </summary>
        private float DistanceAheadOfPlayer(CityHazard h)
        {
            float length = Mathf.Max(track.Length, 1f);
            float d = TrackPath.ForwardDelta(model.TrackT, h.TrackT) * length;
            float speed = Mathf.Max(model.Speed, 8f);
            float eta = d / speed;                                    // segundos hasta llegar
            float earliest = warningTime + h.ActiveTime * 0.15f;
            float latest = warningTime + h.ActiveTime * 0.7f;
            return d >= 15f && eta >= earliest && eta <= latest ? d : -1f;
        }

        /// <summary>Vuelve al estado inicial (en la sección G se llamará al reiniciar la carrera).</summary>
        public void ResetState()
        {
            model.Reset();
            interventionsLeft = interventionsPerLap;
            cooldownLeft = cooldown * 0.5f; // nada más salir no hace nada
            lastDecision = "—";
            foreach (var h in CityHazard.All) h.ResetState();
        }

        private void OnGUI()
        {
            if (!showDebug || race == null || !race.IsRunning) return;
            var st = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(10, Screen.height - 70, 900, 60),
                $"Director · táctica: {current?.Name ?? "ninguna"} · intervenciones: {interventionsLeft} · última: {lastDecision}", st);
        }
    }
}
