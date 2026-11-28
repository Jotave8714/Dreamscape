using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 15 — "Mar de Páginas": ilhas de livros sobre um mar de páginas brilhando, de um cais até um farol.
    /// Ideia central: os vãos são grandes demais para o pulo comum, e cada poder serve a um tipo de vão.
    /// A Velocidade vence vãos longos em descida (5 m, -0,5 m); a Pluma vence vãos em subida (+1,5 m), que nenhum
    /// outro poder alcança.
    ///
    /// Sequência (+Z):
    /// 1. Cais e pedras de livro, com páginas que cedem (apresenta).
    /// 2. Correnteza: três saltos com Velocidade, descendo.
    /// 3. Ilha das correntes, onde há duas rotas até a ilha alta: a ponte dos feixes (curta, difícil, com o Escudo
    ///    como rede de segurança) e a escada de páginas (longa, sem poder: pêndulo numa ponte estreita, espinhos e
    ///    página que cede).
    /// 4. Voo da Pluma: três saltos subindo, com pêndulos e uma nuvem de páginas que cede, e o salto final até o farol.
    /// Medidas, grafo e contas em docs/Fase15.md.
    /// </summary>
    public sealed class Fase15Builder : IPhaseBuilder
    {
        public int Order => 15;
        public string SceneName => "Fase15_MarDePaginas";
        public PhaseTheme Theme => Tema;

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        // a noite clareia: azul-marinho com névoa de mar e destaque turquesa
        static readonly PhaseTheme Tema = new PhaseTheme
        {
            Name = "Fase15", Floor = C(0.40f, 0.55f, 0.62f), Wall = C(0.20f, 0.26f, 0.36f), Accent = C(0.40f, 0.95f, 0.95f),
            Sky = C(0.08f, 0.14f, 0.24f), Equator = C(0.16f, 0.24f, 0.34f), Ground = C(0.05f, 0.07f, 0.11f),
            Fog = C(0.12f, 0.20f, 0.30f), FogDensity = 0.018f, Sun = C(0.80f, 0.90f, 1f), SunIntensity = 1.15f,
            SunEuler = new Vector3(42f, -30f, 0f), Bloom = 0.95f, Vignette = 0.28f, Saturation = 20f, PostExposure = 0.15f,
            Tint = C(0.95f, 1f, 1f)
        };

        static readonly Color LuzMar = new Color(0.55f, 0.95f, 1f);
        static readonly Color LuzQuente = new Color(1f, 0.84f, 0.58f);
        static readonly Color LuzVelocidade = new Color(1f, 0.85f, 0.35f);
        static readonly Color LuzPluma = new Color(0.78f, 0.65f, 1f);

        public void Build(PhaseKit kit)
        {
            Cais(kit);
            PedrasDeLivro(kit);
            Correnteza(kit);
            IlhaDasCorrentes(kit);
            PonteDosFeixes(kit);
            EscadaDePaginas(kit);
            VooDaPluma(kit);
            Farol(kit);
            Mar(kit);

            kit.FallZone(-9f);
            kit.LibraryBackdrop(new Vector2(-10f, -3f), new Vector2(6f, 127f), -10f, 24f, 8f);
        }

        // ------------------------------------------------------------------ P0: cais (x -4..4, z -3..3)

        void Cais(PhaseKit kit)
        {
            kit.OpenBookPlatform("P0 Cais", new Vector3(0f, 0f, 0f), new Vector2(8f, 6f));
            kit.Spawn(new Vector3(0f, 0f, -1f), 0f);
            kit.Bookshelf("Cais fundo", new Vector3(-4f, 0f, -3.3f), new Vector3(4f, 0f, -3.3f), 4f);
            kit.ReadingDesk("Mesa do cais", new Vector3(-2.8f, 0f, -1.8f), 0f);
            kit.ReadingLamp(new Vector3(-3.3f, 0.8f, -1.8f), 0f);
            kit.Pole(new Vector3(3.3f, 0f, 2.3f), LuzMar);
            kit.Sign("FASE 15", new Vector3(0f, 3.6f, 3.2f), 0f, 0.09f);
            kit.Sign("MAR DE PAGINAS", new Vector3(0f, 2.95f, 3.2f), 0f, 0.075f);
        }

        // ------------------------------------------------------------------ P1: pedras de livro (z 4,5..27,5)

        void PedrasDeLivro(PhaseKit kit)
        {
            kit.BookPlatform("B1 Pedra de livro", new Vector3(0f, 0f, 6f), new Vector2(3f, 3f));        // z 4,5..7,5
            kit.BookPlatform("B2 Pedra de livro", new Vector3(1.5f, 0.5f, 11f), new Vector2(3f, 3f));   // z 9,5..12,5
            kit.CrumblingPlatform("B3 Pagina que cede", new Vector3(1.5f, 0.5f, 15f), new Vector2(2.5f, 2.5f), 0.8f);
            kit.CrumblingPlatform("B4 Pagina que cede", new Vector3(0f, 0.5f, 19f), new Vector2(2.5f, 2.5f), 0.8f);
            // B5: pista de 6 m para ganhar velocidade antes do primeiro salto
            kit.BookPlatform("B5 Ilha da Velocidade", new Vector3(0f, 1f, 24.5f), new Vector2(4f, 6f));  // z 21,5..27,5
            kit.Pickup(PickupKind.Speed, new Vector3(0f, 1f, 23f));
            kit.Carpet(new Vector3(0f, 1f, 25.2f), new Vector2(1.4f, 4.4f));

            kit.CeilingLight(new Vector3(0f, 5f, 6f), 0f, LuzMar);
            kit.CeilingLight(new Vector3(1.5f, 5.5f, 11f), 0f, LuzMar);
            kit.CeilingLight(new Vector3(0.7f, 5.5f, 17f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, 6f, 24.5f), 0f, LuzVelocidade);
            kit.Sign("CORRA E SALTE", new Vector3(0f, 3.6f, 27.3f), 0f, 0.06f);
        }

        // ------------------------------------------------------------------ P2: correnteza (Velocidade, z 32,5..56,5)

        void Correnteza(PhaseKit kit)
        {
            // vãos de 5 m, cada um 0,5 m abaixo: Velocidade alcança 5,53 m; o pulo comum, no máximo 4,85 m
            kit.BookPlatform("S1 Ilha da correnteza", new Vector3(0f, 0.5f, 34.5f), new Vector2(3.5f, 4f));   // z 32,5..36,5
            kit.BookPlatform("S2 Ilha da correnteza", new Vector3(0f, 0f, 43.5f), new Vector2(3.5f, 4f));     // z 41,5..45,5
            kit.BookPlatform("S3 Ilha da correnteza", new Vector3(0f, -0.5f, 53.5f), new Vector2(3.5f, 6f));  // z 50,5..56,5
            kit.CeilingLight(new Vector3(0f, 6f, 34.5f), 0f, LuzVelocidade);
            kit.CeilingLight(new Vector3(0f, 5.5f, 43.5f), 0f, LuzVelocidade);
            kit.CeilingLight(new Vector3(0f, 5f, 53.5f), 0f, LuzQuente);
        }

        // ------------------------------------------------------------------ H: ilha das correntes (x -4..4, z 58..66)

        void IlhaDasCorrentes(PhaseKit kit)
        {
            kit.OpenBookPlatform("H Ilha das correntes", new Vector3(0f, -0.5f, 62f), new Vector2(8f, 8f));
            kit.Pickup(PickupKind.Shield, new Vector3(2.5f, -0.5f, 62.5f));
            kit.ReadingDesk("Mesa da ilha", new Vector3(-1.5f, -0.5f, 59.3f), 0f);
            kit.ReadingLamp(new Vector3(-2f, 0.3f, 59.3f), 0f);
            kit.CeilingLight(new Vector3(0f, 5f, 62f), 0f, LuzQuente);
            kit.Sign("PONTE DOS FEIXES", new Vector3(2.5f, 2.3f, 65.8f), 0f, 0.045f);
            kit.Sign("ESCADA DE PAGINAS", new Vector3(-3.8f, 2.3f, 63f), -90f, 0.045f);
        }

        // ------------------------------------------------------------------ rota leste: ponte dos feixes (x 1,3..3,7)

        void PonteDosFeixes(PhaseKit kit)
        {
            const float x = 2.5f;
            // tique de 2,8 s: laser 1,2 ligado / 1,6 apagado; espinhos 1,4 fora / 1,4 dentro. Offsets em onda a 5 m/s
            kit.BookPlatform("R1 Ponte dos feixes", new Vector3(x, 0.25f, 68.5f), new Vector2(2.4f, 5f));   // z 66..71
            Feixe(kit, "R1", new Vector3(x, 0.25f, 69f), 0f);

            kit.BookPlatform("R2 Ponte dos espinhos", new Vector3(x, 1f, 74f), new Vector2(2.4f, 6f));      // z 71..77
            // dois campos lado a lado (x 1,9 e 3,1) cobrem x 1,0..4,0, além da borda da ponte; duas fileiras = 4 m
            // as duas fileiras formam um bloco só, centrado 5,5 m depois do primeiro laser
            float ondaEspinhos = Mathf.Repeat(-5.5f / 5f, 2.8f);
            foreach (float z in new[] { 73.5f, 75.5f })
            {
                kit.Spikes($"R2 espinhos {z} oeste", new Vector3(x - 0.6f, 1f, z), true, 1.4f, 1.4f, ondaEspinhos);
                kit.Spikes($"R2 espinhos {z} leste", new Vector3(x + 0.6f, 1f, z), true, 1.4f, 1.4f, ondaEspinhos);
            }

            kit.BookPlatform("R3 Ponte final", new Vector3(x, 1.75f, 78.5f), new Vector2(2.4f, 3f));         // z 77..80
            Feixe(kit, "R3", new Vector3(x, 1.75f, 78.5f), Mathf.Repeat(-9.5f / 5f, 2.8f));

            kit.CeilingLight(new Vector3(x, 6f, 71.5f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(x, 6.5f, 76.5f), 0f, LuzMar);
        }

        // laser entre duas estantes de 4 m (x 1,0 e 4,0): o feixe de 2,7 m cobre a ponte inteira entre elas
        void Feixe(PhaseKit kit, string nome, Vector3 centro, float offset)
        {
            kit.Laser($"{nome} feixe", centro, 0f, 1.2f, 1.6f, offset);
            kit.Bookshelf($"{nome} estante oeste", centro + new Vector3(-1.5f, 0f, -2f), centro + new Vector3(-1.5f, 0f, 2f), 3f);
            kit.Bookshelf($"{nome} estante leste", centro + new Vector3(1.5f, 0f, -2f), centro + new Vector3(1.5f, 0f, 2f), 3f);
        }

        // ------------------------------------------------------------------ rota oeste: escada de páginas (x -9..-4)

        void EscadaDePaginas(PhaseKit kit)
        {
            kit.BookPlatform("E1 Degrau de livros", new Vector3(-7f, 0.25f, 63f), new Vector2(3f, 4f));       // x -8,5..-5,5, z 61..65
            // ponte de 2 m com pêndulo de 70° e período de 3 s
            kit.BookPlatform("E2 Ponte do pendulo", new Vector3(-7f, 1f, 70.5f), new Vector2(2f, 8f));       // z 66,5..74,5
            kit.Pendulum("E2 Pendulo", new Vector3(-7f, 1f, 70.5f), 70f, 3f, 0f, 0f);
            kit.Platform("E2 Viga do pendulo", new Vector3(-7f, 5.3f, 70.5f), new Vector2(7.6f, 0.6f));
            kit.Pillar("E2 Coluna oeste", new Vector3(-10.8f, -8f, 70.5f), 12.8f);
            kit.Pillar("E2 Coluna leste", new Vector3(-3.2f, -8f, 70.5f), 12.8f);

            // 3,6 m de largura: os campos (x -7,9 e -6,1) cobrem x -8,8..-5,2, e o corpo não passa pela borda
            kit.BookPlatform("E3 Ilha dos espinhos", new Vector3(-7f, 1f, 80f), new Vector2(3.6f, 8f));      // z 76..84
            foreach (float z in new[] { 79f, 81f })
            {
                kit.Spikes($"E3 espinhos {z} oeste", new Vector3(-7.9f, 1f, z), true, 1.2f, 1.8f, 0f);
                kit.Spikes($"E3 espinhos {z} leste", new Vector3(-6.1f, 1f, z), true, 1.2f, 1.8f, 0f);
            }

            kit.CrumblingPlatform("E4 Pagina que cede", new Vector3(-7f, 1.75f, 86.25f), new Vector2(2.5f, 2.5f), 0.8f); // z 85..87,5

            kit.CeilingLight(new Vector3(-7f, 5.5f, 63f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(-7f, 6f, 77f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(-7f, 6f, 83f), 0f, LuzQuente);
        }

        // ------------------------------------------------------------------ P4: voo da Pluma (A y 2,5 → G3 y 7,0)

        void VooDaPluma(PhaseKit kit)
        {
            kit.OpenBookPlatform("A Ilha alta", new Vector3(0f, 2.5f, 84f), new Vector2(8f, 8f));            // x -4..4, z 80..88
            kit.Pickup(PickupKind.Gravity, new Vector3(-1.5f, 2.5f, 86.5f));
            kit.CeilingLight(new Vector3(-1.5f, 8f, 86f), 0f, LuzPluma);
            kit.Sign("SO A PLUMA ALCANCA O FAROL", new Vector3(0f, 5.4f, 87.8f), 0f, 0.045f);

            // vãos de 4 m subindo 1,5 m: a Pluma alcança 5,19 m; sem ela, a borda fica acima de 1,4 m
            kit.BookPlatform("G1 Ilha do pendulo", new Vector3(0f, 4f, 94.5f), new Vector2(3.5f, 5f));        // z 92..97
            kit.Pendulum("G1 Pendulo", new Vector3(0f, 4f, 94.5f), 55f, 2.4f, 0f, 0f);
            kit.Platform("G1 Viga", new Vector3(0f, 8.3f, 94.5f), new Vector2(6f, 0.6f));

            kit.CrumblingPlatform("G2 Nuvem de paginas", new Vector3(0f, 5.5f, 102.25f), new Vector2(3f, 2.5f), 0.8f); // z 101..103,5

            kit.BookPlatform("G3 Ilha do pendulo alto", new Vector3(0f, 7f, 110.5f), new Vector2(3.5f, 6f));  // z 107,5..113,5
            kit.Pendulum("G3 Pendulo", new Vector3(0f, 7f, 110.5f), 55f, 2.4f, 1.2f, 0f);
            kit.Platform("G3 Viga", new Vector3(0f, 11.3f, 110.5f), new Vector2(6f, 0.6f));

            kit.CeilingLight(new Vector3(0f, 10f, 92.8f), 0f, LuzPluma);
            kit.CeilingLight(new Vector3(0f, 11f, 102.25f), 0f, LuzPluma);
            kit.CeilingLight(new Vector3(0f, 13f, 108.3f), 0f, LuzPluma);
        }

        // ------------------------------------------------------------------ farol (x -4..4, z 119..127, y 7,0)

        void Farol(PhaseKit kit)
        {
            kit.OpenBookPlatform("Farol", new Vector3(0f, 7f, 123f), new Vector2(8f, 8f));
            kit.Goal(new Vector3(0f, 7f, 125f), 180f);
            kit.Bookshelf("Torre do farol", new Vector3(-4f, 7f, 127.3f), new Vector3(4f, 7f, 127.3f), 9f);
            kit.ReadingLamp(new Vector3(0f, 16f, 127.3f), 180f, LuzQuente);
            kit.Pole(new Vector3(-3.3f, 7f, 119.7f), LuzQuente);
            kit.Pole(new Vector3(3.3f, 7f, 119.7f), LuzQuente);
            kit.Carpet(new Vector3(0f, 7f, 122.5f), new Vector2(2f, 5f));
            // marco visível desde o cais: relógio sobre a torre do farol
            kit.DreamClock("Relogio do farol", new Vector3(0f, 20f, 128f), 3f, 0f, 3f);
        }

        // ------------------------------------------------------------------ o mar de páginas e o horizonte

        void Mar(PhaseKit kit)
        {
            // o mar: páginas brilhando 4 a 5 m abaixo das ilhas, de ponta a ponta
            for (int k = 0; k < 11; k++)
            {
                float z = 4f + k * 12f;
                kit.FloatingPages($"Mar {k + 1} oeste", new Vector3(-5f, -5f, z), 6f, 22);
                kit.FloatingPages($"Mar {k + 1} leste", new Vector3(6f, -5.5f, z + 6f), 6f, 22);
            }
            kit.DreamMotes("Espuma do mar", new Vector3(0f, -5f, 62f), new Vector3(36f, 3f, 140f), LuzMar, 520);

            // livros nadando ao longe, fora do caminho
            kit.FloatingBooks("Cardume oeste", new Vector3(-14f, -1f, 40f), 3.5f, 9);
            kit.FloatingBooks("Cardume leste", new Vector3(10f, 0f, 70f), 3.5f, 9);
            kit.FloatingBooks("Cardume alto", new Vector3(-12f, 9f, 104f), 3f, 7);
            kit.DreamClock("Relogio do horizonte oeste", new Vector3(-16f, 9f, 60f), 2.6f, -90f, 2f);
            kit.DreamClock("Relogio do horizonte leste", new Vector3(12f, 12f, 100f), 2.4f, 90f, 2f);
        }
    }
}
