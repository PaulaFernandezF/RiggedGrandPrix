using UnityEngine;

namespace RiggedGrandPrix.Hazards
{
    /// <summary>Madrid: el semáforo se pone en rojo y baja una barrera que tapa 2/3 de la carretera.</summary>
    public class TrafficLightHazard : CityHazard
    {
        private GameObject barrier;
        private Renderer lamp;
        public override string DisplayName => "Semáforo";

        protected override void Build()
        {
            MakeBox("Semáforo", new Vector3(-roadWidth * 0.5f - 1f, 2f, 0f), new Vector3(0.3f, 4f, 0.3f), Color.black, true);
            lamp = MakeBox("Luz", new Vector3(-roadWidth * 0.5f - 1f, 4.2f, 0f), Vector3.one * 0.8f, Color.green, false, PrimitiveType.Sphere)
                   .GetComponent<Renderer>();
            barrier = MakeBox("Barrera", Vector3.zero, new Vector3(roadWidth * 0.66f, 1f, 0.6f), new Color(0.9f, 0.1f, 0.1f), true);
            AddHit(barrier, 0.4f, 1.2f, trigger: false);
        }

        protected override void OnActivate()
        {
            lamp.material.color = Color.red;
            // La barrera tapa un lado al azar: siempre queda hueco para pasar
            float side = Random.value < 0.5f ? -1f : 1f;
            barrier.transform.localPosition = new Vector3(side * roadWidth * 0.17f, 0.5f, 0f);
            barrier.SetActive(true);
        }

        protected override void OnRestore()
        {
            lamp.material.color = Color.green;
            barrier.SetActive(false);
        }
    }
}
