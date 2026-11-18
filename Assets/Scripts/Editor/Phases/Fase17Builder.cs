using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 17 — "Despertar": o amanhecer de verdade e a última fase do jogo. A biblioteca do sonho se enche da luz da
    /// manhã, e o caminho é um corredor de lembranças: cada trecho revisita a ideia de uma fase anterior (3 a 16), em ordem
    /// crescente de dificuldade, até o livro aberto onde o estudante adormeceu, com a porta de saída do sonho.
    ///
    /// Atos (+Z):
    ///   I   (y 0)        Sala das lembranças: espinhos fixos para desviar (F3), estantes em zigue-zague lidas pelas
    ///                    luzes (F9), um piso escuro que cede sobre um fosso raso (F3: onde há luz, há chão) e mesas
    ///                    como degraus até a plataforma de 1,6 m (F8).
    ///   II  (y 1,6)      Velocidade (F7): três carteiras que cedem (F4) e o vão de 4,4 m (F7).
    ///   III (y 1,6)      Laboratório: Escudo opcional (F5) e corredor de lasers, dois em onda (F13) e um com outro ritmo (F5).
    ///   IV  (y 1,6)      Oficina e arquivo: serra que vai e volta, passe pelo lado vazio (F6); três pêndulos
    ///                    sincronizados (F14); prensas em onda (F12) e uma página que cede (F12).
    ///   V   (y 2,1–3,6)  Ilhas de leitura com mesas (F11); Pluma e o mar de páginas, vãos de 5,2 m (F15).
    ///   VI  (y 3,6–8,7)  Pulo Duplo: escadaria de livros de +1,7 m (F10) e a serra saltada no trilho (F16); livro do despertar.
    /// Cada tipo de poder aparece uma vez só. Medidas, grafo e contas em docs/Fase17.md.
    /// </summary>
    public sealed class Fase17Builder : IPhaseBuilder
    {
        public int Order => 17;
        public string SceneName => "Fase17_Despertar";
        public PhaseTheme Theme => Tema;

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        // o amanhecer: a fase mais clara e colorida do jogo (Fase 16: saturação 25, exposição 0,2), neblina clara e rala,
        // sol quente e baixo à frente do jogador; destaque pêssego de aurora
        static readonly PhaseTheme Tema = new PhaseTheme
        {
            Name = "Fase17", Floor = C(0.82f, 0.66f, 0.55f), Wall = C(0.52f, 0.40f, 0.44f), Accent = C(1f, 0.66f, 0.55f),
            Sky = C(0.78f, 0.84f, 0.96f), Equator = C(0.94f, 0.82f, 0.74f), Ground = C(0.32f, 0.25f, 0.22f),
            Fog = C(0.98f, 0.88f, 0.80f), FogDensity = 0.01f, Sun = C(1f, 0.86f, 0.66f), SunIntensity = 1.7f,
            SunEuler = new Vector3(24f, 200f, 0f), Bloom = 1.1f, Vignette = 0.15f, Saturation = 30f, PostExposure = 0.3f,
            Tint = C(1f, 0.97f, 0.93f)
        };

        static readonly Color LuzManha = new Color(1f, 0.90f, 0.72f);    // piso firme
        static readonly Color LuzQuente = new Color(1f, 0.78f, 0.50f);   // refúgio
        static readonly Color LuzPerigo = new Color(1f, 0.55f, 0.50f);   // sobre armadilhas
        static readonly Color LuzPluma = new Color(0.80f, 0.66f, 1f);
        static readonly Color LuzPulo = new Color(0.50f, 0.85f, 1f);

        // alturas do topo dos pisos
        const float YOficina = 1.6f;   // atos II a IV
        const float YCais = 3.1f;
        const float YMar = 3.6f;
        const float YFim = 8.7f;

        // offset de uma armadilha para que um corredor a 5 m/s encontre a mesma fase do ciclo em todas
        static float Onda(float distancia, float periodo) => Mathf.Repeat(-distancia / 5f, periodo);

        public void Build(PhaseKit kit)
        {
            SalaDasLembrancas(kit);
            CorredorDaPressa(kit);
            Laboratorio(kit);
            OficinaEArquivo(kit);
            IlhasDeLeitura(kit);
            MarDePaginas(kit);
            EscadariaEDespertar(kit);
            Manha(kit);

            kit.FallZone(-8f);
            kit.LibraryBackdrop(new Vector2(-6f, -4f), new Vector2(6f, 177f), -8f, 20f, 9f);
        }

        // ------------------------------------------------------------------ ato I: sala das lembranças (z -3..23)

        void SalaDasLembrancas(PhaseKit kit)
        {
            // entrada
            kit.Platform("R0 Entrada", new Vector3(0f, 0f, 0f), new Vector2(8f, 6f));
            kit.Spawn(new Vector3(0f, 0f, -1f), 0f);
            kit.Carpet(new Vector3(0f, 0f, 0f), new Vector2(2.2f, 4.4f));
            kit.Bookshelf("Sala fundo", new Vector3(-4.3f, 0f, -3.3f), new Vector3(4.3f, 0f, -3.3f), 4f);
            kit.Bookshelf("Sala oeste", new Vector3(-4.3f, 0f, -3f), new Vector3(-4.3f, 0f, 18f), 4f);
            kit.Bookshelf("Sala leste", new Vector3(4.3f, 0f, -3f), new Vector3(4.3f, 0f, 18f), 4f);
            kit.ShelfArch("Portal do despertar", new Vector3(0f, 0f, 2.4f), 2.4f, 3.4f, 0f);
            kit.CeilingLight(new Vector3(0f, 4f, -0.5f), 0f, LuzQuente);
            kit.Sign("FASE 17", new Vector3(0f, 5.2f, 2.4f), 0f, 0.09f);
            kit.Sign("DESPERTAR", new Vector3(0f, 4.6f, 2.4f), 0f, 0.075f);
            kit.Sign("LEMBRE DE TUDO O QUE APRENDEU", new Vector3(0f, 4.1f, 2.4f), 0f, 0.035f);

            // cela 1 (F3): espinhos fixos; a passagem livre é a da direita, de frente para a abertura
            kit.Platform("Cela 1", new Vector3(0f, 0f, 5f), new Vector2(8f, 4f));
            kit.Spikes("Lembranca espinhos 1", new Vector3(-2f, 0f, 5f));
            kit.Spikes("Lembranca espinhos 2", new Vector3(0f, 0f, 5f));
            kit.CeilingLight(new Vector3(2.4f, 4f, 5f), 0f, LuzManha);
            kit.Bookshelf("Divisoria 1 oeste", new Vector3(-4f, 0f, 7f), new Vector3(1.6f, 0f, 7f), 4f);
            kit.Bookshelf("Divisoria 1 leste", new Vector3(3.2f, 0f, 7f), new Vector3(4f, 0f, 7f), 4f);

            // cela 2 (F3 + F9): a linha reta entre as aberturas cruza o piso escuro; o chão iluminado contorna
            kit.Platform("Cela 2 sul", new Vector3(0f, 0f, 7.75f), new Vector2(8f, 1.5f));
            kit.Platform("Cela 2 norte", new Vector3(0f, 0f, 11.25f), new Vector2(8f, 1.5f));
            kit.Platform("Cela 2 oeste", new Vector3(-2.5f, 0f, 9.5f), new Vector2(3f, 2f));
            kit.Platform("Cela 2 leste", new Vector3(2.5f, 0f, 9.5f), new Vector2(3f, 2f));
            kit.CrumblingPlatform("Piso escuro", new Vector3(0f, 0f, 9.5f), new Vector2(2f, 2f), 0.8f);
            // fosso raso e fechado (0,8 m): as lajes vizinhas deixam só 0,3 m por baixo
            kit.Platform("Fosso do piso escuro", new Vector3(0f, -0.8f, 9.5f), new Vector2(2f, 2f));
            kit.CeilingLight(new Vector3(2.5f, 4f, 9.5f), 0f, LuzManha);
            kit.CeilingLight(new Vector3(-2.5f, 4f, 9.5f), 0f, LuzManha);
            kit.CeilingLight(new Vector3(0f, 4f, 7.9f), 90f, LuzManha);
            kit.CeilingLight(new Vector3(0f, 4f, 11.1f), 90f, LuzManha);
            kit.Bookshelf("Divisoria 2 oeste", new Vector3(-4f, 0f, 12f), new Vector3(-3.2f, 0f, 12f), 4f);
            kit.Bookshelf("Divisoria 2 leste", new Vector3(-1.6f, 0f, 12f), new Vector3(4f, 0f, 12f), 4f);
            kit.Sign("ONDE HA LUZ, HA CHAO", new Vector3(1.4f, 2.7f, 11.6f), 0f, 0.04f);

            // cela 3 (F8): as mesas são os degraus até a plataforma de 1,6 m (borda alcançável sem poder: 1,4 m)
            kit.Platform("Cela 3 Sala das mesas", new Vector3(0f, 0f, 15f), new Vector2(8f, 6f));
            kit.ReadingDesk("Mesa degrau oeste", new Vector3(-2.5f, 0f, 16.6f), 0f);
            kit.ReadingDesk("Mesa degrau leste", new Vector3(2.5f, 0f, 16.6f), 0f);
            kit.ReadingLamp(new Vector3(-3.05f, 0.8f, 16.6f), 0f);
            kit.ReadingLamp(new Vector3(3.05f, 0.8f, 16.6f), 0f);
            kit.CeilingLight(new Vector3(0f, 4f, 14.5f), 0f, LuzManha);
            kit.Sign("AS MESAS SAO DEGRAUS", new Vector3(0f, 4.6f, 17.9f), 0f, 0.04f);

            // plataforma da Velocidade (y 1,6)
            kit.BookPlatform("L Plataforma da pressa", new Vector3(0f, YOficina, 20.5f), new Vector2(8f, 5f), 0f, 1.6f);
            kit.Bookshelf("Pressa oeste", new Vector3(-4.3f, YOficina, 18f), new Vector3(-4.3f, YOficina, 23f), 3f);
            kit.Bookshelf("Pressa leste", new Vector3(4.3f, YOficina, 18f), new Vector3(4.3f, YOficina, 23f), 3f);
            kit.Carpet(new Vector3(0f, YOficina, 20.5f), new Vector2(2.2f, 4.4f));
        }

        // ------------------------------------------------------------------ ato II: corredor da pressa (z 23..50,1)

        void CorredorDaPressa(PhaseKit kit)
        {
            kit.Pickup(PickupKind.Speed, new Vector3(0f, YOficina, 21.6f));
            kit.CeilingLight(new Vector3(0f, YOficina + 4.4f, 21.6f), 0f, LuzQuente);
            kit.Sign("CORRA: AS CARTEIRAS CEDEM", new Vector3(0f, YOficina + 4.2f, 23.1f), 0f, 0.04f);

            // F4: carteiras de 2,5 m que cedem, vãos de 1,8 m (pular uma exige 6,1 m: nem com a Velocidade)
            float[] zs = { 26.05f, 30.35f, 34.65f };
            for (int k = 0; k < zs.Length; k++)
                kit.CrumblingPlatform($"C{k + 1} Carteira", new Vector3(0f, YOficina, zs[k]), new Vector2(2.5f, 2.5f), 0.8f);

            // F7: lançamento e vão de 4,4 m (sem poder: limite 4,16 m; com a Velocidade: 4,97 m)
            kit.BookPlatform("Lp Lancamento", new Vector3(0f, YOficina, 39.2f), new Vector2(3f, 3f), 0f, 2f);
            kit.CeilingLight(new Vector3(0f, YOficina + 4.4f, 39.2f), 0f, LuzManha);
            kit.Sign("4,4 M: SO COM PRESSA", new Vector3(-3.4f, YOficina + 2.2f, 40.5f), 0f, 0.04f);
            kit.FloatingPages("Sob o vao da pressa", new Vector3(0f, -1.5f, 42.9f), 2.5f, 12);
        }

        // ------------------------------------------------------------------ ato III: laboratório (z 45,1..63,1)

        void Laboratorio(PhaseKit kit)
        {
            kit.Platform("V Bancada do laboratorio", new Vector3(0f, YOficina, 47.6f), new Vector2(6f, 5f));
            kit.CeilingLight(new Vector3(0f, YOficina + 4.4f, 47f), 0f, LuzQuente);
            // F5: Escudo opcional, fora da linha reta
            kit.Pickup(PickupKind.Shield, new Vector3(-1.9f, YOficina, 48.4f));
            kit.ReadingDesk("Mesa do laboratorio", new Vector3(2.6f, YOficina, 47.4f), 90f);
            kit.ReadingLamp(new Vector3(2.6f, YOficina + 0.8f, 47f), -90f);
            kit.ShelfArch("Porta do laboratorio", new Vector3(0f, YOficina, 49.5f), 2.7f, 3.2f, 0f);
            kit.Sign("ESCUDO: UM ERRO PERDOADO", new Vector3(-4.8f, YOficina + 2.2f, 48.6f), 0f, 0.04f);

            // corredor de 2,7 m (largura do feixe), estantes de 4 m
            kit.Platform("Las Corredor dos feixes", new Vector3(0f, YOficina, 56.6f), new Vector2(3.9f, 13f));
            kit.Bookshelf("Feixes oeste", new Vector3(-1.65f, YOficina, 50.1f), new Vector3(-1.65f, YOficina, 63.1f), 4f);
            kit.Bookshelf("Feixes leste", new Vector3(1.65f, YOficina, 50.1f), new Vector3(1.65f, YOficina, 63.1f), 4f);
            // F13: dois feixes em onda (1,2 ligado / 2,0 apagado); F5: o terceiro com outro ritmo (1,8 / 1,2)
            kit.Laser("Feixe da onda 1", new Vector3(0f, YOficina, 53.2f), 0f, 1.2f, 2.0f, Onda(0f, 3.2f));
            kit.Laser("Feixe da onda 2", new Vector3(0f, YOficina, 56.7f), 0f, 1.2f, 2.0f, Onda(3.5f, 3.2f));
            kit.Laser("Feixe do outro ritmo", new Vector3(0f, YOficina, 60.2f), 0f, 1.8f, 1.2f, 0f);
            kit.CeilingLight(new Vector3(0f, YOficina + 3.9f, 51.4f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, YOficina + 3.9f, 54.95f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, YOficina + 3.9f, 58.45f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, YOficina + 3.9f, 61.9f), 0f, LuzQuente);
            kit.ReadingLamp(new Vector3(-1.65f, YOficina + 4f, 60.2f), 90f, LuzPerigo);
            kit.ReadingLamp(new Vector3(1.65f, YOficina + 4f, 60.2f), -90f, LuzPerigo);
        }

        // ------------------------------------------------------------------ ato IV: oficina e arquivo (z 63,1..93,1)

        void OficinaEArquivo(PhaseKit kit)
        {
            // W1 (F6): a serra vai e volta em X; passe pelo lado vazio
            kit.Platform("W1 Oficina", new Vector3(0f, YOficina, 66.6f), new Vector2(6f, 7f));
            kit.Bookshelf("Oficina oeste", new Vector3(-3.3f, YOficina, 63.1f), new Vector3(-3.3f, YOficina, 70.1f), 4f);
            kit.Bookshelf("Oficina leste", new Vector3(3.3f, YOficina, 63.1f), new Vector3(3.3f, YOficina, 70.1f), 4f);
            kit.Saw("Serra da oficina", new Vector3(-2f, YOficina, 66.6f), new Vector3(4f, 0f, 0f), 3f, 0f);
            kit.CeilingLight(new Vector3(-2f, YOficina + 4f, 64.2f), 0f, LuzManha);
            kit.CeilingLight(new Vector3(2f, YOficina + 4f, 69f), 0f, LuzManha);
            kit.Sign("PASSE PELO LADO VAZIO", new Vector3(0f, YOficina + 3.3f, 70f), 0f, 0.04f);

            // W2 (F14): três pêndulos sincronizados, refúgios de 1,8 m entre eles
            kit.Platform("W2 Galeria do relogio", new Vector3(0f, YOficina, 76.1f), new Vector2(6f, 12f));
            kit.Bookshelf("Relogio oeste", new Vector3(-3.3f, YOficina, 70.1f), new Vector3(-3.3f, YOficina, 82.1f), 5f);
            kit.Bookshelf("Relogio leste", new Vector3(3.3f, YOficina, 70.1f), new Vector3(3.3f, YOficina, 82.1f), 5f);
            float[] zs = { 72.5f, 76.1f, 79.7f };
            for (int k = 0; k < zs.Length; k++)
            {
                kit.Pendulum($"Pendulo sincronizado {k + 1}", new Vector3(0f, YOficina, zs[k]), 55f, 2.4f, 0f, 0f);
                kit.Platform($"Viga do relogio {k + 1}", new Vector3(0f, YOficina + 4.3f, zs[k]), new Vector2(6f, 0.6f));
            }
            kit.CeilingLight(new Vector3(0f, YOficina + 4.6f, 74.3f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, YOficina + 4.6f, 77.9f), 0f, LuzQuente);
            kit.ReadingLamp(new Vector3(-3.3f, YOficina + 5f, 76.1f), 90f);
            kit.ReadingLamp(new Vector3(3.3f, YOficina + 5f, 76.1f), -90f);
            kit.DreamClock("Relogio da galeria", new Vector3(0f, YOficina + 9f, 84f), 2.2f, 0f, 6f);

            // W3 (F12): corredor de 1,6 m com duas prensas em onda e a página que cede no fim
            kit.Platform("W3 Arquivo", new Vector3(0f, YOficina, 86.1f), new Vector2(3.2f, 8f));
            kit.Bookshelf("Arquivo oeste", new Vector3(-1.1f, YOficina, 82.1f), new Vector3(-1.1f, YOficina, 90.1f), 4.5f);
            kit.Bookshelf("Arquivo leste", new Vector3(1.1f, YOficina, 82.1f), new Vector3(1.1f, YOficina, 90.1f), 4.5f);
            kit.Press("Prensa do arquivo 1", new Vector3(0f, YOficina, 84.3f), 1.4f, 0.8f, Onda(0f, 3.2f), 0f);
            kit.Press("Prensa do arquivo 2", new Vector3(0f, YOficina, 87.6f), 1.4f, 0.8f, Onda(3.3f, 3.2f), 0f);
            kit.CeilingLight(new Vector3(0f, YOficina + 4.2f, 82.8f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, YOficina + 4.2f, 86f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, YOficina + 4.2f, 89.4f), 0f, LuzQuente);
            kit.CrumblingPlatform("CF Pagina do arquivo", new Vector3(0f, YOficina, 91.6f), new Vector2(2f, 3f), 0.8f);
        }

        // ------------------------------------------------------------------ ato V: ilhas de leitura (z 95,4..115,3)

        void IlhasDeLeitura(PhaseKit kit)
        {
            // F11: livros abertos com mesas, vãos de 2,3 m subindo 0,5 m (alcance 2,88 m)
            kit.OpenBookPlatform("X1 Ilha de leitura", new Vector3(0f, 2.1f, 96.9f), new Vector2(3f, 3f));
            kit.ReadingDesk("Mesa da ilha 1", new Vector3(-1.1f, 2.1f, 96.9f), 90f);
            kit.ReadingLamp(new Vector3(-1.1f, 2.9f, 96.4f), 90f);
            kit.OpenBookPlatform("X2 Ilha de leitura", new Vector3(1.5f, 2.6f, 102.2f), new Vector2(3f, 3f));
            kit.OpenBookPlatform("X3 Ilha de leitura", new Vector3(-1.5f, YCais, 107.5f), new Vector2(3f, 3f));
            kit.ReadingDesk("Mesa da ilha 3", new Vector3(-2.6f, YCais, 107.5f), 90f);
            kit.ReadingLamp(new Vector3(-2.6f, YCais + 0.8f, 107f), 90f);
            kit.CeilingLight(new Vector3(0.5f, 2.1f + 4.6f, 96.9f), 0f, LuzManha);
            kit.CeilingLight(new Vector3(1.5f, 2.6f + 4.6f, 102.2f), 0f, LuzManha);
            kit.CeilingLight(new Vector3(-1f, YCais + 4.6f, 107.5f), 0f, LuzManha);
            kit.FloatingBooks("Sob as ilhas", new Vector3(0f, -1.5f, 102f), 3f, 8);

            // cais da Pluma
            kit.OpenBookPlatform("Cais da Pluma", new Vector3(0f, YCais, 113.3f), new Vector2(6f, 4f));
            kit.Pickup(PickupKind.Gravity, new Vector3(0f, YCais, 113.8f));
            kit.CeilingLight(new Vector3(0f, YCais + 4.6f, 113.8f), 0f, LuzPluma);
            kit.Sign("MAR DE PAGINAS: PLUMA", new Vector3(-5f, YCais + 2.2f, 115f), 0f, 0.045f);
        }

        // ------------------------------------------------------------------ ato V: mar de páginas (z 120,5..140,1)

        void MarDePaginas(PhaseKit kit)
        {
            // F15: vãos de 5,2 m, só com a Pluma (6,23 m a +0,5; 6,63 m no plano)
            kit.BookPlatform("Y1 Ilha de paginas", new Vector3(0f, YMar, 121.8f), new Vector2(2.6f, 2.6f), 0f, 1.8f);
            kit.CrumblingPlatform("Y2 Pagina solta", new Vector3(0f, YMar, 129.6f), new Vector2(2.6f, 2.6f), 0.8f);
            kit.CeilingLight(new Vector3(0f, YMar + 6f, 121.8f), 0f, LuzPluma);

            kit.Platform("Base da escadaria", new Vector3(0f, YMar, 138.1f), new Vector2(6f, 4f));
            kit.Pickup(PickupKind.DoubleJump, new Vector3(0f, YMar, 138.6f));
            kit.CeilingLight(new Vector3(0f, YMar + 4.6f, 138.6f), 0f, LuzPulo);
            kit.Sign("PULO DUPLO: SUBA E SALTE", new Vector3(-5f, YMar + 2.2f, 139.6f), 0f, 0.045f);

            // o mar: páginas brilhando bem abaixo do caminho
            for (int k = 0; k < 4; k++)
                kit.FloatingPages($"Mar de paginas {k + 1}", new Vector3(k % 2 == 0 ? -1.5f : 1.5f, 0.3f, 119f + 5.5f * k), 3f, 16);
            kit.DreamMotes("Mar de paginas", new Vector3(0f, 0.5f, 126f), new Vector3(10f, 2f, 24f), C(1f, 0.92f, 0.8f), 200);
        }

        // ------------------------------------------------------------------ ato VI: escadaria, trilho e despertar (z 141,3..175,9)

        void EscadariaEDespertar(PhaseKit kit)
        {
            // F10: degraus de +1,7 m (pulo duplo), vãos de 1,2 m
            kit.BookPlatform("S1 Degrau de livros", new Vector3(1.5f, 5.3f, 142.5f), new Vector2(2.6f, 2.4f), 0f, 1.7f);
            kit.BookPlatform("S2 Degrau de livros", new Vector3(-1.5f, 7f, 146.1f), new Vector2(2.6f, 2.4f), 0f, 1.7f);
            kit.OpenBookPlatform("Pat Patamar da manha", new Vector3(0f, YFim, 150.7f), new Vector2(6f, 4.4f));
            kit.CeilingLight(new Vector3(1.5f, 5.3f + 5f, 142.5f), 0f, LuzPulo);
            kit.CeilingLight(new Vector3(-1.5f, 7f + 5f, 146.1f), 0f, LuzPulo);
            kit.CeilingLight(new Vector3(-1.8f, YFim + 4.6f, 150f), 0f, LuzQuente);
            kit.Sign("A ULTIMA SERRA", new Vector3(-4.8f, YFim + 2.2f, 152.4f), 0f, 0.05f);
            kit.Sign("SALTE QUANDO ELA VIER", new Vector3(-4.8f, YFim + 1.7f, 152.4f), 0f, 0.035f);
            kit.LibraryLadder(new Vector3(5.6f, 2f, 145f), 7f, -90f);

            // F16: trilho de 1,6 m entre estantes de 5 m; a serra corre ao longo dele e não deixa lado livre
            kit.BookPlatform("Ponte Trilho da manha", new Vector3(0f, YFim, 159.9f), new Vector2(2.8f, 14f), 0f, 0.8f);
            kit.Bookshelf("Trilho oeste", new Vector3(-1.1f, YFim, 152.9f), new Vector3(-1.1f, YFim, 166.9f), 5f);
            kit.Bookshelf("Trilho leste", new Vector3(1.1f, YFim, 152.9f), new Vector3(1.1f, YFim, 166.9f), 5f);
            kit.Saw("Ultima serra", new Vector3(0f, YFim, 154.9f), new Vector3(0f, 0f, 10f), 4.5f, 90f);
            kit.ReadingLamp(new Vector3(-1.1f, YFim + 5f, 157f), 90f);
            kit.ReadingLamp(new Vector3(1.1f, YFim + 5f, 163f), -90f);

            // o livro do despertar: o canto de estudo onde o estudante dormiu, com a porta de saída do sonho
            kit.OpenBookPlatform("Fim Livro do despertar", new Vector3(0f, YFim, 171.9f), new Vector2(9f, 8f));
            kit.Goal(new Vector3(0f, YFim, 174.2f), 180f);
            kit.Carpet(new Vector3(-2.7f, YFim, 172.8f), new Vector2(3f, 2.6f));
            kit.ReadingDesk("Mesa onde ele dormiu", new Vector3(-2.8f, YFim, 173.2f), 0f);
            kit.ReadingLamp(new Vector3(-3.3f, YFim + 0.8f, 173.2f), 0f, LuzQuente);
            kit.BookPlatform("Pilha de livros do estudo", new Vector3(-3.7f, YFim + 0.45f, 171.6f), new Vector2(0.8f, 0.6f), 15f, 0.45f);
            kit.Pole(new Vector3(-4f, YFim, 168.6f), LuzQuente);
            kit.Pole(new Vector3(4f, YFim, 168.6f), LuzQuente);
            kit.Bookshelf("Despertar fundo", new Vector3(-4.8f, YFim, 176.2f), new Vector3(4.8f, YFim, 176.2f), 3f);
            kit.Sign("BOM DIA", new Vector3(0f, YFim + 4.2f, 176.1f), 0f, 0.08f);
            kit.Sign("VOCE ACORDOU", new Vector3(0f, YFim + 3.6f, 176.1f), 0f, 0.045f);

            // o sol: relógio gigante que volta a andar, nascendo atrás da porta
            kit.DreamClock("Sol da manha", new Vector3(0f, 18f, 186f), 6f, 0f, 10f);
        }

        // ------------------------------------------------------------------ manhã: cenário fora do caminho

        void Manha(PhaseKit kit)
        {
            // poeira dourada no alto (base a 14 m; a cabeça chega no máximo a 8,7 + 2,2 + 1,76 = 12,7)
            kit.DreamMotes("Luz da manha", new Vector3(0f, 16f, 88f), new Vector3(24f, 4f, 180f), C(1f, 0.90f, 0.70f), 600);

            // uma lembrança de cada arco, flutuando nas laterais
            kit.DreamClock("Relogio parado da sala", new Vector3(-10f, 7f, 10f), 2f, -90f, 2f);
            kit.DreamClock("Relogio da oficina", new Vector3(10f, 8f, 70f), 2.4f, 90f, 4f);
            kit.FloatingBooks("Livros da pressa", new Vector3(8f, 4f, 32f), 2.5f, 7);
            kit.FloatingBooks("Livros do laboratorio", new Vector3(-8f, 6f, 56f), 2.5f, 7);
            kit.FloatingBooks("Livros das ilhas", new Vector3(8.5f, 6f, 104f), 2.5f, 8);
            kit.FloatingPages("Paginas do arquivo", new Vector3(-8.5f, 7f, 88f), 2.5f, 12);
            kit.FloatingPages("Paginas da escadaria", new Vector3(-8f, 10f, 146f), 2.5f, 12);
            kit.FloatingBooks("Livros do despertar", new Vector3(8f, 11f, 165f), 2.5f, 8);
        }
    }
}
