using UnityEngine;

namespace RiggedGrandPrix.Hazards
{
    /// <summary>Tokio: un tren cruza el paso a nivel de lado a lado.</summary>
    public class TrainCrossingHazard : CityHazard
    {
        private GameObject train;
        private Rigidbody rb;
        private float travel, elapsed;
        private bool moving;
        public override string DisplayName => "Tren";

        protected override void Build()
        {
            train = MakeBox("Tren", Vector3.zero, new Vector3(16f, 2.6f, 3.2f), new Color(0.85f, 0.85f, 0.9f), true);
            rb = MakeKinematic(train);
            AddHit(train, 0.2f, 1.5f, trigger: false);
            travel = roadWidth + 20f;
        }

        protected override void OnActivate()
        {
            elapsed = 0f;
            moving = true;
            train.transform.localPosition = new Vector3(-travel, 1.3f, 0f);
            train.SetActive(true);
        }

        private void FixedUpdate()
        {
            if (!moving) return;
            elapsed += Time.fixedDeltaTime;
            float x = Mathf.Lerp(-travel, travel, elapsed / activeTime);
            rb.MovePosition(transform.TransformPoint(new Vector3(x, 1.3f, 0f)));
        }

        protected override void OnRestore() { moving = false; train.SetActive(false); }
    }
}
