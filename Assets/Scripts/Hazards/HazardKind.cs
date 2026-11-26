namespace RiggedGrandPrix.Hazards
{
    /// <summary>Tipo de trampa a generar en un circuito (cada ciudad usa los suyos).</summary>
    public enum HazardKind
    {
        TrafficLight,   // Madrid
        FloodGate,      // Venecia
        TrainCrossing,  // Tokio
        ShortcutGate,   // Tokio (en atajos)
        TaxiTraffic,    // Nueva York
        Roadworks,      // Nueva York
        Sandstorm       // El Cairo
    }
}
