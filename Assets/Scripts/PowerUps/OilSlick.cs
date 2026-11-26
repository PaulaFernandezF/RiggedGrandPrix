using RiggedGrandPrix.Karts;
using RiggedGrandPrix.Karts.Stats;
using UnityEngine;

namespace RiggedGrandPrix.PowerUps
{
    /// <summary>Mancha en el suelo: quien la pisa pierde giro y algo de velocidad.</summary>
    public class OilSlick : MonoBehaviour, IPoolable
    {
        [SerializeField] private float lifetime = 20f;
        private ObjectPool<OilSlick> pool;
        private KartController owner;
        private float age;

        public void Init(ObjectPool<OilSlick> pool, KartController owner)
        {
            this.pool = pool;
            this.owner = owner;
        }

        public void OnSpawn() => age = 0f;
        public void OnDespawn() => owner = null;

        private void Update()
        {
            age += Time.deltaTime;
            if (age > lifetime) pool.Release(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            var kart = other.GetComponentInParent<KartController>();
            if (kart == null) return;
            if (kart == owner && age < 1f) return; // que no pises tu propia mancha al soltarla
            kart.TakeHit(new TurnPenaltyModifier(0.3f, 1.5f));
            kart.TakeHit(new SlowModifier(0.7f, 1.5f));
            pool.Release(this);
        }
    }
}