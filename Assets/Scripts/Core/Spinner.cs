using UnityEngine;

namespace Dreamscape
{
    /// <summary>Gira e flutua suavemente: dá vida aos coletáveis sem animação própria.</summary>
    public sealed class Spinner : MonoBehaviour
    {
        [SerializeField] float degreesPerSecond = 70f;
        [SerializeField] float bobHeight = 0.12f;
        [SerializeField] float bobSpeed = 1.6f;

        Vector3 origin;
        float phase;

        void Start()
        {
            origin = transform.localPosition;
            phase = Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.World);
            transform.localPosition = origin + Vector3.up * (Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight);
        }
    }
}
