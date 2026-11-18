using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 5 — Laboratório (tema roxo/rosa): o laboratório de alquimia escondido dentro da biblioteca do sonho.
    /// Mecânica central: lasers com ritmos diferentes; o Escudo aparece como atalho opcional.
    ///
    /// Ritmo da fase:
    ///   1. apresentar  — recepção e corredor de calibração: um laser só, com janela longa (2 s apagado);
    ///   2. testar      — bancadas de livros subindo sobre o fosso até a sala de triagem (1,5 m acima da entrada);
    ///   3. escolher    — da triagem saem três caminhos:
    ///        • em frente, a passarela escura que aponta direto para a porta (truque: piso falso, sem luz);
    ///        • a oeste, a rota longa e segura (desce à galeria dos lasers em onda, serra e espinhos);
    ///        • a leste, a rota curta e arriscada: uma ponte de livros sobre o vazio, o Escudo e o laser mais difícil.
    ///   As duas rotas verdadeiras terminam no livro aberto gigante da sala final, onde fica a porta.
    ///
    /// Regra visual: arcos de estante com friso rosa e luzes marcam a rota; o que não tem luz não é chão firme.
    /// Eixo Z aponta da entrada para a conclusão; X negativo = oeste (rota segura), X positivo = leste (rota curta).
    /// Coordenadas, grafo e contas dos pulos: docs/Fase5.md (os números de lá batem com os daqui).
    /// </summary>
    public sealed class Fase5Builder : IPhaseBuilder
    {
        public int Order => 5;
        public string SceneName => "Fase5_Laboratorio";
        public PhaseTheme Theme => PhaseTheme.Fase5;

        static readonly Color Rosa = new Color(1f, 0.45f, 0.75f);
        static readonly Color Lilas = new Color(0.75f, 0.55f, 1f);
        static readonly Color Vela = new Color(1f, 0.8f, 0.55f);

        // alturas do topo dos pisos
        const float YEntrada = 0f;
        const float YTriagem = 1.5f;   // sala de triagem e ponte do escudo
        const float YGaleria = 0.6f;   // galeria dos lasers em onda (0,9 m abaixo da triagem: dá para voltar)
        const float YCentrifuga = 1.4f; // sala da centrífuga e passagem de amostras
        const float YLeste = 1.9f;     // patamar de chegada da ponte
        const float YFinal = 2.3f;     // livro aberto da sala final

        // ritmos dos lasers (segundos). Cada laser tem ciclo e offset próprios; o tempo é o da cena (zera a cada tentativa).
        const float CalibracaoLigado = 1.2f, CalibracaoDesligado = 2.0f, CalibracaoOffset = 0.7f;
        // os dois lentos têm o mesmo ciclo (4 s); o segundo está 0,6 s atrasado em relação ao primeiro
        // (offset 3,9 ≡ 0,5 − 0,6 no ciclo de 4 s): quem corre os 3 m entre eles a 5 m/s passa pelos dois na mesma janela
        const float LentoLigado = 1.5f, LentoDesligado = 2.5f, Lento1Offset = 0.5f, Lento2Offset = 3.9f;
        // o laser da barreira fica ligado 4,2 s e só abre 0,8 s por ciclo: é o que o Escudo resolve
        const float BarreiraLigado = 4.2f, BarreiraDesligado = 0.8f, BarreiraOffset = 1.3f;

        public void Build(PhaseKit kit)
        {
            Recepcao(kit);
            CorredorDeCalibracao(kit);
            SalaDasBancadas(kit);
            SalaDeTriagem(kit);
            PassarelaFalsa(kit);
            RotaSegura(kit);
            RotaCurta(kit);
            SalaFinal(kit);
            Horizonte(kit);

            kit.FallZone(-8f);
            // retângulo que cobre todas as plataformas (x -14..7, z -4,5..58,5)
            kit.LibraryBackdrop(new Vector2(-14f, -4.5f), new Vector2(7f, 58.5f), -6f, 14f, 6f);
        }

        // ------------------------------------------------------------------ 1. recepção (N0)

        static void Recepcao(PhaseKit kit)
        {
            kit.Platform("Piso Recepcao", P(0f, YEntrada, 0f), new Vector2(8f, 8f)); // x[-4,4] z[-4,4]
            Estante(kit, "Recepcao sul", -4.3f, -4.3f, 4.3f, -4.3f, YEntrada);
            Estante(kit, "Recepcao oeste", -4.3f, -4.3f, -4.3f, 4.3f, YEntrada);
            Estante(kit, "Recepcao leste", 4.3f, -4.3f, 4.3f, 4.3f, YEntrada);
            Estante(kit, "Recepcao norte O", -4.3f, 4.3f, -1.95f, 4.3f, YEntrada);
            Estante(kit, "Recepcao norte L", 1.95f, 4.3f, 4.3f, 4.3f, YEntrada);
            // o arco emoldura a boca do corredor de calibração (vão de 2,7 m, igual ao feixe)
            kit.ShelfArch("Entrada do laboratorio", P(0f, YEntrada, 4.3f), 2.7f, 3.6f, 0f);

            kit.Spawn(P(0f, YEntrada, -1.5f), 0f);
            kit.Sign("FASE 5", P(0f, 4.7f, 3.6f), 0f, 0.09f);
            kit.Sign("LABORATORIO DE ALQUIMIA", P(0f, 4.2f, 3.6f), 0f, 0.06f);

            // mesas de pesquisa nos cantos do fundo (fora da linha do pufe ao arco)
            kit.ReadingDesk("Recepcao mesa O", P(-2.8f, YEntrada, -2.9f), 0f);
            kit.ReadingDesk("Recepcao mesa L", P(2.8f, YEntrada, -2.9f), 0f);
            kit.ReadingLamp(P(-3.5f, YEntrada, 3.4f), 135f);
            kit.ReadingLamp(P(3.5f, YEntrada, 3.4f), -135f);
            kit.Carpet(P(0f, YEntrada, 0.5f), new Vector2(2.4f, 6f));
            kit.CeilingLight(P(0f, 3.6f, 0f), 0f, Lilas);
        }

        // ------------------------------------------------------------------ 2. corredor de calibração (N1 → N2)

        static void CorredorDeCalibracao(PhaseKit kit)
        {
            // estantes nas linhas x = ±1,65: as faces internas (±1,35) deixam 2,7 m livres, exatamente o comprimento do feixe
            kit.Platform("Piso Corredor de calibracao", P(0f, YEntrada, 9f), new Vector2(3f, 10f)); // z[4,14]
            Estante(kit, "Calibracao oeste", -1.65f, 4.9f, -1.65f, 14f, YEntrada);
            Estante(kit, "Calibracao leste", 1.65f, 4.9f, 1.65f, 14f, YEntrada);

            kit.Laser("Laser de calibracao", P(0f, YEntrada, 9f), 0f, CalibracaoLigado, CalibracaoDesligado, CalibracaoOffset);
            kit.Sign("OBSERVE O RITMO", P(0f, 3.0f, 5.0f), 0f, 0.05f);

            // abajures em cima das estantes, debruçados sobre o corredor
            kit.ReadingLamp(P(-1.65f, YEntrada + 3.5f, 6.5f), 90f);
            kit.ReadingLamp(P(1.65f, YEntrada + 3.5f, 11.5f), -90f);
            kit.CeilingLight(P(0f, 3.6f, 12f), 90f);
        }

        // ------------------------------------------------------------------ 3. sala das bancadas (N2 → N5)

        static void SalaDasBancadas(PhaseKit kit)
        {
            // pilhas de livros subindo sobre o fosso; quem cai volta ao início
            kit.Platform("Patamar das bancadas", P(0f, YEntrada, 15f), new Vector2(6f, 2f));             // z[14,16]  y 0
            kit.BookPlatform("Bancada 1", P(-0.75f, 0.5f, 18f), new Vector2(2.5f, 2f), 0f, 1.6f);        // x[-2;0,5]   z[17,19]     y 0,5
            kit.BookPlatform("Bancada 2", P(0.75f, 1.0f, 21.5f), new Vector2(2.5f, 2f), 0f, 2f);         // x[-0,5;2]   z[20,5;22,5] y 1,0
            kit.OpenBookPlatform("Bancada 3 livro aberto", P(-0.5f, 1.5f, 25f), new Vector2(3f, 2f), 0f); // x[-2;1]  z[24,26] y 1,5

            // paredes do fosso: estantes que descem para a escuridão
            Estante(kit, "Bancadas sul O", -3.6f, 13.7f, -1.95f, 13.7f, -3f, 6.5f);
            Estante(kit, "Bancadas sul L", 1.95f, 13.7f, 3.6f, 13.7f, -3f, 6.5f);
            Estante(kit, "Bancadas oeste", -3.3f, 14f, -3.3f, 27f, -3f, 6.5f);
            Estante(kit, "Bancadas leste", 3.3f, 14f, 3.3f, 27f, -3f, 6.5f);
            kit.LibraryLadder(P(2.4f, -3f, 19.5f), 2.5f, -90f); // some no fosso, abaixo do caminho

            // livros caindo devagar no fosso, abaixo de todas as bancadas
            kit.FloatingBooks("Fosso das bancadas", P(0f, -3f, 21f), 2f, 6);
            kit.ReadingLamp(P(-3.3f, 3.5f, 17f), 90f);
            kit.ReadingLamp(P(3.3f, 3.5f, 23.5f), -90f);
            kit.CeilingLight(P(0f, 4.6f, 18f), 0f, Lilas);
            kit.CeilingLight(P(0f, 5.2f, 24f));
        }

        // ------------------------------------------------------------------ 4. sala de triagem: a bifurcação (N6)

        static void SalaDeTriagem(PhaseKit kit)
        {
            float y = YTriagem;
            kit.Platform("Piso Triagem", P(0f, y, 31f), new Vector2(12f, 8f)); // x[-6,6] z[27,35]
            Estante(kit, "Triagem sul O", -6.3f, 27f, -3.3f, 27f, y);
            Estante(kit, "Triagem sul L", 3.3f, 27f, 6.3f, 27f, y);
            Estante(kit, "Triagem oeste A", -6.3f, 27f, -6.3f, 29.4f, y);
            Estante(kit, "Triagem oeste B", -6.3f, 33.6f, -6.3f, 35.3f, y);
            Estante(kit, "Triagem leste", 6.3f, 27f, 6.3f, 35.3f, y);
            Estante(kit, "Triagem norte O", -6.3f, 35.3f, -1.6f, 35.3f, y);
            Estante(kit, "Triagem norte meio", 1.6f, 35.3f, 2.7f, 35.3f, y);

            // arcos com friso rosa só nas saídas verdadeiras; a passarela escura fica sem arco e sem luz
            kit.ShelfArch("Saida oeste", P(-6.3f, y, 31.5f), 3f, 3.6f, 90f);    // vão z[30,33]
            kit.ShelfArch("Saida da ponte", P(4.65f, y, 35.3f), 2.4f, 3.6f, 0f); // vão x[3,45;5,85]

            // bancadas de alquimia junto à parede sul (obstáculos fora das linhas de saída)
            kit.ReadingDesk("Triagem mesa O", P(-4.4f, y, 28f), 0f);
            kit.ReadingDesk("Triagem mesa L", P(4.4f, y, 28f), 0f);
            kit.ReadingLamp(P(-5.6f, y, 34.6f), 135f);
            kit.ReadingLamp(P(5.6f, y, 29.2f), -45f);
            kit.Carpet(P(0f, y, 30.5f), new Vector2(7f, 3f));

            kit.Sign("SIGA AS LUZES", P(-3.9f, y + 2.2f, 35f), 0f, 0.05f);
            kit.Sign("ESCOLHA SEU CAMINHO", P(0f, y + 3.9f, 34.9f), 0f, 0.05f);
            kit.CeilingLight(P(-3f, y + 3.4f, 31f));
            kit.CeilingLight(P(3f, y + 3.4f, 31f), 0f, Lilas);
        }

        // ------------------------------------------------------------------ 5. truque: passarela falsa (T0)

        static void PassarelaFalsa(PhaseKit kit)
        {
            // Liga a triagem direto à sala final e a porta aparece no fim dela, mas é piso falso e não tem luz.
            // 14 m de comprimento: depois do primeiro contato sobram 0,8 s; o máximo alcançável é ~46,5 m, e a
            // sala final começa em z = 50, 0,8 m mais alta (contas em docs/Fase5.md).
            kit.CrumblingPlatform("Passarela falsa", P(0f, YTriagem, 42f), new Vector2(2f, 14f), 0.8f); // x[-1,1] z[35,49]
            // estantes altas que sobem do abismo: dos dois lados, impedem pular da passarela para a ponte do escudo
            Estante(kit, "Passarela oeste", -1.3f, 35f, -1.3f, 49f, -2f, 7f);
            Estante(kit, "Passarela leste", 1.3f, 35f, 1.3f, 49f, -2f, 7f);
        }

        // ------------------------------------------------------------------ 6-9. rota segura, longa (A1..A5)

        static void RotaSegura(PhaseKit kit)
        {
            // 6. corredor oeste (y 1,5) e a descida de 0,9 m para a galeria
            kit.Platform("Corredor oeste", P(-7.5f, YTriagem, 31.5f), new Vector2(3f, 3f)); // x[-9,-6] z[30,33]
            Estante(kit, "Corredor oeste sul", -6.9f, 29.7f, -9f, 29.7f, YTriagem);
            Estante(kit, "Corredor oeste norte", -6.9f, 33.3f, -9f, 33.3f, YTriagem);
            kit.CeilingLight(P(-7.5f, YTriagem + 3.4f, 31.5f), 0f, Lilas);

            // 7. galeria dos lasers lentos: faces internas em -13,35 e -10,65 (2,7 m, como no corredor de calibração)
            kit.Platform("Galeria de lasers", P(-12f, YGaleria, 39.25f), new Vector2(3f, 18.5f)); // x[-13,5;-10,5] z[30;48,5]
            Estante(kit, "Galeria sul", -13.95f, 29.7f, -10.05f, 29.7f, YGaleria);
            Estante(kit, "Galeria oeste", -13.65f, 29.7f, -13.65f, 58.3f, YGaleria);    // segue pela sala da centrífuga
            Estante(kit, "Galeria leste", -10.35f, 33.3f, -10.35f, 48.5f, YGaleria);   // a abertura z[30;33,3] recebe o corredor
            kit.Laser("Laser lento 1", P(-12f, YGaleria, 37f), 0f, LentoLigado, LentoDesligado, Lento1Offset);
            kit.Laser("Laser lento 2", P(-12f, YGaleria, 40f), 0f, LentoLigado, LentoDesligado, Lento2Offset);
            kit.Sign("LASERS EM ONDA: CORRA NA JANELA", P(-12f, YGaleria + 3.2f, 33.5f), 0f, 0.04f);
            kit.ReadingLamp(P(-13.65f, YGaleria + 3.5f, 35f), 90f);
            kit.ReadingLamp(P(-10.35f, YGaleria + 3.5f, 44f), -90f);
            kit.CeilingLight(P(-12f, YGaleria + 3.4f, 45f), 90f, Lilas);

            // 8. sala da centrífuga (y 1,4): serra que varre a sala de oeste a leste em z = 51,5
            kit.Platform("Sala da centrifuga", P(-10f, YCentrifuga, 53.5f), new Vector2(7f, 9f)); // x[-13,5;-6,5] z[49,58]
            Estante(kit, "Centrifuga sul", -10.05f, 48.7f, -6.2f, 48.7f, YCentrifuga);
            Estante(kit, "Centrifuga norte", -13.95f, 58.3f, -6.2f, 58.3f, YCentrifuga);
            Estante(kit, "Centrifuga leste A", -6.2f, 48.7f, -6.2f, 52.7f, YCentrifuga);   // portal z[53,55]
            Estante(kit, "Centrifuga leste B", -6.2f, 55.3f, -6.2f, 58.3f, YCentrifuga);
            // yaw 0: a lâmina gira no plano XY e rola ao longo de X; o deslocamento é em X puro
            kit.Saw("Serra da centrifuga", P(-12.5f, YCentrifuga, 51.5f), new Vector3(5.5f, 0f, 0f), 4f, 0f);
            kit.ReadingDesk("Centrifuga mesa", P(-12.4f, YCentrifuga, 57.3f), 90f);
            kit.ReadingLamp(P(-7f, YCentrifuga, 57.5f), -135f);
            kit.CeilingLight(P(-10f, YCentrifuga + 3.4f, 55f));

            // 9. passagem de amostras: 2 m entre estantes, coberta por espinhos em ciclo; depois, degrau de 0,9 m
            kit.Platform("Passagem de amostras", P(-4.75f, YCentrifuga, 54f), new Vector2(3.5f, 2f)); // x[-6,5;-3] z[53,55]
            Estante(kit, "Passagem sul", -6.2f, 52.7f, -3.6f, 52.7f, YCentrifuga);
            Estante(kit, "Passagem norte", -6.2f, 55.3f, -3.6f, 55.3f, YCentrifuga);
            kit.Spikes("Espinhos da passagem", P(-5.3f, YCentrifuga, 54f), true, 1.0f, 2.0f, 0f);
            kit.ShelfArch("Chegada oeste", P(-3f, YCentrifuga, 54f), 2f, 3.6f, 90f);
        }

        // ------------------------------------------------------------------ 10-11. rota curta, arriscada (B1..B4)

        static void RotaCurta(PhaseKit kit)
        {
            // ponte de livros de 2,4 m sobre o vazio, sem parede do lado leste: o laser (2,7 m) cobre a largura toda
            kit.BookPlatform("Ponte do escudo", P(4.65f, YTriagem, 38.5f), new Vector2(2.4f, 7f), 0f, 1.6f); // x[3,45;5,85] z[35,42]
            kit.Platform("Ponte da barreira", P(4.65f, YTriagem, 45f), new Vector2(2.4f, 6f));              // z[42,48]

            // 10. o escudo fica no caminho, 7,5 m antes do laser que ele resolve
            kit.Pickup(PickupKind.Shield, P(4.65f, YTriagem, 37.5f));
            kit.Sign("ESCUDO = 1 TOQUE", P(4.65f, YTriagem + 3.2f, 39.5f), 0f, 0.05f);

            // 11. barreira: o laser mais difícil (aberto só 0,8 s a cada 5 s)
            kit.Laser("Laser da barreira", P(4.65f, YTriagem, 45f), 0f, BarreiraLigado, BarreiraDesligado, BarreiraOffset);

            // vão de 2 m, subindo 0,4 m, até o patamar leste; dali se entra na sala final pelo portal leste
            kit.BookPlatform("Patamar leste", P(4.5f, YLeste, 53.5f), new Vector2(3f, 7f), 0f, 2f); // x[3,6] z[50,57]
            kit.ShelfArch("Chegada leste", P(3f, YLeste, 54f), 2f, 3.6f, 90f);

            kit.CeilingLight(P(4.65f, YTriagem + 3.6f, 38f), 90f, Lilas);
            kit.CeilingLight(P(4.65f, YTriagem + 3.6f, 46.5f), 90f);
            kit.CeilingLight(P(4.5f, YLeste + 3.6f, 51.5f), 90f, Lilas);
            // páginas soltas à leste da ponte, longe do caminho
            kit.FloatingPages("Vazio da ponte", P(9.5f, YTriagem + 1f, 44f), 1.8f, 10);
        }

        // ------------------------------------------------------------------ 12. sala final (G1, G)

        static void SalaFinal(PhaseKit kit)
        {
            float y = YFinal;
            // o livro aberto gigante é o marco da fase: visto da triagem pela passarela, com a porta em cima
            kit.OpenBookPlatform("Livro aberto da conclusao", P(0f, y, 54f), new Vector2(6f, 8f), 0f); // x[-3,3] z[50,58]
            Estante(kit, "Final sul O", -3.3f, 49.7f, -1f, 49.7f, y);   // janela x[-1,1]: fim da passarela falsa
            Estante(kit, "Final sul L", 1f, 49.7f, 3.3f, 49.7f, y);
            Estante(kit, "Final norte", -3.3f, 58.3f, 3.3f, 58.3f, y);
            Estante(kit, "Final oeste A", -3f, 49.7f, -3f, 52.4f, y);   // portal z[53,55]: chegada da rota segura
            Estante(kit, "Final oeste B", -3f, 55.6f, -3f, 58.3f, y);
            Estante(kit, "Final leste A", 3f, 49.7f, 3f, 52.4f, y);     // portal z[53,55]: chegada da rota curta
            Estante(kit, "Final leste B", 3f, 55.6f, 3f, 58.3f, y);

            kit.Goal(P(0f, y, 56.5f), 0f);

            kit.ReadingLamp(P(-2.4f, y, 57.5f), 180f);
            kit.ReadingLamp(P(2.4f, y, 57.5f), 180f);
            kit.CeilingLight(P(0f, y + 3.6f, 53f), 0f, Rosa);
            kit.FloatingPages("Sobre a porta", P(0f, y + 5.5f, 54f), 2.5f, 12);
        }

        // ------------------------------------------------------------------ marcos no horizonte

        static void Horizonte(PhaseKit kit)
        {
            // relógio parado atrás da porta: visível por cima das estantes desde a recepção
            kit.DreamClock("Laboratorio", P(0f, 11f, 63f), 3f, 0f, 3f);
            kit.FloatingBooks("Alto da triagem", P(-9f, 9f, 24f), 3f, 7);
            kit.FloatingBooks("Alto da galeria", P(-17f, 7f, 44f), 2f, 5);
            kit.DreamMotes("Vapor alquimico", P(0f, -2.5f, 21f), new Vector3(5f, 3f, 10f), Rosa, 120); // no fosso das bancadas
        }

        // ------------------------------------------------------------------ utilitários

        static Vector3 P(float x, float y, float z) => new Vector3(x, y, z);

        /// <summary>Estante sólida (parede de 0,6 m) de (x1, z1) até (x2, z2), com base em <paramref name="y"/>.</summary>
        static void Estante(PhaseKit kit, string nome, float x1, float z1, float x2, float z2, float y, float altura = 3.5f) =>
            kit.Bookshelf(nome, new Vector3(x1, y, z1), new Vector3(x2, y, z2), altura);
    }
}
