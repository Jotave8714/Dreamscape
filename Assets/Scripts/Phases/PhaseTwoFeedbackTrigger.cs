using Dreamscape.Characters;
using Dreamscape.Core;
using UnityEngine;

namespace Dreamscape.Phases
{
    [RequireComponent(typeof(BoxCollider))]
    [DisallowMultipleComponent]
    public sealed class PhaseTwoFeedbackTrigger : MonoBehaviour
    {
        [SerializeField] PhaseTwoFeedback feedback;
        [SerializeField] bool success;
        bool handled;

        void Awake() => GetComponent<BoxCollider>().isTrigger = true;
        void OnTriggerEnter(Collider other) => Report(other);
        void OnTriggerStay(Collider other) => Report(other);

        void Report(Collider other)
        {
            if (!enabled || handled || GamePause.IsPaused || feedback == null)
                return;
            var player = other.GetComponentInParent<PlayerInputController>();
            if (player == null)
                return;
            if (success)
            {
                if (player.Motor.IsGrounded)
                    handled = feedback.ReportSuccess();
            }
            else
            {
                feedback.ReportFailure();
                handled = true;
            }
        }
    }
}
