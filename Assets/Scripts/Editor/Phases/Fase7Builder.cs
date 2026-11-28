using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 7 — "Falso Despertar": a luz da manhã entra na biblioteca e tudo parece o fim, mas o sonho continua
    /// (depois dela vêm as Fases 8 a 17). É a revisão de tudo o que o jogador aprendeu nas Fases 1 a 6.
    ///
    /// Forma: um anel de galerias que sobe em espiral em volta de um átrio vazio. No meio do átrio, uma torre de
    /// livros com um livro aberto gigante no topo (y 5,6) guarda a porta, visível desde o spawn. O jogador dá a volta
    /// inteira e, no fim, salta do mirante para a torre.
    ///   ato 1 (leste, y 0)        espinhos em ciclo alternados;
    ///   ato 2 (leste, y 0,5)      vão de 4,4 m que exige a Velocidade;
    ///   ato 3 (norte, y 1,2)      escolha: pisos que cedem (curto, arriscado) ou pilhas firmes mais altas (longo, seguro);
    ///   ato 4 (oeste, y 1,8)      Escudo e dois lasers alternados;
    ///   ato 5 (sudoeste, y 2,4)   pêndulo e prensa;
    ///   ato 6 (centro, y 4,1-5,8) Pulo Duplo para subir duas pilhas de 1,7 m, Pluma no mirante e o salto final de 9 m
    ///                             até a torre, que pede Pluma e pulo duplo juntos.
    /// Cada poder é coletado logo antes do trecho que o exige (o efeito dura 30 s).
    /// Medidas e contas em docs/Fase7.md.
    /// </summary>
    public sealed class Fase7Builder : IPhaseBuilder
    {
        public int Order => 7;
        public string SceneName => "Fase7_Despertar";
        public PhaseTheme Theme => PhaseTheme.Fase7;

        // alturas do topo dos pisos, subindo em espiral
        const float YEntrada = 0f;
        const float YLancamento = 0.5f; // plataformas do vão da Velocidade
        const float YNorte = 1.2f;      // ala norte (pisos que cedem e patamares)
        const float YOeste = 1.8f;      // ala oeste (escudo e lasers)
        const float YSudoeste = 2.4f;   // pêndulo, prensa e base da escalada
        const float YPilha1 = 4.1f;     // +1,7
        const float YMirante = 5.8f;    // +1,7
        const float YTorre = 5.6f;      // topo do livro aberto da porta

        // luz de manhã: branca e quente, mais clara que a das outras fases
        static readonly Color Manha = new Color(1f, 0.95f, 0.8f);
        static readonly Color Ceu = new Color(0.75f, 0.88f, 1f);

        public void Build(PhaseKit kit)
        {
            Entrada(kit);
            Ato1Espinhos(kit);
            Ato2Velocidade(kit);
            Ato3PisosQueCedem(kit);
            Ato4Lasers(kit);
            Ato5PenduloEPrensa(kit);
            Ato6Escalada(kit);
            Torre(kit);
            Horizonte(kit);

            kit.FallZone(-10f);
            // retângulo que cobre todas as plataformas e estantes (x -11,6..11,6, z -3,6..49,6)
            kit.LibraryBackdrop(new Vector2(-11.6f, -3.6f), new Vector2(11.6f, 49.6f), -6f, 16f, 6f);
        }

        // ------------------------------------------------------------------ entrada (N0)

        static void Entrada(PhaseKit kit)
        {
            // x [-4, 10], z [-3, 3]: o spawn olha para a torre (norte); a rota segue para a direita (leste)
            kit.Platform("N0 Entrada", P(3f, YEntrada, 0f), new Vector2(14f, 6f));
            kit.Spawn(P(0f, YEntrada, -1f), 0f);
            Estante(kit, "Entrada sul", -4.3f, -3.3f, 10.3f, -3.3f, YEntrada, 4f);
            Estante(kit, "Entrada oeste", -4.3f, -3.3f, -4.3f, 3.3f, YEntrada, 4f);
            Estante(kit, "Entrada leste", 10.3f, -3.3f, 10.3f, 3f, YEntrada, 4f);
            // parapeito de 1,4 m sobre o átrio: deixa ver a torre e impede a queda logo no começo
            Estante(kit, "Parapeito do atrio", -4.3f, 3.3f, 7.05f, 3.3f, -3f, 4.4f);

            kit.Sign("FASE 7", P(0f, 3.9f, 3.2f), 0f, 0.08f);
            kit.Sign("FALSO DESPERTAR", P(0f, 3.3f, 3.2f), 0f, 0.1f);

            kit.ReadingDesk("Mesa do despertar", P(-2.6f, YEntrada, -2.3f), 0f);
            kit.ReadingLamp(P(-3.6f, YEntrada, 2.4f), 135f, Manha);
            kit.ReadingLamp(P(5.5f, YEntrada, -2.6f), 0f, Manha);
            kit.Carpet(P(3.5f, YEntrada, 0f), new Vector2(10f, 2f));
            kit.CeilingLight(P(1f, 3.6f, 0f), 0f, Manha);
        }

        // ------------------------------------------------------------------ ato 1: espinhos em ciclo (N1..N2)

        static void Ato1Espinhos(PhaseKit kit)
        {
            // 1,7 m livres entre as estantes (faces em 7,65 e 9,35): os espinhos (1,8 m) cobrem a largura toda
            kit.ShelfArch("Ato 1", P(8.5f, YEntrada, 3f), 1.7f, 3.6f, 0f);
            kit.Platform("N1 Corredor dos espinhos", P(8.5f, YEntrada, 8f), new Vector2(3f, 10f)); // x[7,10] z[3,13]
            Estante(kit, "Espinhos oeste", 7.35f, 3.6f, 7.35f, 13f, YEntrada, 3.5f);
            Estante(kit, "Espinhos leste", 9.65f, 3.6f, 9.65f, 13f, YEntrada, 3.5f);
            // pares alternados (ciclo de 2,7 s; B meio ciclo à frente): atravessa A, espera no refúgio, atravessa B
            kit.Spikes("Espinhos A", P(8.5f, YEntrada, 6.5f), true, 1.2f, 1.5f, 0f);
            kit.Spikes("Espinhos B", P(8.5f, YEntrada, 10.5f), true, 1.2f, 1.5f, 1.35f);
            kit.Sign("1  RITMO", P(8.5f, 4.4f, 2.9f), 0f, 0.06f);
            kit.CeilingLight(P(8.5f, 3.6f, 8.5f), 90f, Manha);
        }

        // ------------------------------------------------------------------ ato 2: vão de 4,4 m com Velocidade (N2..N4)

        static void Ato2Velocidade(PhaseKit kit)
        {
            kit.Platform("N2 Patio do relampago", P(8.5f, YEntrada, 16f), new Vector2(5f, 6f)); // x[6,11] z[13,19]
            Estante(kit, "Parapeito do patio", 5.7f, 13f, 5.7f, 19f, -3f, 4.4f);           // 1,4 m sobre o piso
            kit.Pickup(PickupKind.Speed, P(9.8f, YEntrada, 15f));
            kit.ReadingLamp(P(10.4f, YEntrada, 13.6f), -45f, Manha);

            // lançamento e pouso de livros, 0,5 m acima do pátio; entre eles, 4,4 m de vazio
            kit.BookPlatform("N3 Lancamento", P(8.5f, YLancamento, 21f), new Vector2(3f, 4f), 0f, 2f);     // z[19,23]
            kit.BookPlatform("N4 Pouso", P(8.5f, YLancamento, 29.4f), new Vector2(3f, 4f), 0f, 2f);        // z[27,4;31,4]
            kit.Sign("2  CORRA!", P(8.5f, 4.6f, 25.2f), 0f, 0.07f);
            kit.CeilingLight(P(8.5f, 4f, 21f), 0f, Manha);
            kit.CeilingLight(P(8.5f, 4f, 29.4f), 0f, Manha);

            // estante alta que acompanha toda a ala leste, do lado de fora
            Estante(kit, "Ala leste", 11.3f, 3f, 11.3f, 45.3f, -3f, 7f);
        }

        // ------------------------------------------------------------------ ato 3: pisos que cedem × pilhas firmes (N5..N6)

        static void Ato3PisosQueCedem(PhaseKit kit)
        {
            // patamares das duas pontas da ala norte
            kit.Platform("N5 Patamar nordeste", P(8.5f, YNorte, 39f), new Vector2(4f, 12f));        // x[6,5;10,5] z[33,45]
            kit.Platform("N6 Patamar noroeste", P(-8.5f, YNorte, 42.85f), new Vector2(4f, 11.7f));  // x[-10,5;-6,5] z[37;48,7]
            kit.Sign("3  RAPIDO E FRAGIL", P(6.9f, YNorte + 2.6f, 39f), -90f, 0.04f);
            kit.Sign("OU LONGO E FIRME", P(6.9f, YNorte + 2.1f, 39f), -90f, 0.035f);

            // rota curta e arriscada: quatro pisos que cedem (0,8 s), sem luz, em linha reta
            kit.CrumblingPlatform("Piso que cede 1", P(5f, YNorte, 39f), new Vector2(2f, 2f), 0.8f);
            kit.CrumblingPlatform("Piso que cede 2", P(2f, YNorte, 39f), new Vector2(2f, 2f), 0.8f);
            kit.CrumblingPlatform("Piso que cede 3", P(-1f, YNorte, 39f), new Vector2(2f, 2f), 0.8f);
            kit.CrumblingPlatform("Piso que cede 4", P(-4f, YNorte, 39f), new Vector2(2f, 2f), 0.8f);

            // rota longa e segura: pilhas firmes mais ao norte, subindo e descendo 0,5 m, iluminadas
            kit.BookPlatform("Pilha firme Q1", P(8.5f, 1.7f, 47.5f), new Vector2(2.4f, 2.4f), 0f, 2f);
            kit.OpenBookPlatform("Pilha firme Q2", P(4.5f, 2.2f, 47.5f), new Vector2(2.4f, 2.4f), 0f);
            kit.BookPlatform("Pilha firme Q2 base", P(4.5f, 1.35f, 47.5f), new Vector2(2.2f, 2.2f), 0f, 2f);
            kit.OpenBookPlatform("Pilha firme Q3", P(0f, 2.2f, 47.5f), new Vector2(2.4f, 2.4f), 0f);
            kit.BookPlatform("Pilha firme Q3 base", P(0f, 1.35f, 47.5f), new Vector2(2.2f, 2.2f), 0f, 2f);
            kit.BookPlatform("Pilha firme Q4", P(-4.5f, 1.7f, 47.5f), new Vector2(2.4f, 2.4f), 0f, 2f);

            // estante alta do lado de fora; abajures sobre ela iluminam só a rota firme
            Estante(kit, "Ala norte", -11.6f, 49.3f, 11.6f, 49.3f, -3f, 7f);
            kit.ReadingLamp(P(8.5f, 4f, 49.3f), 180f, Manha);
            kit.ReadingLamp(P(2.2f, 4f, 49.3f), 180f, Manha);
            kit.ReadingLamp(P(-2.2f, 4f, 49.3f), 180f, Manha);
            kit.ReadingLamp(P(-8.5f, 4f, 49.3f), 180f, Manha);
            kit.CeilingLight(P(8.5f, 4.8f, 39f), 0f, Manha);
            kit.CeilingLight(P(-8.5f, 4.8f, 41f), 0f, Manha);
        }

        // ------------------------------------------------------------------ ato 4: Escudo e lasers alternados (N7..N8)

        static void Ato4Lasers(PhaseKit kit)
        {
            kit.BookPlatform("N7 Patamar do escudo", P(-8.5f, YOeste, 34f), new Vector2(4f, 4f), 0f, 2f); // x[-10,5;-6,5] z[32,36]
            kit.Pickup(PickupKind.Shield, P(-8.5f, YOeste, 34.5f));
            kit.Sign("4  ESCUDO = 1 TOQUE", P(-8.5f, YOeste + 4f, 32.1f), 180f, 0.045f);

            // corredor de 2,7 m (faces em -10,15+0,3 e -6,85-0,3), descendo em -Z; o arco marca a entrada
            kit.ShelfArch("Ato 4", P(-8.5f, YOeste, 32.3f), 2.7f, 3.6f, 0f);
            kit.Platform("N8 Corredor dos lasers", P(-8.5f, YOeste, 28f), new Vector2(3f, 8f)); // x[-10,-7] z[24,32]
            Estante(kit, "Lasers oeste", -10.15f, 24f, -10.15f, 31.7f, YOeste, 3.5f);
            Estante(kit, "Lasers leste", -6.85f, 24f, -6.85f, 31.7f, YOeste, 3.5f);
            // 1,5 s ligado e 1,8 s apagado; B está meio ciclo (1,65 s) defasado: atravessa A, espera, atravessa B
            kit.Laser("Laser A", P(-8.5f, YOeste, 30f), 0f, 1.5f, 1.8f, 0f);
            kit.Laser("Laser B", P(-8.5f, YOeste, 26f), 0f, 1.5f, 1.8f, 1.65f);
            kit.ReadingLamp(P(-10.15f, YOeste + 3.5f, 28f), 90f, Manha);

            // estante alta que acompanha a ala oeste por fora
            Estante(kit, "Ala oeste", -11.6f, 7f, -11.6f, 49.3f, -3f, 8f);
        }

        // ------------------------------------------------------------------ ato 5: pêndulo e prensa (N9..N11)

        static void Ato5PenduloEPrensa(PhaseKit kit)
        {
            float y = YSudoeste;
            // sala do pêndulo: 4 m entre estantes; o pêndulo balança em X de parede a parede
            kit.Platform("N9 Sala do pendulo", P(-8.5f, y, 20f), new Vector2(4f, 6f)); // x[-10,5;-6,5] z[17,23]
            Estante(kit, "Pendulo oeste", -10.8f, 17f, -10.8f, 23f, y, 3.5f);
            Estante(kit, "Pendulo leste", -6.2f, 16.7f, -6.2f, 23f, y, 3.5f);
            Estante(kit, "Pendulo sul O", -10.8f, 16.7f, -9.6f, 16.7f, y, 3.5f);
            Estante(kit, "Pendulo sul L", -7.4f, 16.7f, -6.2f, 16.7f, y, 3.5f); // só sai pela prensa
            kit.Pendulum("Pendulo", P(-8.5f, y, 20f), 55f, 2.4f, 0f, 0f);
            kit.Platform("Viga do pendulo", P(-8.5f, y + 4.3f, 20f), new Vector2(5.2f, 0.6f));
            kit.Sign("5  PENDULO E PRENSA", P(-8.5f, y + 4.9f, 23.2f), 180f, 0.045f);

            // corredor da prensa: 1,6 m livres (faces em -9,3 e -7,7), a prensa cobre a largura toda
            kit.Platform("N10 Corredor da prensa", P(-8.5f, y, 13.5f), new Vector2(2.2f, 7f)); // x[-9,6;-7,4] z[10,17]
            Estante(kit, "Prensa oeste", -9.6f, 10f, -9.6f, 16.7f, y, 3.5f);
            Estante(kit, "Prensa leste", -7.4f, 10f, -7.4f, 16.7f, y, 3.5f);
            kit.Press("Prensa", P(-8.5f, y, 13.5f), 2f, 0.8f, 0f, 0f);

            // curva até a base da escalada
            kit.Platform("N11 Curva", P(-8.05f, y, 9f), new Vector2(3.1f, 2f)); // x[-9,6;-6,5] z[8,10]
            Estante(kit, "Curva oeste", -9.9f, 7.7f, -9.9f, 10f, y, 3.5f);
            Estante(kit, "Curva sul", -9.9f, 7.7f, -6.5f, 7.7f, y, 3.5f);
            kit.CeilingLight(P(-8.5f, y + 3.6f, 18f), 0f, Manha);
            kit.CeilingLight(P(-8.5f, y + 3.6f, 11f), 90f, Manha);
        }

        // ------------------------------------------------------------------ ato 6: escalada e mirante (N12..N14)

        static void Ato6Escalada(PhaseKit kit)
        {
            // base da escalada: livro aberto com o Pulo Duplo
            kit.OpenBookPlatform("N12 Base da escalada", P(-3.2f, YSudoeste, 10f), new Vector2(4f, 4f), 0f); // x[-5,2;-1,2] z[8,12]
            kit.Pickup(PickupKind.DoubleJump, P(-3.2f, YSudoeste, 9.5f));
            kit.Sign("6  SUBA E VOE", P(-3.2f, YSudoeste + 4.6f, 8.1f), 0f, 0.05f);

            // duas pilhas de +1,7 m; estantes altas sob elas fecham as faces de baixo até o vazio
            kit.BookPlatform("N13 Pilha 1", P(-2.4f, YPilha1, 13.2f), new Vector2(2.4f, 2.4f), 0f, 1.2f);   // x[-3,6;-1,2] z[12;14,4]
            Estante(kit, "Sob a pilha 1", -3.6f, 12.3f, -1.2f, 12.3f, -3f, YPilha1 - 0.5f + 3f);
            kit.BookPlatform("N14 Mirante", P(-2.4f, YMirante, 16.8f), new Vector2(2.4f, 2.4f), 0f, 1.2f);  // x[-3,6;-1,2] z[15,6;18]
            Estante(kit, "Sob o mirante", -3.6f, 15.9f, -1.2f, 15.9f, -3f, YMirante - 0.5f + 3f);

            // a Pluma fica no mirante: o salto de 9 m até a torre pede Pluma e pulo duplo juntos
            kit.Pickup(PickupKind.Gravity, P(-2.4f, YMirante, 16.6f));
            kit.CeilingLight(P(-2.4f, YMirante + 4.6f, 16.8f), 0f, Ceu);
        }

        // ------------------------------------------------------------------ torre e porta (N15, G)

        static void Torre(PhaseKit kit)
        {
            // o livro aberto gigante no topo de uma torre de livros; a porta olha para o sul (para o spawn)
            kit.OpenBookPlatform("N15 Livro da manha", P(0f, YTorre, 30f), new Vector2(6f, 6f), 0f); // x[-3,3] z[27,33]
            kit.BookPlatform("Torre de livros", P(0f, YTorre - 0.6f, 30f), new Vector2(5f, 5f), 0f, 10f);
            kit.Goal(P(0f, YTorre, 31.5f), 180f);
            kit.Sign("ACORDOU?", P(0f, YTorre + 4f, 32.6f), 0f, 0.07f);
            kit.Sign("o sonho continua...", P(0f, YTorre + 3.5f, 32.6f), 0f, 0.04f);
            kit.ReadingLamp(P(-2.4f, YTorre, 32.4f), 180f, Manha);
            kit.ReadingLamp(P(2.4f, YTorre, 32.4f), 180f, Manha);
            kit.CeilingLight(P(0f, YTorre + 5f, 29f), 0f, Manha);
        }

        // ------------------------------------------------------------------ marcos e cenário

        static void Horizonte(PhaseKit kit)
        {
            // relógio gigante atrás da torre, o maior marco da fase: visto do spawn por cima da porta
            kit.DreamClock("Manha", P(0f, 14f, 52f), 4f, 0f, 6f);
            // páginas e livros no átrio, longe do anel e do salto final
            kit.FloatingPages("Atrio", P(2.5f, 1f, 20f), 2f, 12);
            kit.FloatingPages("Alto da torre", P(0f, 12.5f, 30f), 2.5f, 14);
            kit.FloatingBooks("Sob a torre", P(0f, -3f, 30f), 4f, 8);
            kit.FloatingBooks("Atrio norte", P(1f, -2f, 42f), 3f, 6);
            kit.DreamMotes("Luz da manha", P(2f, -2f, 22f), new Vector3(6f, 4f, 10f), Manha, 160); // abaixo do anel e do salto
        }

        // ------------------------------------------------------------------ utilitários

        static Vector3 P(float x, float y, float z) => new Vector3(x, y, z);

        /// <summary>Estante sólida (parede de 0,6 m) de (x1, z1) até (x2, z2), com base em <paramref name="y"/>.</summary>
        static void Estante(PhaseKit kit, string nome, float x1, float z1, float x2, float z2, float y, float altura) =>
            kit.Bookshelf(nome, new Vector3(x1, y, z1), new Vector3(x2, y, z2), altura);
    }
}
