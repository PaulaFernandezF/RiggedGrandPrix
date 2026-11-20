using System.Collections.Generic;
using RiggedGrandPrix.Data;
using RiggedGrandPrix.Karts;
using RiggedGrandPrix.Race;
using UnityEngine;

namespace RiggedGrandPrix.AI
{
    /// <summary>
    /// Mediator: los rivales no hablan entre ellos, solo con el coordinador.
    /// Cada pocos segundos (y cuando el jugador adelanta) reparte los roles:
    ///  - Blocker: el rival que va justo delante del jugador → se le pone delante y le cierra.
    ///  - Hunter: el rival que va justo detrás del jugador → le persigue y le embiste.
    ///  - Runner: todos los demás → a ganar la carrera.
    /// Prefiere al personaje cuyo rol favorito (CharacterData.preferredRole) coincide.
    /// </summary>
    public class RaceCoordinator : MonoBehaviour
    {
        [Tooltip("Roles que se usan en esta carrera (en la sección H dependerán de la ciudad).")]
        [SerializeField] private List<RivalRole> activeRoles = new List<RivalRole> { RivalRole.Runner, RivalRole.Blocker, RivalRole.Hunter };
        [SerializeField] private float reassignInterval = 3f;
        [Tooltip("Distancia máxima (m por la pista) al jugador para poder ser Blocker o Hunter.")]
        [SerializeField] private float roleRange = 45f;
        [Tooltip("Metros de ventaja que da tener ese rol como favorito.")]
        [SerializeField] private float preferredBonus = 10f;

        private readonly List<RivalFSMInput> rivals = new List<RivalFSMInput>();
        private RaceManager race;
        private float timer;

        public IReadOnlyList<RivalFSMInput> Rivals => rivals;

        /// <summary>Para la sección H: cada ciudad activa unos roles y otra frecuencia.</summary>
        public void Configure(IList<RivalRole> roles, float interval)
        {
            activeRoles = new List<RivalRole>(roles);
            reassignInterval = interval;
        }

        public void Register(RivalFSMInput rival) { if (!rivals.Contains(rival)) rivals.Add(rival); }
        public void Unregister(RivalFSMInput rival) => rivals.Remove(rival);

        private void Awake() => race = FindAnyObjectByType<RaceManager>();
        private void OnEnable() => RaceEvents.Overtake += OnOvertake;
        private void OnDisable() => RaceEvents.Overtake -= OnOvertake;

        private void OnOvertake(KartController who, KartController overtaken)
        {
            // Si el jugador adelanta (o le adelantan), los papeles cambian ya
            if (who.GetComponent<PlayerInput>() != null || overtaken.GetComponent<PlayerInput>() != null)
                timer = 0f;
        }

        private void Update()
        {
            if (race != null && !race.IsRunning) return;
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = reassignInterval;
            AssignRoles();
        }

        private void AssignRoles()
        {
            var roles = new Dictionary<RivalFSMInput, RivalRole>();
            foreach (var r in rivals) roles[r] = RivalRole.Runner;

            if (activeRoles.Contains(RivalRole.Blocker))
            {
                // El más cercano entre los que van DELANTE del jugador (PlayerDelta < 0)
                var b = Pick(r => -r.Perception.PlayerDelta, RivalRole.Blocker, roles);
                if (b != null) roles[b] = RivalRole.Blocker;
            }
            if (activeRoles.Contains(RivalRole.Hunter))
            {
                // El más cercano entre los que van DETRÁS del jugador (PlayerDelta > 0)
                var h = Pick(r => r.Perception.PlayerDelta, RivalRole.Hunter, roles);
                if (h != null) roles[h] = RivalRole.Hunter;
            }

            foreach (var pair in roles) pair.Key.SetRole(pair.Value);
        }

        /// <summary>
        /// Elige el rival libre más cercano al jugador por el lado indicado.
        /// distance(r) debe ser positiva si el rival está en ese lado; se ignoran los que están lejos.
        /// </summary>
        private RivalFSMInput Pick(System.Func<RivalFSMInput, float> distance, RivalRole role, Dictionary<RivalFSMInput, RivalRole> current)
        {
            RivalFSMInput best = null;
            float bestScore = float.MaxValue;
            foreach (var r in rivals)
            {
                if (current[r] != RivalRole.Runner) continue;     // ya tiene otro rol
                float d = distance(r);
                if (d <= 0f || d > roleRange) continue;
                float score = d;
                var c = r.Kart.Character;
                if (c != null && c.preferredRole == role) score -= preferredBonus;
                if (score < bestScore) { bestScore = score; best = r; }
            }
            return best;
        }
    }
}
