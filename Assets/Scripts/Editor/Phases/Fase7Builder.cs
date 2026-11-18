using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 7 — "Despertar": a última fase, a mais colorida, reúne tudo o que o jogador aprendeu.
    /// Seis atos em linha reta (+Z), cada um cobrando uma mecânica; o poder de cada ato é coletado antes de ser exigido.
    /// Medidas e cálculos em docs/Fase7.md.
    /// </summary>
    public sealed class Fase7Builder : IPhaseBuilder
    {
        public int Order => 7;
        public string SceneName => "Fase7_Despertar";
        public PhaseTheme Theme => PhaseTheme.Fase7;

        public void Build(PhaseKit kit)
        {
            // ---------------------------------------------------------- entrada (N0)
            kit.Platform("N0 Entrada", new Vector3(0f, 0f, 0f), new Vector2(6f, 6f));
            kit.Spawn(new Vector3(0f, 0f, 0f), 0f);
            kit.Sign("DESPERTAR", new Vector3(0f, 3.2f, 3.5f), 0f, 0.18f);
            kit.Wall("Parede entrada E", new Vector3(-3.4f, 0f, -3f), new Vector3(-3.4f, 0f, 3f));
            kit.Wall("Parede entrada D", new Vector3(3.4f, 0f, -3f), new Vector3(3.4f, 0f, 3f));
            kit.Pole(new Vector3(-2.4f, 0f, 2.2f));

            // ---------------------------------------------------------- ato 1: espinhos em ciclo (N1..N2)
            // corredor de 2 m, inteiro coberto pelos espinhos: só passa na janela de 2 s com eles recolhidos
            kit.Platform("N1 Corredor dos espinhos", new Vector3(0f, 0f, 8f), new Vector2(2f, 8f));
            kit.Spikes("Espinhos do ato 1", new Vector3(0f, 0f, 7f), true, 1.2f, 2.0f, 0f);
            kit.Pole(new Vector3(-1.6f, 0f, 5f), new Color(0.6f, 1f, 0.9f));

            kit.Platform("N2 Praca do desvio", new Vector3(0f, 0f, 15f), new Vector2(6f, 6f));
            kit.Pillar("Pilar praca E", new Vector3(-2.6f, 0f, 17.5f));
            kit.Pillar("Pilar praca D", new Vector3(2.6f, 0f, 17.5f));

            // ---------------------------------------------------------- ato 2: vão de 4 m, exige Velocidade (N3..N4)
            kit.Platform("N3 Plataforma do raio", new Vector3(5.5f, 0f, 15f), new Vector2(3f, 3f));
            kit.Pickup(PickupKind.Speed, new Vector3(5.5f, 0f, 15f));
            kit.Platform("N4 Aterrissagem do vao", new Vector3(0f, 0f, 24f), new Vector2(4f, 4f));
            kit.Sign("CORRA!", new Vector3(0f, 2.4f, 17.6f), 0f, 0.14f);

            // ---------------------------------------------------------- ato 3: pisos que cedem (N5..N6)
            for (int i = 0; i < 4; i++)
                kit.CrumblingPlatform($"Piso que cede {i + 1}", new Vector3(0f, 0f, 28f + 3f * i), new Vector2(2f, 2f), 0.8f);
            kit.Platform("N5 Patamar do escudo", new Vector3(0f, 0f, 41f), new Vector2(6f, 6f));
            kit.Pickup(PickupKind.Shield, new Vector3(0f, 0f, 40.5f));
            kit.CeilingLight(new Vector3(0f, 3.2f, 41f));

            // ---------------------------------------------------------- ato 4: dois lasers alternados (N6)
            // 1,5 s ligado e 1,8 s apagado; o segundo começa 1,65 s depois. Com o escudo dá para atravessar um sem esperar
            kit.Platform("N6 Corredor dos lasers", new Vector3(0f, 0f, 50f), new Vector2(4f, 10f));
            kit.Laser("Laser A", new Vector3(0f, 0f, 48f), 0f, 1.5f, 1.8f, 0f);
            kit.Laser("Laser B", new Vector3(0f, 0f, 52f), 0f, 1.5f, 1.8f, 1.65f);
            kit.Wall("Parede lasers E", new Vector3(-2.2f, 0f, 45f), new Vector3(-2.2f, 0f, 55f));
            kit.Wall("Parede lasers D", new Vector3(2.2f, 0f, 45f), new Vector3(2.2f, 0f, 55f));

            // ---------------------------------------------------------- ato 5: pêndulo e prensa (N7..N8)
            kit.Platform("N7 Sala do pendulo", new Vector3(0f, 0f, 59.5f), new Vector2(4f, 5f));
            kit.Pendulum("Pendulo", new Vector3(0f, 0f, 59.5f), 55f, 2.4f, 0f, 0f);
            kit.Platform("N8 Corredor da prensa", new Vector3(0f, 0f, 65f), new Vector2(1.8f, 6f));
            kit.Press("Prensa", new Vector3(0f, 0f, 65f), 2f, 0.8f, 0f, 0f);
            kit.Wall("Parede prensa E", new Vector3(-1.1f, 0f, 62f), new Vector3(-1.1f, 0f, 68f));
            kit.Wall("Parede prensa D", new Vector3(1.1f, 0f, 62f), new Vector3(1.1f, 0f, 68f));

            // ---------------------------------------------------------- ato 6: subida e salto final (N9..N12)
            kit.Platform("N9 Patamar dos poderes", new Vector3(0f, 0f, 72f), new Vector2(6f, 6f));
            kit.Pickup(PickupKind.DoubleJump, new Vector3(-1.5f, 0f, 72f));
            kit.Pickup(PickupKind.Gravity, new Vector3(1.5f, 0f, 72f));
            kit.Platform("N10 Degrau 1", new Vector3(0f, 1.7f, 77.4f), new Vector2(2.4f, 2.4f));
            kit.Platform("N11 Degrau 2", new Vector3(0f, 3.4f, 81.6f), new Vector2(2.4f, 2.4f));
            kit.Platform("N12 Patamar final", new Vector3(0f, 3.4f, 91.6f), new Vector2(6f, 6f));
            kit.Sign("SALTE ALTO", new Vector3(0f, 6f, 84.5f), 0f, 0.14f);
            kit.Goal(new Vector3(0f, 3.4f, 93.5f), 180f);
            kit.Pole(new Vector3(-2.5f, 3.4f, 90f), new Color(1f, 0.95f, 0.6f));
            kit.Pole(new Vector3(2.5f, 3.4f, 90f), new Color(1f, 0.95f, 0.6f));

            kit.FallZone(-10f);
        }
    }
}
