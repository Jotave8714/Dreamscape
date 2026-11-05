using System.Collections.Generic;
using Dreamscape.Characters;
using UnityEngine;

namespace Dreamscape.PowerUps
{
    /// <summary>
    /// Efeitos ativos no jogador e o tempo restante de cada um. Adicionado na primeira coleta. Conta com Time.deltaTime,
    /// então o menu de pause congela os efeitos.
    /// </summary>
    [RequireComponent(typeof(CharacterMotor))]
    [DisallowMultipleComponent]
    public sealed class ActivePowerUps : MonoBehaviour
    {
        public sealed class Effect
        {
            public PowerUpPickup Pickup;
            public float Remaining;
            public float Progress => Remaining / Pickup.Duration;
        }

        readonly List<Effect> effects = new List<Effect>();
        CharacterMotor motor;

        public IReadOnlyList<Effect> Effects => effects;

        void Awake()
        {
            motor = GetComponent<CharacterMotor>();
            var hud = new GameObject("PowerUpHud").AddComponent<PowerUpHud>();
            hud.transform.SetParent(transform, false);
            hud.Bind(this, motor);
        }

        public void Activate(PowerUpPickup pickup)
        {
            pickup.Apply(motor);
            effects.Add(new Effect { Pickup = pickup, Remaining = pickup.Duration });
        }

        void Update()
        {
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                Effect effect = effects[i];
                effect.Remaining -= Time.deltaTime;
                if (effect.Remaining > 0f)
                    continue;
                effect.Pickup.Remove(motor);
                effect.Pickup.gameObject.SetActive(true);
                effects.RemoveAt(i);
            }
        }
    }
}
