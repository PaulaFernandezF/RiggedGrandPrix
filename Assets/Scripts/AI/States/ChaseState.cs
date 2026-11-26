using UnityEngine;

namespace RiggedGrandPrix.AI
{
    /// <summary>
    /// Hunter: persigue al jugador por su carril y, si lo tiene cerca y en marcha, va a embestirle.
    /// Para que sea justo: no embiste a un kart parado (lo esquiva y sigue su carrera)
    /// y, tras un intento de golpe, descansa unos segundos.
    /// </summary>
    public class ChaseState : RivalStateBase
    {
        private const float RamMaxTime = 2.5f;   // segundos máximos intentando golpear
        private const float RamCooldown = 4f;    // segundos de descanso tras un intento
        private float ramTime, cooldown;

        public override string Name => "Perseguir";

        public override void Enter(RivalFSMInput rival)
        {
            base.Enter(rival);
            rival.SpeedBoost = 1.05f;
        }

        public override void Exit(RivalFSMInput rival) => rival.SpeedBoost = 1f;

        protected override IRivalState OnTick(RivalFSMInput rival, RivalPerception p, float dt)
        {
            if (p.StuckTime > 1.5f) return new RecoverState();
            if (cooldown > 0f) cooldown -= dt;

            if (p.PlayerKart == null || p.PlayerDelta < -5f)
            {
                // Ya le ha pasado: sigue su carrera
                DriveAlongTrack(rival, p, rival.PreferredLane);
                return null;
            }

            bool playerMoving = p.PlayerKart.ForwardSpeed > 6f;

            if (!playerMoving && p.PlayerDelta < 15f)
            {
                // El jugador está parado: lo esquivamos por el otro lado y seguimos
                float lane = p.PlayerLateral > 0f ? -rival.MaxLaneOffset : rival.MaxLaneOffset;
                DriveAlongTrack(rival, p, lane);
                return null;
            }

            if (playerMoving && p.PlayerDelta < 10f && cooldown <= 0f)
            {
                // Muy cerca: apunta un poco por delante del jugador para darle un golpe
                ramTime += dt;
                if (ramTime > RamMaxTime) { ramTime = 0f; cooldown = RamCooldown; }
                var t = p.PlayerKart.transform;
                SteerTo(rival, p, t.position + t.forward * 2f);
            }
            else
            {
                // Lejos (o descansando): por la pista, en su mismo carril
                float lane = Mathf.Clamp(p.PlayerLateral, -rival.MaxLaneOffset, rival.MaxLaneOffset);
                DriveAlongTrack(rival, p, lane);
            }
            return null;
        }
    }
}
