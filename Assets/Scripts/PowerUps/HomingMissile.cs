using RiggedGrandPrix.Karts;
using RiggedGrandPrix.Karts.Stats;
using UnityEngine;

namespace RiggedGrandPrix.PowerUps
{
    /// <summary>Proyectil que persigue a su objetivo y lo frena al impactar.</summary>
    public class HomingMissile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 38f;
        [SerializeField] private float turnSpeed = 240f;
        [SerializeField] private float lifetime = 6f;

        private ObjectPool<HomingMissile> pool;
        private KartController owner, target;
        private float age;

        public void Init(ObjectPool<HomingMissile> pool, KartController owner, KartController target)
        {
            this.pool = pool;
            this.owner = owner;
            this.target = target;
        }

        public void OnSpawn() => age = 0f;
        public void OnDespawn() { owner = null; target = null; }

        private void Update()
        {
            age += Time.deltaTime;
            if (age > lifetime) { pool.Release(this); return; }

            if (target != null)
            {
                Vector3 dir = (target.transform.position + Vector3.up * 0.4f) - transform.position;
                Quaternion want = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, want, turnSpeed * Time.deltaTime);
            }
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            var kart = other.GetComponentInParent<KartController>();
            if (kart == null || kart == owner) return;
            kart.TakeHit(new SlowModifier(0.25f, 1.2f));
            pool.Release(this);
        }
    }
}