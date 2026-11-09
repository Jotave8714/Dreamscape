using UnityEngine;

namespace Dreamscape.Hazards
{
    /// <summary>Pêndulo que balança em torno do eixo local Z, a partir do pivô (topo do modelo).</summary>
    public sealed class PendulumTrap : Hazard
    {
        [SerializeField, Range(10f, 80f)] float angle = 55f;
        [SerializeField, Min(0.3f)] float period = 2.4f;
        [SerializeField, Min(0f)] float offset;
        [SerializeField] Vector3 axis = Vector3.forward;

        Quaternion rest;

        void Start() => rest = transform.localRotation;

        void Update()
        {
            float swing = Mathf.Sin((Time.timeSinceLevelLoad + offset) / period * Mathf.PI * 2f) * angle;
            transform.localRotation = rest * Quaternion.AngleAxis(swing, axis);
        }
    }
}
