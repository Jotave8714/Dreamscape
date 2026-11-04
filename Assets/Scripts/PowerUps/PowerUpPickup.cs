using System;
using Dreamscape.Characters;
using UnityEngine;

namespace Dreamscape.PowerUps
{
    /// <summary>
    /// Coletável de power-up: ao ser tocado pelo jogador, aplica o efeito no motor e some.
    /// O efeito fica no Player e o coletável na cena, então recarregar a cena (morte/reinício)
    /// remove o poder e restaura o coletável sem código extra.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class PowerUpPickup : MonoBehaviour
    {
        /// <summary>Disparado na coleta (gancho para áudio e efeitos visuais).</summary>
        public event Action Collected;

        protected abstract void Apply(CharacterMotor motor);

        void Reset() => GetComponent<Collider>().isTrigger = true;

        // os coletáveis desenhados nas cenas têm colisor sólido; sem trigger o jogador esbarraria neles
        void Awake() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            PlayerInputController player = other.GetComponentInParent<PlayerInputController>();
            if (player == null) // a Moema usa o mesmo motor, mas não coleta
                return;

            Apply(player.GetComponent<CharacterMotor>());
            Collected?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
