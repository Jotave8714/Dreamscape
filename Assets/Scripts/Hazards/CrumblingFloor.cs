using UnityEngine;

namespace Dreamscape.Hazards
{
    /// <summary>
    /// Piso que cede depois que o jogador pisa (como o chão falso do DS-08), com um tremor de aviso.
    /// Recarregar a cena restaura o piso, então a tentativa seguinte é idêntica.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class CrumblingFloor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] float delay = 0.8f;
        [SerializeField, Min(0.1f)] float fallSpeed = 12f;
        [SerializeField] float shake = 0.04f;

        float stepTime = -1f;
        Vector3 rest;
        Collider solid;

        void Awake()
        {
            solid = GetComponent<Collider>();
            rest = transform.localPosition;
        }

        void Step(Component other)
        {
            if (stepTime < 0f && other.GetComponentInParent<Dreamscape.Characters.PlayerInputController>() != null)
                stepTime = Time.time;
        }

        // o CharacterController não gera OnCollisionEnter; um raio curto acima do piso detecta quem está em cima
        void FixedUpdate()
        {
            if (stepTime >= 0f)
                return;
            Bounds b = solid.bounds;
            Vector3 center = new Vector3(b.center.x, b.max.y + 0.1f, b.center.z);
            Vector3 half = new Vector3(b.extents.x * 0.9f, 0.15f, b.extents.z * 0.9f);
            foreach (Collider c in Physics.OverlapBox(center, half, transform.rotation, ~0, QueryTriggerInteraction.Ignore))
                Step(c);
        }

        void Update()
        {
            if (stepTime < 0f)
                return;
            float elapsed = Time.time - stepTime;
            if (elapsed < delay)
            {
                transform.localPosition = rest + Random.insideUnitSphere * shake;
                return;
            }
            solid.enabled = false;
            transform.localPosition += Vector3.down * fallSpeed * Time.deltaTime;
            if (elapsed > delay + 3f)
                gameObject.SetActive(false);
        }
    }
}
