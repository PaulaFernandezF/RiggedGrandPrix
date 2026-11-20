using RiggedGrandPrix.Karts;

namespace RiggedGrandPrix.AI
{
    /// <summary>
    /// Lo que "ve" un rival en este frame. La rellena RivalFSMInput y la leen los estados,
    /// así los estados no calculan nada de la escena: solo deciden.
    /// </summary>
    public class RivalPerception
    {
        public float TrackT;              // posición en la vuelta (0..1)
        public float LateralOffset;       // metros a la derecha (+) o izquierda (-) del centro de la pista
        public float Speed;               // velocidad hacia delante (m/s)
        public KartController KartAhead;  // kart más cercano justo delante (o null)
        public float DistanceAhead;       // metros hasta él
        public float AheadLateral;        // en qué lado de la pista va ese kart
        public float StuckTime;           // segundos intentando avanzar sin moverse

        // ---- El jugador (para los roles Blocker y Hunter) ----
        public KartController PlayerKart;
        /// <summary>Metros por la pista hasta el jugador: + = el jugador va delante, - = va detrás.</summary>
        public float PlayerDelta;
        /// <summary>En qué lado de la pista va el jugador.</summary>
        public float PlayerLateral;
    }
}
