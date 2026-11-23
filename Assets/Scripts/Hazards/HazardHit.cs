using RiggedGrandPrix.Karts;
using RiggedGrandPrix.Karts.Stats;
using UnityEngine;

namespace RiggedGrandPrix.Hazards
{
    /// <summary>Frena a los karts que chocan (o están dentro, si es un trigger) con esta pieza.</summary>
    public class HazardHit : MonoBehaviour
    {
        private float slowFactor = 0.5f, duration = 1f;
        private bool continuous;
        private float nextTick;

        public void Setup(float slow, float dur, bool continuous)
        {
            slowFactor = slow; duration = dur; this.continuous = continuous;
        }

        private void OnCollisionEnter(Collision c) => Hit(c.collider);
        private void OnTriggerEnter(Collider c) => Hit(c);

        private void OnTriggerStay(Collider c)
        {
            if (!continuous || Time.time < nextTick) return;
            nextTick = Time.time + duration * 0.8f;
            Hit(c);
        }

        private void Hit(Collider c)
        {
            var kart = c.GetComponentInParent<KartController>();
            if (kart != null) kart.TakeHit(new SlowModifier(slowFactor, duration));
        }
    }
}
