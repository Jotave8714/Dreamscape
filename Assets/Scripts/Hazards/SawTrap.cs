using UnityEngine;

namespace Dreamscape.Hazards
{
    /// <summary>Serra que gira e vai e volta entre dois pontos locais. Trajeto fixo, sem sorteio entre tentativas.</summary>
    public sealed class SawTrap : Hazard
    {
        [SerializeField] Vector3 travel = new Vector3(4f, 0f, 0f);
        [SerializeField, Min(0.1f)] float period = 3f;
        [SerializeField] float spinSpeed = 540f;
        [Tooltip("Parte que gira. Vazio = o próprio objeto.")]
        [SerializeField] Transform blade;

        Vector3 origin;

        void Start()
        {
            origin = transform.localPosition;
            if (blade == null)
                blade = transform;
        }

        void Update()
        {
            float t = Mathf.PingPong(Time.timeSinceLevelLoad / period * 2f, 1f);
            transform.localPosition = origin + travel * Mathf.SmoothStep(0f, 1f, t);
            blade.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self);
        }
    }
}
