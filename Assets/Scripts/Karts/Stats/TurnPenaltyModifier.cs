namespace RiggedGrandPrix.Karts.Stats
{
    /// <summary>ConcreteDecorator: dirección torpe durante un rato (gira menos).</summary>
    public class TurnPenaltyModifier : StatModifier
    {
        private readonly float factor;

        public TurnPenaltyModifier(float factor, float duration) : base(duration) => this.factor = factor;

        public override float TurnSpeed => inner.TurnSpeed * factor;
    }
}
