namespace RiggedGrandPrix.Karts.Stats
{
    /// <summary>
    /// Decorator abstracto: envuelve otro IKartStats y por defecto deja pasar sus valores.
    /// Cada modificador concreto solo cambia lo que le interesa.
    /// Tiene duración: cuando caduca, el KartController reconstruye la cadena sin él.
    /// </summary>
    public abstract class StatModifier : IKartStats
    {
        protected IKartStats inner;
        private float timeLeft;

        protected StatModifier(float duration) => timeLeft = duration;

        /// <summary>Lo coloca encima de otro IKartStats (la base u otro modificador).</summary>
        public void Wrap(IKartStats wrapped) => inner = wrapped;

        public void Tick(float dt) => timeLeft -= dt;
        public bool IsExpired => timeLeft <= 0f;

        public virtual float MaxSpeed => inner.MaxSpeed;
        public virtual float Acceleration => inner.Acceleration;
        public virtual float TurnSpeed => inner.TurnSpeed;
        public virtual float Weight => inner.Weight;
    }
}
