using Dreamscape.Characters;
using Dreamscape.Core;
using UnityEngine;

namespace Dreamscape
{
    [RequireComponent(typeof(BoxCollider))]
    [DisallowMultipleComponent]
    public sealed class PhaseExit : MonoBehaviour
    {
        bool transitioning;

        void Reset() => GetComponent<BoxCollider>().isTrigger = true;

        void Awake() => GetComponent<BoxCollider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            if (transitioning || !other.GetComponentInParent<PlayerInputController>())
                return;
            transitioning = true;
            if (!GameFlow.CompletePhase(gameObject.scene))
                transitioning = false;
        }
    }
}
