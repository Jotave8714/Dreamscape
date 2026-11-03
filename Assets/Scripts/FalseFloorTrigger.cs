using System.Collections;
using Dreamscape.Characters;
using UnityEngine;

namespace Dreamscape
{
    public sealed class FalseFloorTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject floor;
        [SerializeField, Min(0f)] private float delay = 0.25f;
        private bool activated;

        private void OnTriggerEnter(Collider other)
        {
            if (activated || !floor || !other.GetComponentInParent<PlayerInputController>())
                return;

            activated = true;
            StartCoroutine(Collapse());
        }

        private IEnumerator Collapse()
        {
            yield return new WaitForSeconds(delay);
            floor.SetActive(false);
        }
    }
}
