using Dreamscape.Characters;
using UnityEngine;

namespace Dreamscape.PowerUps
{
    /// <summary>Aumenta a velocidade de andar e correr enquanto o efeito durar.</summary>
    public sealed class SpeedPickup : PowerUpPickup
    {
        [Tooltip("Multiplicador de andar e correr. 1,5 = corrida de 5 para 7,5 m/s.")]
        [SerializeField, Min(1f)] float multiplier = 1.5f;

        public override string DisplayName => "SUPER VELOCIDADE";
        public override Color DisplayColor => new Color(1f, 0.84f, 0.29f);

        public override void Apply(CharacterMotor motor) => motor.SetSpeedMultiplier(multiplier);

        public override void Remove(CharacterMotor motor) => motor.SetSpeedMultiplier(1f);
    }
}
