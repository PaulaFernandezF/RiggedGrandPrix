using UnityEngine;

namespace RiggedGrandPrix.Hazards
{
    /// <summary>Tokio: se cierra la valla de un atajo. Colócala en la entrada del atajo, no en la carretera principal.</summary>
    public class ShortcutGate : CityHazard
    {
        [SerializeField] private float gateWidth = 6f;
        private GameObject gate;
        public override string DisplayName => "Cierre de atajo";

        protected override void Build()
        {
            gate = MakeBox("Valla", new Vector3(0f, 1f, 0f), new Vector3(gateWidth, 2f, 0.4f), new Color(1f, 0.55f, 0.1f), true);
        }

        protected override void OnActivate() => gate.SetActive(true);
        protected override void OnRestore() => gate.SetActive(false);
    }
}
