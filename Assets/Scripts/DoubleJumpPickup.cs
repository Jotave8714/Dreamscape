using Dreamscape.Characters;
using UnityEngine;

namespace Dreamscape
{
    public sealed class DoubleJumpPickup : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerInputController>();
            if (!player || !player.TryGetComponent(out CharacterMotor motor))
                return;

            motor.HasDoubleJump = true;
            gameObject.SetActive(false);
        }
    }
}
