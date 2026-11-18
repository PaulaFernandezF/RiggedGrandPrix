using UnityEngine;

namespace RiggedGrandPrix.Utils
{
    /// <summary>Cámara en tercera persona que sigue al kart con suavizado.</summary>
    public class KartCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 3f, -7f);
        [SerializeField] private float positionSmooth = 8f;
        [SerializeField] private float rotationSmooth = 6f;
        [SerializeField] private float lookHeight = 1f;

        private void LateUpdate()
        {
            if (target == null) return;
            float dt = Time.deltaTime;

            // Solo seguimos el giro horizontal del kart
            Quaternion yaw = Quaternion.Euler(0f, target.eulerAngles.y, 0f);
            Vector3 desired = target.position + yaw * offset;
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-positionSmooth * dt));

            Vector3 lookAt = target.position + Vector3.up * lookHeight;
            Quaternion look = Quaternion.LookRotation(lookAt - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 1f - Mathf.Exp(-rotationSmooth * dt));
        }
    }
}
