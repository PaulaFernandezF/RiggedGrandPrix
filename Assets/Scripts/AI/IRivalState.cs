namespace RiggedGrandPrix.AI
{
    /// <summary>State del patrón State: un comportamiento del rival.</summary>
    public interface IRivalState
    {
        string Name { get; }
        void Enter(RivalFSMInput rival);
        /// <summary>Decide los controles de este frame. Devuelve el siguiente estado, o null para seguir en este.</summary>
        IRivalState Tick(RivalFSMInput rival, RivalPerception p, float dt);
        void Exit(RivalFSMInput rival);
    }
}