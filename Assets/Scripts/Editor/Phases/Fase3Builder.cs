using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 3 — "Corredor das Luzes": a fase que apresenta as armadilhas.
    /// Salas de backrooms em sequência (+Z): espinhos fixos (desviar), espinhos em ciclo (esperar o ritmo),
    /// um vão grande que só a Pluma atravessa (atalho opcional, com desvio lateral sem poder),
    /// um laser perto do fim e um truque de cenário: o corredor escuro que leva direto à porta é piso falso.
    /// Regra visual da fase: onde há luminária no teto há chão firme. Pontos, grafo e cálculos em docs/Fase3.md.
    /// </summary>
    public sealed class Fase3Builder : IPhaseBuilder
    {
        public int Order => 3;
        public string SceneName => "Fase3_CorredorDasLuzes";
        public PhaseTheme Theme => PhaseTheme.Fase3;

        // luz fria dos postes, no tom ciano/verde-água do tema
        static readonly Color PoleCyan = new Color(0.55f, 1f, 0.92f);
        // luz da Pluma, um pouco mais azulada para destacar o poder
        static readonly Color FeatherBlue = new Color(0.55f, 0.85f, 1f);

        public void Build(PhaseKit kit)
        {
            // ---------------------------------------------------------- N0: entrada
            kit.Platform("N0 Entrada", new Vector3(0f, 0f, 0f), new Vector2(6f, 4f));
            kit.Spawn(new Vector3(0f, 0f, 0f), 0f);
            kit.Wall("Parede entrada fundo", new Vector3(-3f, 0f, -2f), new Vector3(3f, 0f, -2f));
            kit.Wall("Parede entrada E", new Vector3(-3f, 0f, -2f), new Vector3(-3f, 0f, 2f));
            kit.Wall("Parede entrada D", new Vector3(3f, 0f, -2f), new Vector3(3f, 0f, 2f));
            kit.CeilingLight(new Vector3(0f, 3f, 0f));
            kit.Pole(new Vector3(2.3f, 0f, -1.3f), PoleCyan);
            kit.Sign("FASE 3", new Vector3(0f, 3.3f, 3f), 0f, 0.09f);
            kit.Sign("CORREDOR DAS LUZES", new Vector3(0f, 2.7f, 3f), 0f, 0.06f);

            // ---------------------------------------------------------- N1: sala dos espinhos fixos
            // vão de 2 m (alcance correndo 3,32 m). Duas fileiras desencontradas: a passagem livre muda de lado
            kit.Platform("N1 Sala dos espinhos fixos", new Vector3(0f, 0f, 8.5f), new Vector2(6f, 9f));
            kit.Spikes("Fixo 1A", new Vector3(-2f, 0f, 7.5f));
            kit.Spikes("Fixo 1B", new Vector3(0f, 0f, 7.5f));   // fileira 1: passa pela direita (x = +2)
            kit.Spikes("Fixo 2A", new Vector3(0f, 0f, 11f));
            kit.Spikes("Fixo 2B", new Vector3(2f, 0f, 11f));    // fileira 2: passa pela esquerda (x = -2)
            kit.Wall("Parede sala 1 E", new Vector3(-3f, 0f, 4f), new Vector3(-3f, 0f, 13f));
            kit.Wall("Parede sala 1 D", new Vector3(3f, 0f, 4f), new Vector3(3f, 0f, 13f));
            kit.Wall("Parede sala 1 frente E", new Vector3(-3f, 0f, 13f), new Vector3(-1f, 0f, 13f));
            kit.Wall("Parede sala 1 frente D", new Vector3(1f, 0f, 13f), new Vector3(3f, 0f, 13f));
            kit.CeilingLight(new Vector3(0f, 3f, 5.5f));
            kit.CeilingLight(new Vector3(0f, 3f, 10f));

            // ---------------------------------------------------------- N2..N3: corredor do ritmo (espinhos em ciclo)
            // 1,7 m livres entre as paredes: os espinhos (1,8 m) cobrem a largura toda, só passa no ritmo.
            // Ciclo de 2,7 s (1,2 s expostos, 1,5 s recolhidos). O par B está meio ciclo atrasado em relação ao A:
            // atravessa-se A, espera-se no refúgio (N3) e atravessa-se B.
            kit.Platform("N2-N3 Corredor do ritmo", new Vector3(0f, 0f, 18.5f), new Vector2(2f, 11f));
            kit.Spikes("Ritmo A1", new Vector3(0f, 0f, 15f), true, 1.2f, 1.5f, 0f);
            kit.Spikes("Ritmo A2", new Vector3(0f, 0f, 17f), true, 1.2f, 1.5f, 0f);
            kit.Spikes("Ritmo B1", new Vector3(0f, 0f, 21f), true, 1.2f, 1.5f, 1.35f);
            kit.Spikes("Ritmo B2", new Vector3(0f, 0f, 23f), true, 1.2f, 1.5f, 1.35f);
            kit.Wall("Parede corredor ritmo E", new Vector3(-1f, 0f, 13f), new Vector3(-1f, 0f, 24f));
            kit.Wall("Parede corredor ritmo D", new Vector3(1f, 0f, 13f), new Vector3(1f, 0f, 24f));
            kit.CeilingLight(new Vector3(0f, 3f, 15.5f));
            kit.CeilingLight(new Vector3(0f, 3f, 19f));
            kit.CeilingLight(new Vector3(0f, 3f, 22.5f));
            kit.Sign("OBSERVE O RITMO", new Vector3(0f, 2.6f, 12.8f), 0f, 0.05f);

            // ---------------------------------------------------------- N4: patamar da escolha e N5: pedestal da Pluma
            kit.Platform("N4 Patamar da escolha", new Vector3(0f, 0f, 28f), new Vector2(8f, 8f));
            kit.Platform("N5 Pedestal da Pluma", new Vector3(-3f, 0.5f, 27f), new Vector2(2f, 2f));
            kit.Pickup(PickupKind.Gravity, new Vector3(-3f, 0.5f, 27f));
            kit.Wall("Parede patamar fundo E", new Vector3(-4f, 0f, 24f), new Vector3(-1f, 0f, 24f));
            kit.Wall("Parede patamar fundo D", new Vector3(1f, 0f, 24f), new Vector3(4f, 0f, 24f));
            kit.Wall("Parede patamar E", new Vector3(-4f, 0f, 24f), new Vector3(-4f, 0f, 32f));
            kit.Pillar("Pilar patamar E", new Vector3(-3.5f, 0f, 31.5f));
            kit.Pillar("Pilar patamar D", new Vector3(3.5f, 0f, 31.5f));
            kit.CeilingLight(new Vector3(0f, 3f, 29f));
            kit.CeilingLight(new Vector3(-3f, 3.5f, 26.5f), 0f, FeatherBlue);
            kit.Pole(new Vector3(3.3f, 0f, 24.7f), PoleCyan);
            // à frente (z 32 -> 37,5) fica o vão de 5,5 m: impossível sem poder, folgado com a Pluma

            // ---------------------------------------------------------- N6..N7: desvio lateral sem poder
            kit.Platform("N6 Galeria lateral", new Vector3(7.5f, 0f, 29f), new Vector2(3f, 8f));
            kit.Wall("Parede galeria fundo", new Vector3(6f, 0f, 25f), new Vector3(9f, 0f, 25f));
            kit.Wall("Parede galeria D", new Vector3(9f, 0f, 25f), new Vector3(9f, 0f, 33f));
            kit.CeilingLight(new Vector3(7.5f, 3f, 27f));
            kit.CeilingLight(new Vector3(7.5f, 3f, 31f));

            kit.Platform("N7 Degrau lateral", new Vector3(8f, 0.5f, 37.5f), new Vector2(3f, 5f));
            kit.Wall("Parede degrau lateral D", new Vector3(9.5f, 0.5f, 35f), new Vector3(9.5f, 0.5f, 40f));
            kit.Wall("Parede degrau lateral frente", new Vector3(6.5f, 0.5f, 40f), new Vector3(9.5f, 0.5f, 40f));
            kit.CeilingLight(new Vector3(8f, 3.5f, 37.5f));

            // ---------------------------------------------------------- N8: salão (chegada dos dois caminhos)
            kit.Platform("N8 Salao", new Vector3(0f, 0f, 41.5f), new Vector2(8f, 8f));
            kit.Wall("Parede salao frente E", new Vector3(-4f, 0f, 45.5f), new Vector3(-1f, 0f, 45.5f));
            kit.Wall("Parede salao frente D", new Vector3(1f, 0f, 45.5f), new Vector3(4f, 0f, 45.5f));
            kit.Wall("Parede salao D", new Vector3(4f, 0f, 40f), new Vector3(4f, 0f, 45.5f));
            kit.Wall("Parede salao E", new Vector3(-4f, 0f, 37.5f), new Vector3(-4f, 0f, 40f));
            // luzes afastadas da boca do corredor escuro, para ele continuar sem luz
            kit.CeilingLight(new Vector3(-2f, 3f, 40f));
            kit.CeilingLight(new Vector3(2f, 3f, 40f));
            kit.Pole(new Vector3(-3.3f, 0f, 38.2f), PoleCyan);
            kit.Sign("SIGA AS LUZES", new Vector3(-2.5f, 1.9f, 45.3f), 0f, 0.05f);

            // ---------------------------------------------------------- T0: truque de cenário, o corredor escuro
            // Parece o caminho direto: a porta aparece no fim, em linha reta. Mas não há luminária, o piso é falso
            // (cede 0,8 s depois de pisado) e a porta está 2,5 m acima do piso do corredor (o pulo alcança 1,1 m;
            // com a Pluma, 2,2 m). Quem desce sente o tremor e ainda pode voltar ao salão (1 m acima) se for rápido.
            kit.CrumblingPlatform("T0 Corredor escuro", new Vector3(0f, -1f, 48.75f), new Vector2(2f, 6.5f), 0.8f);
            kit.Wall("Parede corredor escuro E", new Vector3(-1f, -1f, 45.5f), new Vector3(-1f, -1f, 52f), 4f);
            kit.Wall("Parede corredor escuro D", new Vector3(1f, -1f, 45.5f), new Vector3(1f, -1f, 52f), 4f);
            kit.Wall("Parede corredor escuro fim", new Vector3(-1f, -1f, 52f), new Vector3(1f, -1f, 52f), 2.5f);

            // ---------------------------------------------------------- N9..N10: corredor do laser
            // largura interna 2,7 m = largura do feixe: não há como contornar, só passar com o laser apagado
            kit.Platform("N9-N10 Corredor do laser", new Vector3(-7f, 0f, 45f), new Vector2(3f, 10f));
            kit.Laser("Laser do corredor", new Vector3(-7f, 0f, 47.5f), 0f, 1.5f, 1.8f, 0f);
            kit.Wall("Parede laser fundo", new Vector3(-8.5f, 0f, 40f), new Vector3(-5.5f, 0f, 40f));
            kit.Wall("Parede laser E", new Vector3(-8.5f, 0f, 40f), new Vector3(-8.5f, 0f, 50f));
            kit.Wall("Parede laser D", new Vector3(-5.5f, 0f, 45.5f), new Vector3(-5.5f, 0f, 50f));
            kit.CeilingLight(new Vector3(-7f, 3f, 42.5f));
            kit.CeilingLight(new Vector3(-7f, 3f, 47.5f));

            // ---------------------------------------------------------- N11: degrau final
            kit.Platform("N11 Degrau final", new Vector3(-7f, 0.75f, 53f), new Vector2(3f, 4f));
            kit.Wall("Parede degrau final E", new Vector3(-8.5f, 0.75f, 51f), new Vector3(-8.5f, 0.75f, 55f));
            kit.Wall("Parede degrau final frente", new Vector3(-8.5f, 0.75f, 55f), new Vector3(-5.5f, 0.75f, 55f));
            kit.CeilingLight(new Vector3(-7f, 3.75f, 53f));

            // ---------------------------------------------------------- N12..N13: patamar da porta
            kit.Platform("N12 Patamar da porta", new Vector3(-0.5f, 1.5f, 56f), new Vector2(7f, 8f));
            kit.Wall("Parede porta fundo E", new Vector3(-4f, 1.5f, 52f), new Vector3(-1f, 1.5f, 52f));
            kit.Wall("Parede porta fundo D", new Vector3(1f, 1.5f, 52f), new Vector3(3f, 1.5f, 52f));
            kit.Wall("Parede porta D", new Vector3(3f, 1.5f, 52f), new Vector3(3f, 1.5f, 60f));
            kit.Wall("Parede porta frente", new Vector3(-4f, 1.5f, 60f), new Vector3(3f, 1.5f, 60f));
            kit.CeilingLight(new Vector3(-2f, 4.5f, 55f));
            kit.CeilingLight(new Vector3(1f, 4.5f, 58.5f));
            kit.Pole(new Vector3(2.3f, 1.5f, 59.3f), PoleCyan);
            // voltada para o sul: vista do salão, ao fundo do corredor escuro
            kit.Goal(new Vector3(0f, 1.5f, 58f), 180f);

            kit.FallZone(-8f);
        }
    }
}
