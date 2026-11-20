using UnityEngine;

namespace RiggedGrandPrix.AI
{
    /// <summary>Hunter: persigue al jugador por su carril y, si lo tiene cerca, va directo a embestirle.</summary>
    public class ChaseState : RivalStateBase
    {
        public override string Name => "Perseguir";

        public override void Enter(RivalFSMInput rival)
        {
            base.Enter(rival);
            rival.SpeedBoost = 1.1f;
        }

        public override void Exit(RivalFSMInput rival) => rival.SpeedBoost = 1f;

        protected override IRivalState OnTick(RivalFSMInput rival, RivalPerception p, float dt)
        {
            if (p.StuckTime > 1.5f) return new RecoverState();
            if (p.PlayerKart == null || p.PlayerDelta < -5f)
            {
                // Ya le ha pasado: sigue su carrera
                DriveAlongTrack(rival, p, rival.PreferredLane);
                return null;
            }

            if (p.PlayerDelta < 10f)
            {
                // Muy cerca: apunta un poco por delante del jugador para darle un golpe
                var t = p.PlayerKart.transform;
                SteerTo(rival, p, t.position + t.forward * 2f);
            }
            else
            {
                // Lejos: por la pista, en su mismo carril
                float lane = Mathf.Clamp(p.PlayerLateral, -rival.MaxLaneOffset, rival.MaxLaneOffset);
                DriveAlongTrack(rival, p, lane);
            }
            return null;
        }
    }
}
