using System.Collections.Generic;
using RiggedGrandPrix.Karts;
using UnityEngine;

namespace RiggedGrandPrix.PowerUps
{
    public enum PowerUpType { Turbo, OilSlick, HomingMissile, Shield, Pulse }

    /// <summary>Producto del patrón Factory: cada objeto sabe qué hace al usarse.</summary>
    public abstract class PowerUp
    {
        public abstract PowerUpType Type { get; }
        public abstract void Use(KartController user);
    }

    /// <summary>Objeto que se puede reutilizar desde un ObjectPool.</summary>
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }

    /// <summary>
    /// Object Pool genérico: reutiliza objetos en vez de Instantiate/Destroy
    /// (menos basura y sin tirones).
    /// </summary>
    public class ObjectPool<T> where T : Component, IPoolable
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> available = new Queue<T>();
        private readonly List<T> active = new List<T>();

        public ObjectPool(T prefab, int prewarm, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;
            for (int i = 0; i < prewarm; i++) available.Enqueue(Create());
        }

        private T Create()
        {
            T item = Object.Instantiate(prefab, parent);
            item.gameObject.SetActive(false);
            return item;
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T item = available.Count > 0 ? available.Dequeue() : Create();
            item.transform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            active.Add(item);
            item.OnSpawn();
            return item;
        }

        public void Release(T item)
        {
            if (!active.Remove(item)) return; // ya estaba devuelto
            item.OnDespawn();
            item.gameObject.SetActive(false);
            available.Enqueue(item);
        }

        public void ReleaseAll()
        {
            for (int i = active.Count - 1; i >= 0; i--) Release(active[i]);
        }
    }
}