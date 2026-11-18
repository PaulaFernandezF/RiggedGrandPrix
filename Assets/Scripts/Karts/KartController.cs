using System.Collections.Generic;
using RiggedGrandPrix.Data;
using RiggedGrandPrix.Karts.Stats;
using UnityEngine;

namespace RiggedGrandPrix.Karts
{
    /// <summary>
    /// Física arcade del kart. Es el contexto de dos patrones:
    ///  - Strategy: lee los controles de un IKartInput sin saber quién conduce.
    ///  - Decorator: sus atributos son una cadena de StatModifier sobre BaseKartStats.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class KartController : MonoBehaviour
    {
        [Header("Personaje")]
        [SerializeField] private CharacterData character;

        [Header("Conducción")]
        [SerializeField] private float reverseSpeedFactor = 0.4f;
        [SerializeField] private float coastDeceleration = 4f;
        [SerializeField] private float brakeMultiplier = 2f;

        [Header("Agarre y derrape")]
        [Tooltip("Rapidez con la que se elimina el deslizamiento lateral (1/s).")]
        [SerializeField] private float gripStrength = 15f;
        [SerializeField] private float driftGripStrength = 1.2f;
        [SerializeField] private float driftTurnMultiplier = 1.6f;

        [Header("Suelo")]
        [SerializeField] private float extraGravity = 20f;
        [SerializeField] private float groundCheckDistance = 0.6f;

        private IKartInput input;
        private Rigidbody rb;
        private IKartStats baseStats, stats;
        private readonly List<StatModifier> modifiers = new List<StatModifier>();
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        // ---- Consultas (cámara, depuración y, más adelante, HUD e IA) ----
        public IKartStats Stats => stats;
        public CharacterData Character => character;
        public IReadOnlyList<StatModifier> Modifiers => modifiers;
        public float ForwardSpeed => Vector3.Dot(rb.linearVelocity, transform.forward);
        public bool IsGrounded { get; private set; }
        public bool IsDrifting { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // Sin fricción: la conducción la decide el script, no el suelo
            var col = GetComponent<Collider>();
            if (col != null)
                col.material = new PhysicsMaterial("KartSinFriccion")
                {
                    dynamicFriction = 0f,
                    staticFriction = 0f,
                    frictionCombine = PhysicsMaterialCombine.Minimum
                };

            input = GetComponent<IKartInput>();
            if (input == null)
                Debug.LogError($"[KartController] {name}: falta un componente que implemente IKartInput (Player Input (Kart)).", this);

            if (character != null) SetCharacter(character);
            else Debug.LogError($"[KartController] {name}: falta asignar el personaje (Character).", this);

            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        // ---------------- Configuración ----------------

        /// <summary>Strategy: cambia quién conduce (jugador o IA).</summary>
        public void SetInput(IKartInput newInput) => input = newInput;

        public void SetCharacter(CharacterData data)
        {
            character = data;
            baseStats = new BaseKartStats(data);
            RebuildStats();
        }

        /// <summary>Añade un efecto temporal (turbo, aceite...) encima de la cadena.</summary>
        public void AddModifier(StatModifier modifier)
        {
            modifiers.Add(modifier);
            RebuildStats();
        }

        /// <summary>Vuelve a la salida, parado y sin efectos.</summary>
        public void ResetToSpawn()
        {
            modifiers.Clear();
            RebuildStats();
            rb.position = spawnPosition;
            rb.rotation = spawnRotation;
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        /// <summary>Reconstruye la cadena: base → mod1 → mod2 → ...</summary>
        private void RebuildStats()
        {
            if (baseStats == null) return;
            IKartStats current = baseStats;
            foreach (var m in modifiers)
            {
                m.Wrap(current);
                current = m;
            }
            stats = current;
            rb.mass = stats.Weight;
        }

        private void TickModifiers(float dt)
        {
            bool changed = false;
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                modifiers[i].Tick(dt);
                if (modifiers[i].IsExpired) { modifiers.RemoveAt(i); changed = true; }
            }
            if (changed) RebuildStats();
        }

        // ---------------- Física ----------------

        private void FixedUpdate()
        {
            if (input == null || stats == null) return;
            float dt = Time.fixedDeltaTime;
            TickModifiers(dt);

            IsGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

            if (IsGrounded)
            {
                // Descomponemos la velocidad en: adelante, lateral y vertical
                Vector3 v = rb.linearVelocity;
                Vector3 fwd = transform.forward, right = transform.right;
                float forward = Vector3.Dot(v, fwd);
                float lateral = Vector3.Dot(v, right);

                // 1. Acelerar / frenar / marcha atrás
                float throttle = input.Throttle;
                float target = throttle >= 0f ? throttle * stats.MaxSpeed : throttle * stats.MaxSpeed * reverseSpeedFactor;
                float rate;
                if (Mathf.Abs(throttle) < 0.01f) rate = coastDeceleration;                      // soltar: se para poco a poco
                else if (forward * throttle < 0f) rate = stats.Acceleration * brakeMultiplier;  // sentido contrario: frenar
                else rate = stats.Acceleration;
                forward = Mathf.MoveTowards(forward, target, rate * dt);

                // 2. Derrape: con poco agarre el deslizamiento lateral dura más
                IsDrifting = input.Drift && Mathf.Abs(forward) > 5f;
                float grip = IsDrifting ? driftGripStrength : gripStrength;
                lateral *= Mathf.Exp(-grip * dt);   // decae igual a cualquier framerate

                // 3. Giro: proporcional a la velocidad (parado no gira) e invertido marcha atrás
                float speedFactor = Mathf.Clamp01(Mathf.Abs(forward) / (stats.MaxSpeed * 0.5f));
                float turn = input.Steer * stats.TurnSpeed * speedFactor * Mathf.Sign(forward);
                if (IsDrifting) turn *= driftTurnMultiplier;
                rb.angularVelocity = new Vector3(0f, turn * Mathf.Deg2Rad, 0f);

                rb.linearVelocity = fwd * forward + right * lateral + Vector3.up * v.y;
            }
            else IsDrifting = false;

            // Gravedad extra para que no "flote"
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
    }
}
