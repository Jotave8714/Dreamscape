using Dreamscape.Characters;
using Dreamscape.Core;
using UnityEngine;

namespace Dreamscape
{
    /// <summary>
    /// Porta de conclusão: ao ser tocada pelo jogador conclui a fase (<see cref="GameFlow.CompletePhase"/>), carregando
    /// a próxima fase do build ou, depois da última, o encerramento. A Moema não aciona a porta e a pausa a bloqueia.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class PhaseGoal : MonoBehaviour
    {
        [Tooltip("Nome da cena seguinte. Vazio = a próxima cena do build; sem próxima, o encerramento.")]
        [SerializeField] string nextScene;

        bool used;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void Awake() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            if (used || other.GetComponentInParent<PlayerInputController>() == null)
                return;
            used = GameFlow.CompletePhase(gameObject.scene, nextScene);
        }
    }
}
