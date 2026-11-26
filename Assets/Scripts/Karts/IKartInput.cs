namespace RiggedGrandPrix.Karts
{
    /// <summary>
    /// Strategy: quién conduce el kart (jugador, IA...).
    /// El KartController solo conoce esta interfaz.
    /// </summary>
    public interface IKartInput
    {
        /// <summary>-1 marcha atrás, 0 nada, 1 acelerar a fondo.</summary>
        float Throttle { get; }

        /// <summary>-1 izquierda, 0 recto, 1 derecha.</summary>
        float Steer { get; }

        /// <summary>True mientras se mantiene el botón de derrape.</summary>
        bool Drift { get; }

        /// <summary>True en el frame en que se pulsa "usar objeto".</summary>
        bool WantsUsePowerUp();
    }
}