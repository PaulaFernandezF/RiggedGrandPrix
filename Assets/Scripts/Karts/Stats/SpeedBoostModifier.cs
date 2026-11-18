namespace RiggedGrandPrix.Karts.Stats
{
    /// <summary>ConcreteDecorator: turbo. Más velocidad máxima y mucha más aceleración.</summary>
    public class SpeedBoostModifier : StatModifier
    {
        private readonly float factor;

        public SpeedBoostModifier(float factor, float duration) : base(duration) => this.factor = factor;

        public override float MaxSpeed => inner.MaxSpeed * factor;
        public override float Acceleration => inner.Acceleration * factor * 2f;
    }
}
