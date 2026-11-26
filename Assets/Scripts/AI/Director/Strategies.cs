using System.Collections.Generic;
using System.Linq;
using RiggedGrandPrix.Hazards;

namespace RiggedGrandPrix.AI
{
    /// <summary>
    /// El jugador va 1º con ventaja: activar una trampa en su tramo más débil
    /// de los próximos (donde más tiempo pierde).
    /// </summary>
    public class PressureStrategy : IDirectorStrategy
    {
        private readonly float minLead;
        private readonly int segmentCount;
        public PressureStrategy(float minLead, int segmentCount) { this.minLead = minLead; this.segmentCount = segmentCount; }

        public string Name => "Presión";
        public int Priority => 2;
        public bool IsApplicable(PlayerModel m) => m.Position == 1 && m.GapBehind > minLead;

        public CityHazard ChooseHazard(PlayerModel m, IReadOnlyList<CityHazard> hazards, HazardQuery distanceAhead)
        {
            var candidates = hazards.Where(h => h.IsReady && distanceAhead(h) >= 0f).ToList();
            if (candidates.Count == 0) return null;
            int weak = m.WeakestSegmentAhead(4, segmentCount);
            // La del tramo débil si existe; si no, la más cercana por delante
            return candidates.FirstOrDefault(h => h.segment == weak)
                   ?? candidates.OrderBy(h => distanceAhead(h)).First();
        }
    }

    /// <summary>
    /// El jugador está cerca de la cabeza: cerrarle el paso justo cuando llega
    /// (atajos si los usa; si no, la trampa más próxima por delante).
    /// </summary>
    public class ContainmentStrategy : IDirectorStrategy
    {
        private readonly float maxGap;
        public ContainmentStrategy(float maxGap) => this.maxGap = maxGap;

        public string Name => "Contención";
        public int Priority => 1;
        public bool IsApplicable(PlayerModel m) => m.Position > 1 && m.Position <= 3 && m.GapAhead < maxGap;

        public CityHazard ChooseHazard(PlayerModel m, IReadOnlyList<CityHazard> hazards, HazardQuery distanceAhead)
        {
            var ahead = hazards.Where(h => h.IsReady && distanceAhead(h) >= 0f).OrderBy(h => distanceAhead(h)).ToList();
            if (m.UsesShortcuts)
            {
                var gate = ahead.FirstOrDefault(h => h is ShortcutGate);
                if (gate != null) return gate;
            }
            return ahead.FirstOrDefault();
        }
    }

    /// <summary>
    /// El jugador va muy atrás: no intervenir, para no hacer imposible la remontada.
    /// Tiene la máxima prioridad para que el Director sea difícil pero justo.
    /// </summary>
    public class PassiveStrategy : IDirectorStrategy
    {
        private readonly float maxDeficit;
        public PassiveStrategy(float maxDeficit) => this.maxDeficit = maxDeficit;

        public string Name => "Pasiva";
        public int Priority => 3;
        public bool IsApplicable(PlayerModel m) => m.Position >= m.TotalKarts - 1 || m.GapAhead > maxDeficit;
        public CityHazard ChooseHazard(PlayerModel m, IReadOnlyList<CityHazard> h, HazardQuery q) => null;
    }
}
