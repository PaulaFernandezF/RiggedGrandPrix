using RiggedGrandPrix.Karts;

namespace RiggedGrandPrix.Race
{
    /// <summary>Lleva la cuenta de vueltas y checkpoints de un kart.</summary>
    public class LapTracker
    {
        public KartController Kart { get; }
        public bool IsPlayer { get; }

        public int Lap;                 // vueltas completadas
        public int NextCheckpoint = 1;  // la salida está justo después de la meta (checkpoint 0)
        public bool Finished;
        public float FinishTime;

        public LapTracker(KartController kart, bool isPlayer)
        {
            Kart = kart;
            IsPlayer = isPlayer;
        }

        public void Reset()
        {
            Lap = 0;
            NextCheckpoint = 1;
            Finished = false;
            FinishTime = 0f;
        }
    }
}