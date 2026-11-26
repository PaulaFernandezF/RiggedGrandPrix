using System.Collections.Generic;
using RiggedGrandPrix.Hazards;

namespace RiggedGrandPrix.AI
{
    /// <summary>Strategy: una táctica del Director.</summary>
    public interface IDirectorStrategy
    {
        string Name { get; }
        /// <summary>Si varias son aplicables, gana la de mayor prioridad.</summary>
        int Priority { get; }
        bool IsApplicable(PlayerModel m);
        /// <summary>Elige qué trampa activar (o null para no hacer nada).</summary>
        CityHazard ChooseHazard(PlayerModel m, IReadOnlyList<CityHazard> hazards, HazardQuery query);
    }

    /// <summary>Ayuda para filtrar trampas por distancia por delante del jugador.</summary>
    public delegate float HazardQuery(CityHazard h);
}
