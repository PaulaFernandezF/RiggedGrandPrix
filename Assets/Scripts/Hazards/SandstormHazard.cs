using UnityEngine;

namespace RiggedGrandPrix.Hazards
{
    /// <summary>El Cairo: tormenta de arena. Baja la visibilidad (niebla) y frena un poco en la zona.</summary>
    public class SandstormHazard : CityHazard
    {
        [SerializeField] private float stormDensity = 0.05f;
        private GameObject zone;
        private bool prevFog;
        private float prevDensity;
        private Color prevColor;
        private bool storming;

        public override string DisplayName => "Tormenta de arena";

        protected override void Build()
        {
            zone = MakeBox("Zona tormenta", new Vector3(0f, 2f, 20f), new Vector3(roadWidth * 2f, 4f, 40f), new Color(0.85f, 0.7f, 0.4f), true);
            zone.GetComponent<Renderer>().enabled = false;
            AddHit(zone, 0.85f, 0.6f, trigger: true);
            activeTime = Mathf.Max(activeTime, 7f);
        }

        protected override void OnActivate()
        {
            prevFog = RenderSettings.fog;
            prevDensity = RenderSettings.fogDensity;
            prevColor = RenderSettings.fogColor;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.82f, 0.68f, 0.45f);
            RenderSettings.fogDensity = stormDensity;
            zone.SetActive(true);
            storming = true;
        }

        protected override void OnRestore()
        {
            if (storming)
            {
                RenderSettings.fog = prevFog;
                RenderSettings.fogDensity = prevDensity;
                RenderSettings.fogColor = prevColor;
                storming = false;
            }
            zone.SetActive(false);
        }
    }
}
