using UnityEngine;

namespace Dreamscape.Hazards
{
    /// <summary>Prensa que desce sobre o piso em ciclo fixo, com pausa no alto para o jogador passar.</summary>
    public sealed class PressTrap : Hazard
    {
        [SerializeField, Min(0.1f)] float drop = 2.2f;
        [SerializeField, Min(0.3f)] float timeUp = 2f;
        [SerializeField, Min(0.1f)] float timeDown = 0.8f;
        [SerializeField, Min(0.05f)] float fall = 0.2f;
        [SerializeField, Min(0.05f)] float rise = 0.8f;
        [SerializeField, Min(0f)] float offset;

        Vector3 top;
        float Cycle => timeUp + fall + timeDown + rise;
        float T => (Time.timeSinceLevelLoad + offset) % Cycle;

        // só fere enquanto desce e fica embaixo
        protected override bool IsDangerous => T >= timeUp && T < timeUp + fall + timeDown;

        void Start() => top = transform.localPosition;

        void Update()
        {
            float t = T, h;
            if (t < timeUp) h = 1f;
            else if (t < timeUp + fall) h = 1f - (t - timeUp) / fall;
            else if (t < timeUp + fall + timeDown) h = 0f;
            else h = (t - timeUp - fall - timeDown) / rise;
            transform.localPosition = top + Vector3.down * drop * (1f - h);
        }
    }
}
