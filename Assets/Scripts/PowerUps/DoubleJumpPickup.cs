using Dreamscape.Characters;

namespace Dreamscape.PowerUps
{
    /// <summary>Libera um pulo extra no ar até o próximo reinício da fase.</summary>
    public sealed class DoubleJumpPickup : PowerUpPickup
    {
        protected override void Apply(CharacterMotor motor) => motor.SetMaxAirJumps(1);
    }
}
