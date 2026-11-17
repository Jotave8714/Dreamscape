using Dreamscape.Characters;
using UnityEngine;

namespace Dreamscape.PowerUps
{
    /// <summary>Pluma: reduz a gravidade da queda, deixando os saltos mais longos, até o próximo reinício da fase.</summary>
    public sealed class GravityPickup : PowerUpPickup
    {
        [Tooltip("Fração da gravidade normal. 0,5 = cai na metade da velocidade.")]
        [SerializeField, Range(0.1f, 1f)] float gravityScale = 0.5f;

        protected override void Apply(CharacterMotor motor) => motor.SetGravityScale(gravityScale);
    }
}
