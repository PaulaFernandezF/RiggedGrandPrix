using UnityEngine;

namespace RiggedGrandPrix.AI
{
    /// <summary>Blocker por delante del jugador: se pone en su carril y frena un poco para cerrarle el paso.</summary>
    public class BlockState : RivalStateBase
    {
        public override string Name => "Bloquear";

        protected override IRivalState OnTick(RivalFSMInput rival, RivalPerception p, float dt)
        {
            if (p.StuckTime > 1.5f) return new RecoverState();
            if (p.PlayerKart == null) { DriveAlongTrack(rival, p, rival.PreferredLane); return null; }

            float d = p.PlayerDelta;                        // + = el jugador va delante
            if (d > 2f) return new InterceptState();        // nos ha pasado: hay que volver a ponerse delante

            if (d < -35f)
            {
                // Vamos demasiado lejos: levantamos el pie para que el jugador se acerque
                DriveAlongTrack(rival, p, rival.PreferredLane, 0.8f);
                return null;
            }

            // Cerca: copiamos su carril para taparle, algo más despacio para que se nos eche encima
            float lane = Mathf.Clamp(p.PlayerLateral, -rival.MaxLaneOffset, rival.MaxLaneOffset);
            DriveAlongTrack(rival, p, lane, d > -8f ? 0.95f : 0.9f);
            return null;
        }
    }
}
