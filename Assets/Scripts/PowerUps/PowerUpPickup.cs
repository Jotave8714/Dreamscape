using System;
using Dreamscape.Characters;
using UnityEngine;

namespace Dreamscape.PowerUps
{
    /// <summary>
    /// Coletável de power-up: ao ser tocado pelo jogador, ativa o efeito por um tempo e some. Quando o efeito acaba,
    /// o coletável volta, senão o jogador ficaria sem como atravessar um trecho que exige o poder.
    /// O efeito fica no Player e o coletável na cena, então recarregar a cena (morte/reinício) restaura tudo.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class PowerUpPickup : MonoBehaviour
    {
        [Tooltip("Duração do efeito (s).")]
        [SerializeField, Min(1f)] float duration = 30f;

        /// <summary>Disparado na coleta (gancho para áudio e efeitos visuais).</summary>
        public event Action Collected;

        public float Duration => duration;
        public abstract string DisplayName { get; }
        public abstract Color DisplayColor { get; }

        public abstract void Apply(CharacterMotor motor);
        public abstract void Remove(CharacterMotor motor);

        void Reset() => GetComponent<Collider>().isTrigger = true;

        // os coletáveis desenhados nas cenas têm colisor sólido; sem trigger o jogador esbarraria neles
        void Awake() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            PlayerInputController player = other.GetComponentInParent<PlayerInputController>();
            if (player == null) // a Moema usa o mesmo motor, mas não coleta
                return;

            if (!player.TryGetComponent(out ActivePowerUps active))
                active = player.gameObject.AddComponent<ActivePowerUps>();
            active.Activate(this);
            Collected?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
