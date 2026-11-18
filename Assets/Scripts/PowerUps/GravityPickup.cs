using Dreamscape.Characters;
using UnityEngine;

namespace Dreamscape.PowerUps
{
    /// <summary>Pluma: reduz a gravidade da queda, deixando os saltos mais longos, enquanto o efeito durar.</summary>
    public sealed class GravityPickup : PowerUpPickup
    {
        [Tooltip("Fração da gravidade normal. 0,5 = cai na metade da velocidade.")]
        [SerializeField, Range(0.1f, 1f)] float gravityScale = 0.5f;

        public override string DisplayName => "PLUMA";
        public override Color DisplayColor => new Color(0.78f, 0.62f, 1f);

        public override void Apply(CharacterMotor motor) => motor.SetGravityScale(gravityScale);

        public override void Remove(CharacterMotor motor) => motor.SetGravityScale(1f);
    }
}
