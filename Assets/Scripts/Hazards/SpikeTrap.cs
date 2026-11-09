using UnityEngine;

namespace Dreamscape.Hazards
{
    /// <summary>
    /// Espinhos fixos ou em ciclo. No ciclo ficam expostos por <c>timeOut</c> s e recolhidos por <c>timeIn</c> s;
    /// começam a subir 0,3 s antes de ferir, como aviso.
    /// </summary>
    public sealed class SpikeTrap : Hazard
    {
        [Tooltip("Visual dos espinhos que sobe e desce. Vazio = só a regra de dano.")]
        [SerializeField] Transform spikes;
        [SerializeField] bool cycle;
        [SerializeField, Min(0.4f)] float timeOut = 1.2f;
        [SerializeField, Min(0.4f)] float timeIn = 1.5f;
        [SerializeField, Min(0f)] float offset;
        [SerializeField, Min(0.01f)] float travel = 0.5f;

        const float Warning = 0.3f;
        Vector3 outPosition;

        float Period => timeOut + timeIn;
        float T => (Time.timeSinceLevelLoad + offset) % Period;

        protected override bool IsDangerous => !cycle || T < timeOut;

        void Start()
        {
            if (spikes != null)
                outPosition = spikes.localPosition;
        }

        void Update()
        {
            if (!cycle || spikes == null)
                return;
            float t = T, up;
            if (t < timeOut) up = 1f;
            else if (t >= Period - Warning) up = Mathf.InverseLerp(Period - Warning, Period, t) * 0.4f;
            else up = 0f;
            spikes.localPosition = outPosition + Vector3.down * travel * (1f - up);
        }
    }
}
