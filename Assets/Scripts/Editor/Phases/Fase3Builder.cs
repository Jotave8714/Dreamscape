using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 3 — "Corredor das Luzes": a fase que apresenta as armadilhas, num corredor de estantes da biblioteca do sonho.
    /// Regra visual: onde há luminária no teto, há chão firme.
    ///
    /// Ritmo (+Z):
    ///  1. Sala de leitura (spawn). A regra é apresentada em segurança: um piso escuro ao lado do caminho cede
    ///     e derruba o jogador num fosso de 0,8 m, de onde ele sobe sozinho.
    ///  2. Sala dos espinhos fixos (desviar) e corredor apertado de estantes com espinhos em ciclo (esperar o ritmo).
    ///  3. Salão da escolha, amplo e alto: rota curta e arriscada com a Pluma (vão de 5,5 m) ou rota longa e segura
    ///     pela galeria lateral de livros gigantes.
    ///  4. Salão de chegada com o truque do corredor escuro (piso falso apontando para a porta).
    ///  5. Desafio final: corredor do laser, degrau de livros e o livro aberto gigante onde fica a porta.
    /// O relógio do sonho flutua alto, atrás da porta, e é visível desde o spawn pelo arco da sala de leitura.
    /// Pontos, grafo e cálculos em docs/Fase3.md.
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
            SalaDeLeitura(kit);
            EspinhosFixos(kit);
            CorredorDoRitmo(kit);
            SalaoDaEscolha(kit);
            GaleriaLateral(kit);
            SalaoDeChegada(kit);
            CorredorEscuro(kit);
            LaserEPorta(kit);

            // marco no horizonte: alto, atrás da porta, com o mostrador voltado para o spawn
            kit.DreamClock("Horizonte", new Vector3(0f, 12f, 66f), 3f, 0f, 3f);
            kit.FloatingBooks("Acima da porta", new Vector3(4f, 8.5f, 62f), 3f, 7);

            kit.FallZone(-8f);
            // moldura de estantes gigantes cobrindo x -9,5..10,5 e z -4..61
            kit.LibraryBackdrop(new Vector2(-9.5f, -4f), new Vector2(10.5f, 61f), -5f, 14f, 6f);
        }

        // ------------------------------------------------------------------ N0: sala de leitura (spawn)

        static void SalaDeLeitura(PhaseKit kit)
        {
            // piso x -3..3, z -3..2, com um fosso de 2 x 2 m no lado esquerdo (x -3..-1, z -1,5..0,5)
            kit.Platform("N0 Entrada", new Vector3(1f, 0f, -0.5f), new Vector2(4f, 5f));
            kit.Platform("N0 Entrada canto E", new Vector3(-2f, 0f, -2.25f), new Vector2(2f, 1.5f));
            kit.Platform("N0 Entrada frente E", new Vector3(-2f, 0f, 1.25f), new Vector2(2f, 1.5f));
            // apresentação da regra em segurança: piso escuro (sem luminária) que cede sobre um fosso raso.
            // Quem pisa cai 0,8 m e volta com um pulo (degrau de 0,8 m; o pulo sobe 1,1 m)
            kit.CrumblingPlatform("N0 Piso escuro", new Vector3(-2f, 0f, -0.5f), new Vector2(2f, 2f), 0.8f);
            kit.Platform("N0 Fosso", new Vector3(-2f, -0.8f, -0.5f), new Vector2(2f, 2f));
            kit.Spawn(new Vector3(0.5f, 0f, -0.8f), 0f);

            // estantes: a da esquerda desce até o fundo do fosso, para ninguém sair por baixo dela
            kit.Bookshelf("N0 fundo", new Vector3(-3.6f, 0f, -3.3f), new Vector3(3.6f, 0f, -3.3f), 3.5f);
            kit.Bookshelf("N0 esquerda", new Vector3(-3.3f, -0.8f, -3f), new Vector3(-3.3f, -0.8f, 2.6f), 3.8f);
            kit.Bookshelf("N0 direita", new Vector3(3.3f, 0f, -3f), new Vector3(3.3f, 0f, 2.6f), 3f);
            kit.Bookshelf("N0 frente E", new Vector3(-3f, 0f, 2.3f), new Vector3(-1.8f, 0f, 2.3f), 3f);
            kit.Bookshelf("N0 frente D", new Vector3(1.8f, 0f, 2.3f), new Vector3(3f, 0f, 2.3f), 3f);
            // arco de saída: emoldura o corredor e, lá no fundo, o relógio sobre a porta
            kit.ShelfArch("Saida da sala de leitura", new Vector3(0f, 0f, 2f), 2.4f, 3.6f, 0f);

            kit.CeilingLight(new Vector3(1f, 3f, -0.5f));
            kit.CeilingLight(new Vector3(0f, 3f, 1.2f));
            kit.ReadingDesk("Mesa da sala de leitura", new Vector3(2.2f, 0f, -2.4f), 0f);
            kit.Carpet(new Vector3(0.2f, 0f, 0.3f), new Vector2(2f, 3f), 0f);
            kit.ReadingLamp(new Vector3(3.3f, 3f, -1.5f), 0f);
            kit.ReadingLamp(new Vector3(-3.3f, 3f, -2.2f), 0f);

            kit.Sign("FASE 3", new Vector3(0f, 4.5f, 1.3f), 0f, 0.09f);
            kit.Sign("CORREDOR DAS LUZES", new Vector3(0f, 3.75f, 1.3f), 0f, 0.05f);
            // dica ao lado do fosso, na estante esquerda (lida de quem olha para -X)
            kit.Sign("ONDE HÁ LUZ, HÁ CHÃO", new Vector3(-2.95f, 2f, -0.5f), -90f, 0.045f);
        }

        // ------------------------------------------------------------------ N1: sala dos espinhos fixos

        static void EspinhosFixos(PhaseKit kit)
        {
            // vão de 2 m (alcance correndo 3,32 m). Duas fileiras desencontradas: a passagem livre muda de lado
            kit.Platform("N1 Sala dos espinhos fixos", new Vector3(0f, 0f, 8.5f), new Vector2(6f, 9f));
            kit.Spikes("Fixo 1A", new Vector3(-2f, 0f, 7.5f));
            kit.Spikes("Fixo 1B", new Vector3(0f, 0f, 7.5f));   // fileira 1: passa pela direita (x = +2)
            kit.Spikes("Fixo 2A", new Vector3(0f, 0f, 11f));
            kit.Spikes("Fixo 2B", new Vector3(2f, 0f, 11f));    // fileira 2: passa pela esquerda (x = -2)

            // estantes por fora do piso (face interna na borda); a frente deixa a boca de 1,7 m do corredor
            kit.Bookshelf("N1 esquerda", new Vector3(-3.3f, 0f, 3.7f), new Vector3(-3.3f, 0f, 13.6f), 3.5f);
            kit.Bookshelf("N1 direita", new Vector3(3.3f, 0f, 3.7f), new Vector3(3.3f, 0f, 13.6f), 3.5f);
            kit.Bookshelf("N1 frente E", new Vector3(-3.6f, 0f, 13.3f), new Vector3(-1.45f, 0f, 13.3f), 3.5f);
            kit.Bookshelf("N1 frente D", new Vector3(1.45f, 0f, 13.3f), new Vector3(3.6f, 0f, 13.3f), 3.5f);

            kit.CeilingLight(new Vector3(0f, 3f, 5.5f));
            kit.CeilingLight(new Vector3(0f, 3f, 10f));
            kit.ReadingLamp(new Vector3(-3.3f, 3.5f, 5f), 0f);
            kit.ReadingLamp(new Vector3(3.3f, 3.5f, 12f), 180f);
        }

        // ------------------------------------------------------------------ N2..N3: corredor do ritmo

        static void CorredorDoRitmo(PhaseKit kit)
        {
            // corredor apertado entre estantes altas: 1,7 m livres (faces em x = ±0,85).
            // Os espinhos (1,8 m) cobrem a largura toda, só passa no ritmo.
            // Ciclo de 2,7 s (1,2 s expostos, 1,5 s recolhidos). O par B está meio ciclo atrasado em relação ao A:
            // atravessa-se A, espera-se no refúgio (N3, marcado pelo tapete) e atravessa-se B.
            kit.Platform("N2-N3 Corredor do ritmo", new Vector3(0f, 0f, 18.5f), new Vector2(2f, 11f));
            kit.Spikes("Ritmo A1", new Vector3(0f, 0f, 15f), true, 1.2f, 1.5f, 0f);
            kit.Spikes("Ritmo A2", new Vector3(0f, 0f, 17f), true, 1.2f, 1.5f, 0f);
            kit.Spikes("Ritmo B1", new Vector3(0f, 0f, 21f), true, 1.2f, 1.5f, 1.35f);
            kit.Spikes("Ritmo B2", new Vector3(0f, 0f, 23f), true, 1.2f, 1.5f, 1.35f);
            kit.Bookshelf("Ritmo esquerda", new Vector3(-1.15f, 0f, 13f), new Vector3(-1.15f, 0f, 24f), 4.5f);
            kit.Bookshelf("Ritmo direita", new Vector3(1.15f, 0f, 13f), new Vector3(1.15f, 0f, 24f), 4.5f);
            // refúgio entre os pares (z 17,9..20,1): tapete com o friso da cor de destaque
            kit.Carpet(new Vector3(0f, 0f, 19f), new Vector2(1.4f, 1.8f), 0f);

            kit.CeilingLight(new Vector3(0f, 3f, 15.5f));
            kit.CeilingLight(new Vector3(0f, 3f, 19f));
            kit.CeilingLight(new Vector3(0f, 3f, 22.5f));
            kit.Sign("OBSERVE O RITMO", new Vector3(0f, 2.6f, 12.8f), 0f, 0.05f);
        }

        // ------------------------------------------------------------------ N4..N5: salão da escolha

        static void SalaoDaEscolha(PhaseKit kit)
        {
            // salão amplo e alto logo depois do corredor apertado
            kit.Platform("N4 Patamar da escolha", new Vector3(0f, 0f, 28f), new Vector2(8f, 8f));
            kit.BookPlatform("N5 Pedestal da Pluma", new Vector3(-3f, 0.5f, 27f), new Vector2(2f, 2f), 0f, 0.5f);
            kit.Pickup(PickupKind.Gravity, new Vector3(-3f, 0.5f, 27f));

            kit.Bookshelf("N4 fundo E", new Vector3(-4.6f, 0f, 23.7f), new Vector3(-1.45f, 0f, 23.7f), 6f);
            kit.Bookshelf("N4 fundo D", new Vector3(1.45f, 0f, 23.7f), new Vector3(4f, 0f, 23.7f), 6f);
            kit.Bookshelf("N4 esquerda", new Vector3(-4.3f, 0f, 23.4f), new Vector3(-4.3f, 0f, 32f), 6f);
            // os pilares atrapalham a corrida para pular direto de N4 a N7 (ver docs/Fase3.md)
            kit.Pillar("Pilar patamar E", new Vector3(-3.5f, 0f, 31.5f));
            kit.Pillar("Pilar patamar D", new Vector3(3.5f, 0f, 31.5f));
            kit.ReadingDesk("Mesa do salao da escolha", new Vector3(2.5f, 0f, 25.2f), 0f);

            kit.CeilingLight(new Vector3(0f, 3f, 29f));
            kit.CeilingLight(new Vector3(-3f, 3.5f, 26.5f), 0f, FeatherBlue);
            kit.Pole(new Vector3(3.3f, 0f, 24.7f), PoleCyan);
            kit.ReadingLamp(new Vector3(-4.3f, 6f, 30f), 0f);

            // rastro de páginas bem acima do vão de 5,5 m (z 32 -> 37,5): sugere o voo da Pluma, fora do alcance
            kit.FloatingPages("Rastro da Pluma", new Vector3(0f, 6.2f, 34.75f), 2f, 8);
            kit.FloatingBooks("Teto do salao", new Vector3(-1f, 7.5f, 28f), 3f, 6);
        }

        // ------------------------------------------------------------------ N6..N7: galeria lateral (rota longa e segura)

        static void GaleriaLateral(PhaseKit kit)
        {
            kit.BookPlatform("N6 Galeria lateral", new Vector3(7.5f, 0f, 29f), new Vector2(3f, 8f));
            kit.Bookshelf("N6 fundo", new Vector3(6f, 0f, 24.7f), new Vector3(9.6f, 0f, 24.7f), 3f);
            kit.Bookshelf("N6 direita", new Vector3(9.3f, 0f, 24.4f), new Vector3(9.3f, 0f, 33f), 3f);
            kit.CeilingLight(new Vector3(7.5f, 3f, 27f));
            kit.CeilingLight(new Vector3(7.5f, 3f, 31f));
            kit.ReadingLamp(new Vector3(9.3f, 3f, 29f), 0f);

            kit.BookPlatform("N7 Degrau lateral", new Vector3(8f, 0.5f, 37.5f), new Vector2(3f, 5f));
            kit.Bookshelf("N7 direita", new Vector3(9.8f, 0.5f, 35f), new Vector3(9.8f, 0.5f, 40.6f), 3f);
            kit.Bookshelf("N7 frente", new Vector3(6.5f, 0.5f, 40.3f), new Vector3(10.1f, 0.5f, 40.3f), 3f);
            kit.CeilingLight(new Vector3(8f, 3.5f, 37.5f));
            kit.FloatingBooks("Fora da galeria", new Vector3(13f, 2f, 33f), 2f, 5);
        }

        // ------------------------------------------------------------------ N8: salão de chegada

        static void SalaoDeChegada(PhaseKit kit)
        {
            kit.Platform("N8 Salao", new Vector3(0f, 0f, 41.5f), new Vector2(8f, 8f));
            // a frente deixa a boca de 1,7 m do corredor escuro (faces em x = ±0,85)
            kit.Bookshelf("N8 frente E", new Vector3(-4f, 0f, 45.8f), new Vector3(-1.45f, 0f, 45.8f), 3.5f);
            kit.Bookshelf("N8 frente D", new Vector3(1.45f, 0f, 45.8f), new Vector3(4.6f, 0f, 45.8f), 3.5f);
            kit.Bookshelf("N8 direita", new Vector3(4.3f, 0f, 40f), new Vector3(4.3f, 0f, 46.1f), 3.5f);
            kit.Bookshelf("N8 esquerda", new Vector3(-4.3f, 0f, 37.5f), new Vector3(-4.3f, 0f, 40f), 3.5f);
            kit.ReadingDesk("Mesa do salao", new Vector3(2.6f, 0f, 44.6f), 0f);
            // tapete apontando para o oeste: a rota verdadeira (corredor do laser), não o corredor escuro
            kit.Carpet(new Vector3(-1.8f, 0f, 42.75f), new Vector2(4f, 1.4f), 0f);

            // luzes afastadas da boca do corredor escuro, para ele continuar sem luz
            kit.CeilingLight(new Vector3(-2f, 3f, 40f));
            kit.CeilingLight(new Vector3(2f, 3f, 40f));
            kit.Pole(new Vector3(-3.3f, 0f, 38.2f), PoleCyan);
            kit.ReadingLamp(new Vector3(4.3f, 3.5f, 42f), 0f);
            kit.Sign("SIGA AS LUZES", new Vector3(-2.5f, 1.9f, 45.45f), 0f, 0.05f);
        }

        // ------------------------------------------------------------------ T0: truque de cenário

        static void CorredorEscuro(PhaseKit kit)
        {
            // Parece o caminho direto: a porta aparece no fim, em linha reta. Mas não há luminária, o piso é falso
            // (cede 0,8 s depois de pisado) e a porta está 2,5 m acima do piso do corredor (o pulo alcança 1,1 m;
            // com a Pluma, 2,2 m). Quem desce sente o tremor e ainda pode voltar ao salão (1 m acima) se for rápido.
            kit.CrumblingPlatform("T0 Corredor escuro", new Vector3(0f, -1f, 48.75f), new Vector2(2f, 6.5f), 0.8f);
            kit.Bookshelf("Escuro esquerda", new Vector3(-1.15f, -1f, 45.5f), new Vector3(-1.15f, -1f, 52f), 4f);
            kit.Bookshelf("Escuro direita", new Vector3(1.15f, -1f, 45.5f), new Vector3(1.15f, -1f, 52f), 4f);
            // fundo: topo em y = 1,5 (rente ao patamar da porta), 2,5 m acima do piso falso
            kit.Bookshelf("Escuro fim", new Vector3(-0.85f, -1f, 51.7f), new Vector3(0.85f, -1f, 51.7f), 2.5f);
        }

        // ------------------------------------------------------------------ N9..N13: laser, degrau e porta

        static void LaserEPorta(PhaseKit kit)
        {
            // largura interna 2,7 m (faces em x = -8,35 e -5,65) = largura do feixe: só passa com o laser apagado
            kit.Platform("N9-N10 Corredor do laser", new Vector3(-7f, 0f, 45f), new Vector2(3f, 10f));
            kit.Laser("Laser do corredor", new Vector3(-7f, 0f, 47.5f), 0f, 1.5f, 1.8f, 0f);
            kit.Bookshelf("Laser fundo", new Vector3(-8.95f, 0f, 39.7f), new Vector3(-5.05f, 0f, 39.7f), 3.5f);
            kit.Bookshelf("Laser esquerda", new Vector3(-8.65f, 0f, 39.4f), new Vector3(-8.65f, 0f, 50f), 3.5f);
            kit.Bookshelf("Laser direita", new Vector3(-5.35f, 0f, 45.5f), new Vector3(-5.35f, 0f, 50f), 3.5f);
            kit.CeilingLight(new Vector3(-7f, 3f, 42.5f));
            kit.CeilingLight(new Vector3(-7f, 3f, 47.5f));
            kit.ReadingLamp(new Vector3(-8.65f, 3.5f, 43f), 0f);

            // degrau final de livros gigantes
            kit.BookPlatform("N11 Degrau final", new Vector3(-7f, 0.75f, 53f), new Vector2(3f, 4f));
            kit.Bookshelf("N11 esquerda", new Vector3(-8.65f, 0.75f, 51f), new Vector3(-8.65f, 0.75f, 55.3f), 3f);
            kit.Bookshelf("N11 frente", new Vector3(-8.95f, 0.75f, 55.3f), new Vector3(-5.5f, 0.75f, 55.3f), 3f);
            kit.CeilingLight(new Vector3(-7f, 3.75f, 53f));

            // patamar da porta: um livro aberto gigante, o marco de chegada
            kit.OpenBookPlatform("N12 Patamar da porta", new Vector3(-0.5f, 1.5f, 56f), new Vector2(7f, 8f), 0f);
            kit.Bookshelf("Porta fundo E", new Vector3(-3f, 1.5f, 51.7f), new Vector3(-1.45f, 1.5f, 51.7f), 3f);
            kit.Bookshelf("Porta fundo D", new Vector3(1.45f, 1.5f, 51.7f), new Vector3(3.6f, 1.5f, 51.7f), 3f);
            kit.Bookshelf("Porta direita", new Vector3(3.3f, 1.5f, 51.4f), new Vector3(3.3f, 1.5f, 60.3f), 3f);
            kit.Bookshelf("Porta frente", new Vector3(-4.3f, 1.5f, 60.3f), new Vector3(3.6f, 1.5f, 60.3f), 4f);
            kit.CeilingLight(new Vector3(-2f, 4.5f, 55f));
            kit.CeilingLight(new Vector3(1f, 4.5f, 58.5f));
            kit.Pole(new Vector3(2.3f, 1.5f, 59.3f), PoleCyan);
            kit.ReadingLamp(new Vector3(-2.5f, 5.5f, 60.3f), 90f);
            kit.ReadingLamp(new Vector3(2f, 5.5f, 60.3f), 90f);
            // voltada para o sul: vista do salão, ao fundo do corredor escuro
            kit.Goal(new Vector3(0f, 1.5f, 58f), 180f);
        }
    }
}
