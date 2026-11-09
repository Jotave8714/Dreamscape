using Dreamscape.Characters;
using Dreamscape.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamscape
{
    /// <summary>
    /// Porta de conclusão: ao ser tocada pelo jogador carrega a próxima fase do build ou, se não houver, volta ao menu.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class PhaseGoal : MonoBehaviour
    {
        [Tooltip("Nome da cena seguinte. Vazio = a próxima cena do build; sem próxima, volta ao menu.")]
        [SerializeField] string nextScene;

        bool used;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void Awake() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            if (used || other.GetComponentInParent<PlayerInputController>() == null)
                return;
            used = true;

            if (!string.IsNullOrEmpty(nextScene) && Application.CanStreamedLevelBeLoaded(nextScene))
            {
                SceneManager.LoadScene(nextScene);
                return;
            }

            int next = gameObject.scene.buildIndex + 1;
            if (gameObject.scene.buildIndex >= 0 && next < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(next);
            else if (GameFlow.MenuAvailable)
                GameFlow.ReturnToMenu();
        }
    }
}
