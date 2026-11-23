using UnityEngine;

namespace RiggedGrandPrix.Hazards
{
    /// <summary>Nueva York: aparecen obras que cortan la mitad de la carretera.</summary>
    public class RoadworksHazard : CityHazard
    {
        private GameObject works;
        public override string DisplayName => "Obras";

        protected override void Build()
        {
            works = new GameObject("Obras");
            works.transform.SetParent(transform, false);
            var fence = MakeBox("Valla obras", new Vector3(0f, 0.6f, 0f), new Vector3(roadWidth * 0.5f, 1.2f, 0.5f), new Color(1f, 0.45f, 0f), true);
            fence.transform.SetParent(works.transform, false);
            AddHit(fence, 0.4f, 1f, trigger: false);
            for (int i = 0; i < 4; i++)
            {
                var cone = MakeBox($"Cono {i}", new Vector3(-roadWidth * 0.2f + i * roadWidth * 0.12f, 0.4f, -3f),
                                   new Vector3(0.5f, 0.4f, 0.5f), new Color(1f, 0.4f, 0f), true, PrimitiveType.Cylinder);
                cone.transform.SetParent(works.transform, false);
            }
        }

        protected override void OnActivate()
        {
            float side = Random.value < 0.5f ? -1f : 1f;
            works.transform.localPosition = new Vector3(side * roadWidth * 0.25f, 0f, 0f);
            works.SetActive(true);
        }

        protected override void OnRestore() => works.SetActive(false);
    }
}
