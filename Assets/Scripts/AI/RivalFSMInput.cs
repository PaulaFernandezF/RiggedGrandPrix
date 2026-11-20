using RiggedGrandPrix.Karts;
using RiggedGrandPrix.Race;
using UnityEngine;

namespace RiggedGrandPrix.AI
{
    /// <summary>
    /// Conductor de un rival. Es otra estrategia de IKartInput (Strategy) y por dentro
    /// una máquina de estados (State): percibe → el estado actual decide → cambia de estado si toca.
    /// </summary>
    [RequireComponent(typeof(KartController))]
    public class RivalFSMInput : MonoBehaviour, IKartInput
    {
        [Tooltip("Velocidad máxima del rival respecto a su personaje (1 = al 100%).")]
        [SerializeField] private float speedFactor = 0.95f;
        [SerializeField] private bool showDebugLabel = true;

        // IKartInput
        public float Throttle { get; private set; }
        public float Steer { get; private set; }
        public bool Drift { get; private set; }

        public KartController Kart { get; private set; }
        public TrackPath Track { get; private set; }
        public float PreferredLane { get; private set; }
        public float RoadHalfWidth { get; private set; } = 6f;
        public float MaxLaneOffset => Mathf.Max(1f, RoadHalfWidth - 2f);
        public string StateName => state != null ? state.Name : "-";

        private readonly RivalPerception perception = new RivalPerception();
        private IRivalState state;
        private RaceManager race;
        private float laneTimer;

        public void SetSpeedFactor(float f) => speedFactor = f;

        private void Awake()
        {
            Kart = GetComponent<KartController>();
            race = FindAnyObjectByType<RaceManager>();
            Track = race != null ? race.Track : FindAnyObjectByType<TrackPath>();
            if (Track == null) race = null;
            if (Track == null) Track = FindAnyObjectByType<TrackPath>();
            var road = Track != null ? Track.GetComponent<RoadMeshBuilder>() : null;
            if (road != null) RoadHalfWidth = road.Width * 0.5f;
            PreferredLane = Random.Range(-1f, 1f) * MaxLaneOffset * 0.6f;
        }

        private void Start() => ChangeState(new FollowLineState());

        private void Update()
        {
            if (Track == null || state == null) return;
            if (race != null && !race.IsRunning) { SetOutput(0f, 0f, false); return; }

            float dt = Time.deltaTime;
            Sense(dt);

            // Cada pocos segundos cambia de carril para que no vayan todos en fila
            laneTimer -= dt;
            if (laneTimer <= 0f)
            {
                laneTimer = Random.Range(3f, 6f);
                PreferredLane = Random.Range(-1f, 1f) * MaxLaneOffset * 0.6f;
            }

            var next = state.Tick(this, perception, dt);
            if (next != null) ChangeState(next);
        }

        /// <summary>Rellena la percepción: dónde estoy, a qué velocidad, quién tengo delante, si me he atascado.</summary>
        private void Sense(float dt)
        {
            Vector3 pos = transform.position;
            perception.TrackT = Track.NearestT(pos);
            Vector3 center = Track.PointAt(perception.TrackT);
            perception.LateralOffset = Vector3.Dot(pos - center, Track.RightAt(perception.TrackT));
            perception.Speed = Kart.ForwardSpeed;

            if (Throttle > 0.3f && Mathf.Abs(perception.Speed) < 1f) perception.StuckTime += dt;
            else perception.StuckTime = 0f;

            perception.KartAhead = null;
            perception.DistanceAhead = float.MaxValue;
            foreach (var other in KartController.All)
            {
                if (other == Kart) continue;
                Vector3 d = other.transform.position - pos;
                d.y = 0f;
                float ahead = Vector3.Dot(d, transform.forward);
                float side = Vector3.Dot(d, transform.right);
                if (ahead > 0f && ahead < 15f && Mathf.Abs(side) < 2.5f && ahead < perception.DistanceAhead)
                {
                    perception.KartAhead = other;
                    perception.DistanceAhead = ahead;
                }
            }
            if (perception.KartAhead != null)
            {
                Vector3 op = perception.KartAhead.transform.position;
                float ot = Track.NearestT(op);
                perception.AheadLateral = Vector3.Dot(op - Track.PointAt(ot), Track.RightAt(ot));
            }
        }

        /// <summary>Lo llaman los estados para "pisar los pedales".</summary>
        public void SetOutput(float throttle, float steer, bool drift)
        {
            throttle = Mathf.Clamp(throttle, -1f, 1f);
            Throttle = throttle > 0f ? throttle * speedFactor : throttle;
            Steer = Mathf.Clamp(steer, -1f, 1f);
            Drift = drift;
        }

        public void RespawnOnTrack()
        {
            if (race != null) race.RespawnOnTrack(Kart);
            else
            {
                float t = Track.NearestT(transform.position);
                Kart.Teleport(Track.PointAt(t) + Vector3.up, Quaternion.LookRotation(Track.TangentAt(t)));
            }
        }

        private void ChangeState(IRivalState next)
        {
            state?.Exit(this);
            state = next;
            state.Enter(this);
        }

        // Etiqueta encima del rival con su estado (para probar)
        private void OnGUI()
        {
            if (!showDebugLabel || Camera.main == null) return;
            Vector3 s = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
            if (s.z < 0f) return;
            GUI.Label(new Rect(s.x - 80f, Screen.height - s.y - 20f, 200f, 22f), $"{name} · {StateName}");
        }
    }
}