namespace RiggedGrandPrix.AI
{
    /// <summary>Tiene un kart pegado delante: se abre por el lado contrario hasta pasarlo.</summary>
    public class OvertakeState : RivalStateBase
    {
        private readonly float side;

        public OvertakeState(float blockerLateral) => side = blockerLateral > 0f ? -1f : 1f;

        public override string Name => "Adelantar";

        protected override IRivalState OnTick(RivalFSMInput rival, RivalPerception p, float dt)
        {
            if (p.StuckTime > 1.5f) return new RecoverState();
            if (timeInState > 4f || p.KartAhead == null || p.DistanceAhead > 14f) return RivalStateFactory.Create(rival.Role);

            DriveAlongTrack(rival, p, side * rival.MaxLaneOffset);
            return null;
        }
    }
}