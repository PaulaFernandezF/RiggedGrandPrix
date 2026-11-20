using System.Collections.Generic;
using RiggedGrandPrix.Data;
using RiggedGrandPrix.Karts;
using RiggedGrandPrix.Utils;
using UnityEngine;

namespace RiggedGrandPrix.AI
{
    /// <summary>
    /// Crea los rivales clonando el kart del jugador: misma física, pero con
    /// RivalFSMInput en vez de PlayerInput, otro personaje y otro color.
    /// </summary>
    public class RivalSpawner : MonoBehaviour
    {
        [SerializeField] private KartController player;
        [SerializeField] private int rivalCount = 5;
        [Tooltip("Personajes posibles (no se repite el del jugador).")]
        [SerializeField] private List<CharacterData> characters = new List<CharacterData>();
        [SerializeField] private float rivalSpeedFactor = 0.95f;

        private static readonly Color[] Colors =
        {
            new Color(0.9f, 0.2f, 0.2f), new Color(1f, 0.55f, 0.1f), new Color(0.95f, 0.85f, 0.2f),
            new Color(0.25f, 0.75f, 0.3f), new Color(0.6f, 0.3f, 0.85f)
        };

        private readonly List<KartController> rivals = new List<KartController>();
        public IReadOnlyList<KartController> Rivals => rivals;

        private void Awake()
        {
            if (player == null)
            {
                var pi = FindAnyObjectByType<PlayerInput>();
                if (pi != null) player = pi.GetComponent<KartController>();
            }
            if (player == null) { Debug.LogError("[RivalSpawner] No encuentro el kart del jugador."); return; }

            var available = new List<CharacterData>();
            foreach (var c in characters)
                if (c != null && c != player.Character) available.Add(c);

            // Desactivamos la plantilla un momento: así los clones nacen "dormidos"
            // y les cambiamos el conductor antes de que se ejecute su Awake.
            var template = player.gameObject;
            bool wasActive = template.activeSelf;
            template.SetActive(false);

            for (int i = 0; i < rivalCount; i++)
            {
                Vector3 pos = template.transform.position + template.transform.right * 3f * (i + 1);
                var go = Instantiate(template, pos, template.transform.rotation);
                go.name = $"Rival {i + 1}";

                foreach (var p in go.GetComponents<PlayerInput>()) DestroyImmediate(p);
                foreach (var d in go.GetComponents<KartDebugKeys>()) DestroyImmediate(d);
                go.AddComponent<RivalFSMInput>().SetSpeedFactor(rivalSpeedFactor);

                var r = go.GetComponentInChildren<Renderer>();
                if (r != null) r.material.color = Colors[i % Colors.Length];

                go.SetActive(true);
                var kart = go.GetComponent<KartController>();
                if (available.Count > 0) kart.SetCharacter(available[i % available.Count]);
                rivals.Add(kart);
            }

            template.SetActive(wasActive);
        }
    }
}