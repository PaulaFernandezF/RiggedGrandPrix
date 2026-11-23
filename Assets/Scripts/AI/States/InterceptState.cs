namespace RiggedGrandPrix.AI
{
    /// <summary>Blocker al que el jugador ha adelantado: acelera para volver a ponerse delante.</summary>
    public class InterceptState : RivalStateBase
    {
        public override string Name => "Interceptar";

        public override void Enter(RivalFSMInput rival)
        {
            base.Enter(rival);
            rival.SpeedBoost = 1.05f;
        }

        public override void Exit(RivalFSMInput rival) => rival.SpeedBoost = 1f;

        protected override IRivalState OnTick(RivalFSMInput rival, RivalPerception p, float dt)
        {
            if (p.StuckTime > 1.5f) return new RecoverState();
            if (p.PlayerKart == null || p.PlayerDelta < -3f) return new BlockState(); // ya va delante otra vez

            // Por el carril contrario al del jugador, para adelantarle sin chocar
            float lane = p.PlayerLateral > 0f ? -rival.MaxLaneOffset * 0.7f : rival.MaxLaneOffset * 0.7f;
            DriveAlongTrack(rival, p, lane);
            return null;
        }
    }
}
