namespace RiggedGrandPrix.Karts.Stats
{
    /// <summary>
    /// Component del patrón Decorator: los atributos efectivos de un kart.
    /// El KartController solo lee esta interfaz; no sabe si hay modificadores encima.
    /// </summary>
    public interface IKartStats
    {
        float MaxSpeed { get; }      // m/s
        float Acceleration { get; }  // m/s²
        float TurnSpeed { get; }     // grados/s
        float Weight { get; }        // masa del Rigidbody
    }
}
