using System.Collections.Generic;

namespace RiggedGrandPrix.Race
{
    /// <summary>Resultado de una carrera para el jugador. Lo crea el RaceManager.</summary>
    public class RaceResult
    {
        public int PlayerPosition { get; }
        public int RequiredPosition { get; }
        public List<float> Times { get; }

        public RaceResult(int playerPosition, int requiredPosition, List<float> times)
        {
            PlayerPosition = playerPosition;
            RequiredPosition = requiredPosition;
            Times = times;
        }

        /// <summary>Gana si queda en el puesto exigido o mejor (1 es el mejor).</summary>
        public bool IsVictory => PlayerPosition <= RequiredPosition;
    }
}