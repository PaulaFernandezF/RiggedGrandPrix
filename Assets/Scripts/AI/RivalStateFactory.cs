using RiggedGrandPrix.Data;

namespace RiggedGrandPrix.AI
{
    /// <summary>
    /// Factory: crea el estado "principal" de cada rol. Así ni el coordinador ni los estados
    /// necesitan saber qué clase concreta corresponde a cada rol.
    /// </summary>
    public static class RivalStateFactory
    {
        public static IRivalState Create(RivalRole role) => role switch
        {
            RivalRole.Blocker => new BlockState(),
            RivalRole.Hunter => new ChaseState(),
            _ => new FollowLineState()
        };
    }
}
