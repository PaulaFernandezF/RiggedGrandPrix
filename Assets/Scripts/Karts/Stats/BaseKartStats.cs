using RiggedGrandPrix.Data;

namespace RiggedGrandPrix.Karts.Stats
{
    /// <summary>
    /// ConcreteComponent del Decorator: convierte los atributos 1..5 del personaje
    /// (CharacterData) en números de física.
    /// </summary>
    public class BaseKartStats : IKartStats
    {
        private readonly CharacterData data;

        public BaseKartStats(CharacterData data) => this.data = data;

        public float MaxSpeed => 16f + 2.5f * data.maxSpeed;        // 3 → 23,5 m/s (~85 km/h)
        public float Acceleration => 6f + 2f * data.acceleration;   // 3 → 12 m/s²
        public float TurnSpeed => 110f + 15f * data.turn;           // 3 → 155 °/s
        public float Weight => 0.8f + 0.3f * data.weight;           // 3 → 1,7
    }
}
