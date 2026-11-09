using Dreamscape.Characters;
using Dreamscape.PowerUps;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamscape.Hazards
{
    /// <summary>
    /// Base das armadilhas: tocar o colisor (trigger) reinicia a fase, como a <see cref="PhaseFallZone"/>,
    /// a menos que o jogador tenha o escudo, que é gasto no lugar.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class Hazard : MonoBehaviour
    {
        bool restarting;

        /// <summary>Armadilhas desligadas (laser apagado, prensa levantada) não ferem.</summary>
        protected virtual bool IsDangerous => true;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void Awake() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerStay(Collider other) => Touch(other);

        void OnTriggerEnter(Collider other) => Touch(other);

        void Touch(Collider other)
        {
            if (restarting || !IsDangerous)
                return;
            PlayerInputController player = other.GetComponentInParent<PlayerInputController>();
            if (player == null)
                return;
            if (PlayerShield.IsProtected)
                return;
            if (PlayerShield.TryAbsorb(player.gameObject))
            {
                OnShieldAbsorbed();
                return;
            }

            Scene scene = gameObject.scene;
            if (scene.buildIndex < 0)
            {
                Debug.LogError("Adicione a cena ao perfil de build para reiniciar a fase.", this);
                return;
            }
            restarting = true;
            SceneManager.LoadScene(scene.buildIndex, LoadSceneMode.Single);
        }

        /// <summary>Gancho para áudio/efeito quando o escudo absorve o golpe.</summary>
        protected virtual void OnShieldAbsorbed() { }
    }
}
