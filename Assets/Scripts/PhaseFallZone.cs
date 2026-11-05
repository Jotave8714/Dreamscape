using Dreamscape.Characters;
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

        private void OnTriggerEnter(Collider other)
        {
            if (restarting || !other.GetComponentInParent<PlayerInputController>())
                return;

            var scene = gameObject.scene;
            if (scene.buildIndex < 0)
            {
                Debug.LogError("Adicione a cena ao perfil de build para reiniciar a fase.", this);
                return;
            }

            restarting = true;
            SceneManager.LoadScene(scene.buildIndex, LoadSceneMode.Single);
        }
    }
}
