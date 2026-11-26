using RiggedGrandPrix.Karts;
using RiggedGrandPrix.Race;
using UnityEngine;

namespace RiggedGrandPrix.PowerUps
{
    /// <summary>
    /// Factory: crea el objeto concreto sin que la caja ni el kart conozcan las clases.
    /// También tiene los Object Pools de lo que aparece en la pista (aceite y proyectiles).
    /// </summary>
    public class PowerUpFactory : MonoBehaviour
    {
        [Tooltip("Probabilidades por puesto. Si se deja vacío se usa la tabla por defecto.")]
        [SerializeField] private PowerUpTable table;

        private ObjectPool<OilSlick> oilPool;
        private ObjectPool<HomingMissile> missilePool;
        private RaceManager race;

        private void Awake()
        {
            if (table == null) table = PowerUpTable.CreateDefault();
            race = FindAnyObjectByType<RaceManager>();
            oilPool = new ObjectPool<OilSlick>(BuildOilTemplate(), 6, transform);
            missilePool = new ObjectPool<HomingMissile>(BuildMissileTemplate(), 6, transform);
        }

        public PowerUp Create(PowerUpType type) => type switch
        {
            PowerUpType.Turbo => new TurboPowerUp(),
            PowerUpType.OilSlick => new OilSlickPowerUp(oilPool),
            PowerUpType.HomingMissile => new HomingMissilePowerUp(missilePool, race),
            PowerUpType.Shield => new ShieldPowerUp(),
            _ => new PulsePowerUp()
        };

        /// <summary>Elige el tipo según el puesto (PowerUpTable) y lo crea.</summary>
        public PowerUp CreateFor(int position, int total) => Create(table.Sample(position, total));

        public int PositionOf(KartController k) => race != null ? Mathf.Max(1, race.GetPosition(k)) : 1;
        public int TotalKarts => race != null ? Mathf.Max(1, race.Standings.Count) : 1;

        /// <summary>Recoge manchas y proyectiles y vuelve a mostrar las cajas (en la G, al reiniciar).</summary>
        public void ResetState()
        {
            oilPool.ReleaseAll();
            missilePool.ReleaseAll();
            foreach (var box in PowerUpBox.All) box.ResetState();
        }

        // ----- Plantillas hechas por código (sin prefabs que configurar) -----

        private OilSlick BuildOilTemplate()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "OilSlick";
            go.transform.localScale = new Vector3(3f, 0.02f, 3f);
            Destroy(go.GetComponent<Collider>());
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1f, 40f, 1f); // alto en local (la escala Y es muy pequeña)
            go.GetComponent<Renderer>().material.color = new Color(0.08f, 0.06f, 0.12f);
            go.SetActive(false);
            go.transform.SetParent(transform);
            return go.AddComponent<OilSlick>();
        }

        private HomingMissile BuildMissileTemplate()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "HomingMissile";
            go.transform.localScale = Vector3.one * 0.7f;
            go.GetComponent<Collider>().isTrigger = true;
            go.GetComponent<Renderer>().material.color = new Color(1f, 0.35f, 0.1f);
            go.SetActive(false);
            go.transform.SetParent(transform);
            return go.AddComponent<HomingMissile>();
        }
    }
}