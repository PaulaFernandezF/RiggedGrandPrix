using UnityEngine;
using UnityEngine.InputSystem;

namespace RiggedGrandPrix.Karts
{
    /// <summary>
    /// Estrategia concreta: el jugador conduce con teclado (WASD/flechas) o mando.
    /// </summary>
    [AddComponentMenu("Rigged Grand Prix/Player Input (Kart)")]
    public class PlayerInput : MonoBehaviour, IKartInput
    {
        public float Throttle { get; private set; }
        public float Steer { get; private set; }
        public bool Drift { get; private set; }

        public bool WantsUsePowerUp()
        {
            var kb = Keyboard.current;
            var pad = Gamepad.current;
            return (kb != null && kb.spaceKey.wasPressedThisFrame) ||
                   (pad != null && pad.buttonSouth.wasPressedThisFrame);
        }

        private void Update()
        {
            float throttle = 0f, steer = 0f;
            bool drift = false;

            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) throttle += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) throttle -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) steer += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) steer -= 1f;
                drift = kb.shiftKey.isPressed || kb.leftCtrlKey.isPressed;
            }

            var pad = Gamepad.current;
            if (pad != null)
            {
                throttle += pad.rightTrigger.ReadValue() - pad.leftTrigger.ReadValue();
                steer += pad.leftStick.x.ReadValue();
                drift |= pad.rightShoulder.isPressed;
            }

            Throttle = Mathf.Clamp(throttle, -1f, 1f);
            Steer = Mathf.Clamp(steer, -1f, 1f);
            Drift = drift;
        }
    }
}