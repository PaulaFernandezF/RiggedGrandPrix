namespace RiggedGrandPrix.AI
{
    /// <summary>Estado normal: ir por la pista en su carril.</summary>
    public class FollowLineState : RivalStateBase
    {
        public override string Name => "Seguir trazada";

        protected override IRivalState OnTick(RivalFSMInput rival, RivalPerception p, float dt)
        {
            if (rival.Role != Data.RivalRole.Runner) return RivalStateFactory.Create(rival.Role);
            if (p.StuckTime > 1.5f) return new RecoverState();
            if (p.KartAhead != null && p.DistanceAhead < 9f && p.Speed > 8f && timeInState > 0.5f)
                return new OvertakeState(p.AheadLateral);

            DriveAlongTrack(rival, p, rival.PreferredLane);
            return null;
        }
    }
}