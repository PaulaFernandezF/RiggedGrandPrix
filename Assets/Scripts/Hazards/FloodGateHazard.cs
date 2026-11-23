using UnityEngine;

namespace RiggedGrandPrix.Hazards
{
    /// <summary>Venecia: se abre una compuerta y se inunda un tramo (el agua frena mientras estás dentro).</summary>
    public class FloodGateHazard : CityHazard
    {
        private GameObject water, gate;
        public override string DisplayName => "Compuerta";

        protected override void Build()
        {
            water = MakeBox("Agua", new Vector3(0f, 0.05f, 6f), new Vector3(roadWidth, 0.1f, 14f), new Color(0.2f, 0.45f, 0.85f), true);
            water.GetComponent<BoxCollider>().size = new Vector3(1f, 20f, 1f); // trigger alto (escala Y pequeña)
            AddHit(water, 0.6f, 0.6f, trigger: true);
            gate = MakeBox("Compuerta", new Vector3(roadWidth * 0.3f, 0.75f, 0f), new Vector3(roadWidth * 0.4f, 1.5f, 0.5f),
                           new Color(0.35f, 0.25f, 0.15f), true);
            AddHit(gate, 0.4f, 1f, trigger: false);
        }

        protected override void OnActivate() { water.SetActive(true); gate.SetActive(true); }
        protected override void OnRestore() { water.SetActive(false); gate.SetActive(false); }
    }
}
