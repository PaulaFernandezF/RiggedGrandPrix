using UnityEngine;

namespace RiggedGrandPrix.AI
{
    /// <summary>
    /// Base común de los estados: cuenta el tiempo en el estado y sabe conducir
    /// hacia un punto de la pista (lo usan todos).
    /// </summary>
    public abstract class RivalStateBase : IRivalState
    {
        public abstract string Name { get; }
        protected float timeInState;

        public virtual void Enter(RivalFSMInput rival) => timeInState = 0f;

        public IRivalState Tick(RivalFSMInput rival, RivalPerception p, float dt)
        {
            timeInState += dt;
            return OnTick(rival, p, dt);
        }

        protected abstract IRivalState OnTick(RivalFSMInput rival, RivalPerception p, float dt);

        public virtual void Exit(RivalFSMInput rival) { }

        /// <summary>Apunta a un punto de la trazada por delante, desplazado 'lateral' metros (carril).</summary>
        protected void DriveAlongTrack(RivalFSMInput rival, RivalPerception p, float lateral, float throttleScale = 1f)
        {
            var track = rival.Track;
            float lookAhead = 6f + Mathf.Abs(p.Speed) * 0.45f;           // más rápido = mira más lejos
            float t = p.TrackT + lookAhead / Mathf.Max(track.Length, 1f);
            Vector3 target = track.PointAt(t) + track.RightAt(t) * lateral;
            SteerTo(rival, p, target, throttleScale);
        }

        /// <summary>Gira hacia el objetivo; frena en curvas cerradas y derrapa si va rápido.</summary>
        protected void SteerTo(RivalFSMInput rival, RivalPerception p, Vector3 target, float throttleScale = 1f)
        {
            Vector3 dir = target - rival.transform.position;
            dir.y = 0f;
            Vector3 fwd = rival.transform.forward;
            fwd.y = 0f;
            float angle = Vector3.SignedAngle(fwd, dir, Vector3.up);   // + = objetivo a la derecha

            float steer = Mathf.Clamp(angle / 30f, -1f, 1f);
            float throttle = Mathf.Abs(angle) > 45f ? 0.55f : Mathf.Abs(angle) > 25f ? 0.8f : 1f;
            bool drift = Mathf.Abs(angle) > 35f && p.Speed > 12f;
            rival.SetOutput(throttle * throttleScale, steer, drift);
        }
    }
}