using UnityEngine;

namespace RiggedGrandPrix.AI
{
    /// <summary>Atascado: marcha atrás girando hacia la pista; si no sale, vuelve a la pista directamente.</summary>
    public class RecoverState : RivalStateBase
    {
        private Vector3 startPos;

        public override string Name => "Recuperarse";

        public override void Enter(RivalFSMInput rival)
        {
            base.Enter(rival);
            startPos = rival.transform.position;
        }

        protected override IRivalState OnTick(RivalFSMInput rival, RivalPerception p, float dt)
        {
            if (timeInState < 1.2f)
            {
                Vector3 center = rival.Track.PointAt(p.TrackT) - rival.transform.position;
                float angle = Vector3.SignedAngle(rival.transform.forward, center, Vector3.up);
                rival.SetOutput(-1f, -Mathf.Sign(angle), false);   // marcha atrás: el volante va al revés
                return null;
            }

            bool barelyMoved = Vector3.Distance(startPos, rival.transform.position) < 1f;
            bool offTrack = Mathf.Abs(p.LateralOffset) > rival.RoadHalfWidth + 3f;
            if (barelyMoved || offTrack) rival.RespawnOnTrack();
            return new FollowLineState();
        }
    }
}