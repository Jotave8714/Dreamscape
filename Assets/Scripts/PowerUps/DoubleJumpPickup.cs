using Dreamscape.Characters;
using UnityEngine;

namespace Dreamscape.PowerUps
{
    /// <summary>Libera um pulo extra no ar enquanto o efeito durar.</summary>
    public sealed class DoubleJumpPickup : PowerUpPickup
    {
        public override string DisplayName => "PULO DUPLO";
        public override Color DisplayColor => new Color(0.45f, 0.85f, 1f);

        public override void Apply(CharacterMotor motor) => motor.SetMaxAirJumps(1);

        public override void Remove(CharacterMotor motor) => motor.SetMaxAirJumps(0);
    }
}
