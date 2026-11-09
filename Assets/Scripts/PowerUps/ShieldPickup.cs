using Dreamscape.Characters;
using UnityEngine;

namespace Dreamscape.PowerUps
{
    /// <summary>Escudo: absorve o próximo contato com uma armadilha (<see cref="Dreamscape.Hazards.Hazard"/>).</summary>
    public sealed class ShieldPickup : PowerUpPickup
    {
        protected override void Apply(CharacterMotor motor) => PlayerShield.Grant(motor.gameObject);
    }

    /// <summary>Estado do escudo no jogador. Some ao absorver um golpe ou ao recarregar a cena.</summary>
    public sealed class PlayerShield : MonoBehaviour
    {
        public static void Grant(GameObject player)
        {
            if (!player.TryGetComponent(out PlayerShield _))
                player.AddComponent<PlayerShield>();
        }

        const float GraceSeconds = 1f;
        static float protectedUntil;

        /// <summary>Verdadeiro logo depois de um golpe absorvido: dá tempo de sair da zona de dano da armadilha.</summary>
        public static bool IsProtected => Time.time < protectedUntil;

        /// <summary>Consome o escudo se existir. Retorna verdadeiro quando o golpe foi absorvido.</summary>
        public static bool TryAbsorb(GameObject player)
        {
            if (!player.TryGetComponent(out PlayerShield shield))
                return false;
            Destroy(shield);
            protectedUntil = Time.time + GraceSeconds;
            return true;
        }
    }
}
