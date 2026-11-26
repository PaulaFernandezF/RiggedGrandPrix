using System.Collections.Generic;
using RiggedGrandPrix.Karts;
using UnityEngine;

namespace RiggedGrandPrix.PowerUps
{
    /// <summary>
    /// Caja en la pista: al atravesarla, el kart recibe un objeto de la fábrica
    /// (según su puesto). Desaparece y vuelve a aparecer a los pocos segundos.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PowerUpBox : MonoBehaviour
    {
        [SerializeField] private float respawnTime = 4f;
        [SerializeField] private float spinSpeed = 90f;
        [SerializeField] private Color color = new Color(1f, 0.75f, 0.1f, 1f);

        public static IReadOnlyList<PowerUpBox> All => all;
        private static readonly List<PowerUpBox> all = new List<PowerUpBox>();

        private PowerUpFactory factory;
        private Renderer[] renderers;
        private Collider col;
        private float hiddenUntil;
        private Vector3 basePos;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) r.material.color = color;
            col = GetComponent<Collider>();
            col.isTrigger = true;
            basePos = transform.position;
        }

        private void OnEnable() { if (!all.Contains(this)) all.Add(this); }
        private void OnDisable() => all.Remove(this);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearStatics() => all.Clear();

        private void Start() => factory = FindAnyObjectByType<PowerUpFactory>();

        private void Update()
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
            transform.position = basePos + Vector3.up * (Mathf.Sin(Time.time * 2f) * 0.2f);
            if (hiddenUntil > 0f && Time.time >= hiddenUntil) SetVisible(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (factory == null || hiddenUntil > 0f) return;
            var kart = other.GetComponentInParent<KartController>();
            if (kart == null || kart.HeldPowerUp != null) return;
            kart.ReceivePowerUp(factory.CreateFor(factory.PositionOf(kart), factory.TotalKarts));
            SetVisible(false);
            hiddenUntil = Time.time + respawnTime;
        }

        private void SetVisible(bool visible)
        {
            foreach (var r in renderers) r.enabled = visible;
            col.enabled = visible;
            if (visible) hiddenUntil = 0f;
        }

        public void ResetState() => SetVisible(true);
    }
}