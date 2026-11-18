namespace RiggedGrandPrix.Karts.Stats
{
    /// <summary>ConcreteDecorator: ralentización (aceite, golpes...). Baja la velocidad máxima.</summary>
    public class SlowModifier : StatModifier
    {
        private readonly float factor;

        public SlowModifier(float factor, float duration) : base(duration) => this.factor = factor;

        public override float MaxSpeed => inner.MaxSpeed * factor;
    }
}
