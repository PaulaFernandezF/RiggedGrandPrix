using RiggedGrandPrix.Karts;
using RiggedGrandPrix.Karts.Stats;
using RiggedGrandPrix.Race;
using UnityEngine;

namespace RiggedGrandPrix.PowerUps
{
    /// <summary>Turbo: +40 % de velocidad máxima durante 2 s (Decorator).</summary>
    public class TurboPowerUp : PowerUp
    {
        public override PowerUpType Type => PowerUpType.Turbo;
        public override void Use(KartController user) => user.AddModifier(new SpeedBoostModifier(1.4f, 2f));
    }

    /// <summary>Mancha de aceite: se deja detrás del kart.</summary>
    public class OilSlickPowerUp : PowerUp
    {
        private readonly ObjectPool<OilSlick> pool;
        public OilSlickPowerUp(ObjectPool<OilSlick> pool) => this.pool = pool;
        public override PowerUpType Type => PowerUpType.OilSlick;
        public override void Use(KartController user)
        {
            Vector3 pos = user.transform.position - user.transform.forward * 3f;
            pos.y = user.transform.position.y - 0.25f;
            pool.Get(pos, Quaternion.identity).Init(pool, user);
        }
    }

    /// <summary>Proyectil dirigido: persigue al kart que va justo delante.</summary>
    public class HomingMissilePowerUp : PowerUp
    {
        private readonly ObjectPool<HomingMissile> pool;
        private readonly RaceManager race;
        public HomingMissilePowerUp(ObjectPool<HomingMissile> pool, RaceManager race) { this.pool = pool; this.race = race; }
        public override PowerUpType Type => PowerUpType.HomingMissile;
        public override void Use(KartController user)
        {
            KartController target = null;
            if (race != null)
            {
                int pos = race.GetPosition(user);   // 1 = primero
                if (pos > 1) target = race.Standings[pos - 2];
            }
            Vector3 start = user.transform.position + user.transform.forward * 2.5f + Vector3.up * 0.6f;
            pool.Get(start, user.transform.rotation).Init(pool, user, target);
        }
    }

    /// <summary>Escudo: para el siguiente golpe durante 8 s.</summary>
    public class ShieldPowerUp : PowerUp
    {
        public override PowerUpType Type => PowerUpType.Shield;
        public override void Use(KartController user) => user.ActivateShield(8f);
    }

    /// <summary>Onda: frena a todos los karts cercanos (menos a quien la usa).</summary>
    public class PulsePowerUp : PowerUp
    {
        public override PowerUpType Type => PowerUpType.Pulse;
        public override void Use(KartController user)
        {
            foreach (var k in KartController.All)
                if (k != user && Vector3.Distance(k.transform.position, user.transform.position) < 10f)
                    k.TakeHit(new SlowModifier(0.5f, 1.5f));
        }
    }
}