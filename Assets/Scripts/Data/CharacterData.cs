using UnityEngine;

namespace RiggedGrandPrix.Data
{
    /// <summary>
    /// Datos de un personaje. Los atributos van de 1 a 5;
    /// BaseKartStats los traduce a valores físicos.
    /// </summary>
    [CreateAssetMenu(menuName = "Rigged Grand Prix/Character", fileName = "NewCharacter")]
    public class CharacterData : ScriptableObject
    {
        public string displayName = "Teo";

        [Range(1, 5)] public int maxSpeed = 3;
        [Range(1, 5)] public int acceleration = 3;
        [Range(1, 5)] public int turn = 3;
        [Range(1, 5)] public int weight = 3;

        [Tooltip("Rol que prefiere el RaceCoordinator para este personaje cuando es rival.")]
        public RivalRole preferredRole = RivalRole.Runner;

        [Tooltip("Modelo visual del kart (opcional).")]
        public GameObject kartPrefab;
    }
}
