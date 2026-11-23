using UnityEngine;

namespace RiggedGrandPrix.Hazards
{
    /// <summary>Nueva York: varios taxis cruzan la calle escalonados.</summary>
    public class TaxiTrafficHazard : CityHazard
    {
        private readonly GameObject[] taxis = new GameObject[3];
        private readonly Rigidbody[] bodies = new Rigidbody[3];
        private float elapsed;
        private bool moving;
        public override string DisplayName => "Taxis";

        protected override void Build()
        {
            for (int i = 0; i < taxis.Length; i++)
            {
                taxis[i] = MakeBox($"Taxi {i}", Vector3.zero, new Vector3(4f, 1.4f, 2f), new Color(1f, 0.82f, 0.1f), true);
                bodies[i] = MakeKinematic(taxis[i]);
                AddHit(taxis[i], 0.3f, 1.2f, trigger: false);
            }
        }

        protected override void OnActivate()
        {
            elapsed = 0f;
            moving = true;
            foreach (var t in taxis) t.SetActive(true);
        }

        private void FixedUpdate()
        {
            if (!moving) return;
            elapsed += Time.fixedDeltaTime;
            float span = roadWidth + 12f;
            for (int i = 0; i < taxis.Length; i++)
            {
                // Cada taxi sale un poco después y en un carril distinto; alternan sentido
                float p = Mathf.Clamp01((elapsed - i * 0.7f) / (activeTime - 1.4f));
                float dir = i % 2 == 0 ? 1f : -1f;
                float x = Mathf.Lerp(-span * 0.5f, span * 0.5f, p) * dir;
                bodies[i].MovePosition(transform.TransformPoint(new Vector3(x, 0.7f, (i - 1) * 5f)));
            }
        }

        protected override void OnRestore()
        {
            moving = false;
            foreach (var t in taxis) t.SetActive(false);
        }
    }
}
