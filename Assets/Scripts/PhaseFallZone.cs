using Dreamscape.Characters;
using Dreamscape.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamscape
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class PhaseFallZone : MonoBehaviour
    {
        private bool restarting;

        private void Reset()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void Awake() => GetComponent<BoxCollider>().isTrigger = true;

        private void OnTriggerEnter(Collider other) => TryRestart(other);

        private void OnTriggerStay(Collider other) => TryRestart(other);

        private void TryRestart(Collider other)
        {
            if (restarting || GamePause.IsPaused || !other.GetComponentInParent<PlayerInputController>())
                return;

            restarting = true;
            var scene = gameObject.scene;
            if (scene.buildIndex < 0)
            {
                Debug.LogError("Adicione a cena ao perfil de build para reiniciar a fase.", this);
                return;
            }

            SceneManager.LoadScene(scene.buildIndex, LoadSceneMode.Single);
        }
    }
}
