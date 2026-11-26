using RiggedGrandPrix.Karts;
using RiggedGrandPrix.Karts.Stats;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RiggedGrandPrix.Utils
{
    /// <summary>
    /// SOLO PARA PROBAR la sección B (quitar antes de la entrega).
    /// 1 = turbo, 2 = ralentizar, 3 = dirección torpe, R = volver a la salida.
    /// Sirve para ver el Decorator funcionando antes de que existan los power-ups.
    /// </summary>
    [RequireComponent(typeof(KartController))]
    public class KartDebugKeys : MonoBehaviour
    {
        private KartController kart;

        private void Awake() => kart = GetComponent<KartController>();

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.digit1Key.wasPressedThisFrame) kart.AddModifier(new SpeedBoostModifier(1.5f, 2f));
            if (kb.digit2Key.wasPressedThisFrame) kart.AddModifier(new SlowModifier(0.5f, 3f));
            if (kb.digit3Key.wasPressedThisFrame) kart.AddModifier(new TurnPenaltyModifier(0.4f, 3f));
            if (kb.rKey.wasPressedThisFrame) kart.ResetToSpawn();
            if (kb.tKey.wasPressedThisFrame)   // T: dispara todas las trampas (para probar la sección F)
                foreach (var h in Hazards.CityHazard.All) h.Trigger(2f);
        }

        private void OnGUI()
        {
            if (kart.Stats == null) return;
            var s = kart.Stats;
            string mods = kart.Modifiers.Count == 0 ? "ninguno" : "";
            foreach (var m in kart.Modifiers) mods += m.GetType().Name + " ";
            GUI.Label(new Rect(10, 10, 900, 170),
                $"Piloto: {(kart.Character != null ? kart.Character.displayName : "-")}\n" +
                $"Velocidad: {kart.ForwardSpeed * 3.6f:F0} km/h  (máx {s.MaxSpeed * 3.6f:F0})\n" +
                $"Aceleración: {s.Acceleration:F1}   Giro: {s.TurnSpeed:F0} °/s   Peso: {s.Weight:F1}\n" +
                $"En suelo: {kart.IsGrounded}   Derrapando: {kart.IsDrifting}\n" +
                $"Objeto: {(kart.HeldPowerUp != null ? kart.HeldPowerUp.Type.ToString() : "ninguno")} [Espacio]   Escudo: {kart.ShieldActive}\n" +
                $"Modificadores: {mods}\n" +
                "Teclas: 1 turbo · 2 frenazo · 3 dirección torpe · R salida · T trampas");
        }
    }
}
