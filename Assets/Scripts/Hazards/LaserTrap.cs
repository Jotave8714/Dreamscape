using UnityEngine;

namespace Dreamscape.Hazards
{
    /// <summary>Feixe que liga e desliga em ciclo fixo; pisca antes de ligar para o jogador aprender o ritmo.</summary>
    public sealed class LaserTrap : Hazard
    {
        [Tooltip("Visual do feixe. Vazio = sem visual.")]
        [SerializeField] Renderer beam;
        [SerializeField, Min(0.2f)] float timeOn = 1.5f;
        [SerializeField, Min(0.2f)] float timeOff = 1.8f;
        [SerializeField, Min(0f)] float offset;
        [SerializeField, Min(0.05f)] float warning = 0.4f;

        float Cycle => timeOn + timeOff;
        float T => (Time.timeSinceLevelLoad + offset) % Cycle;

        protected override bool IsDangerous => T < timeOn;

        void Update()
        {
            if (beam == null)
                return;
            float t = T;
            bool warn = t >= Cycle - warning;
            beam.enabled = t < timeOn || (warn && Mathf.FloorToInt(t * 12f) % 2 == 0);
        }
    }
}
