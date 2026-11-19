using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 8 — "Saguão dos Sussurros": o hall de entrada da biblioteca mágica, à noite, sob luz de vela.
    /// Reapresenta as mecânicas com calma, uma por sala, cada uma com um "sussurro" (placa curta) que a explica.
    /// Regra da fase: onde há tapete, dá para parar; onde não há, é preciso atenção (armadilha ou piso que cede).
    /// Percurso: saguão → livros empilhados (pulos) → sala de leitura (mesas como degraus) → mezanino (espinhos fixos)
    /// → corredor do ritmo (espinhos em ciclo) → rotunda (Escudo opcional) → galeria do pêndulo → ponte que cede
    /// → corredor do laser → salto da Pluma até a porta. Pontos e cálculos em docs/Fase8.md.
    /// </summary>
    public sealed class Fase8Builder : IPhaseBuilder
    {
        public int Order => 8;
        public string SceneName => "Fase8_SaguaoDosSussurros";
        public PhaseTheme Theme => Tema;

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        // segundo arco, início: noite quente, quase sem saturação, neblina densa; destaque âmbar de vela
        static readonly PhaseTheme Tema = new PhaseTheme
        {
            Name = "Fase8", Floor = C(0.42f, 0.30f, 0.22f), Wall = C(0.22f, 0.16f, 0.14f), Accent = C(1f, 0.66f, 0.30f),
            Sky = C(0.05f, 0.04f, 0.06f), Equator = C(0.12f, 0.08f, 0.07f), Ground = C(0.03f, 0.02f, 0.02f),
            Fog = C(0.09f, 0.06f, 0.06f), FogDensity = 0.03f, Sun = C(1f, 0.75f, 0.5f), SunIntensity = 0.8f,
            SunEuler = new Vector3(40f, 120f, 0f), Bloom = 0.6f, Vignette = 0.36f, Saturation = 0f, PostExposure = -0.1f,
            Tint = C(1f, 0.94f, 0.86f)
        };

        const float YM = 1.6f; // altura do mezanino e de tudo o que vem depois dele

        static readonly Color LuzVela = new Color(1f, 0.78f, 0.5f);
        static readonly Color LuzPluma = new Color(0.55f, 0.85f, 1f);

        public void Build(PhaseKit kit)
        {
            Saguao(kit);
            LivrosEmpilhados(kit);
            SalaDeLeitura(kit);
            Mezanino(kit);
            CorredorDoRitmo(kit);
            Rotunda(kit);
            GaleriaDoPendulo(kit);
            PonteQueCede(kit);
            CorredorDoLaser(kit);
            SaltoDaPluma(kit);
            Cenario(kit);

            kit.FallZone(-6f);
            kit.LibraryBackdrop(new Vector2(-5f, -4f), new Vector2(21f, 85.2f), -4f, 14f, 6f);
        }

        // ------------------------------------------------------------------ S0: saguão (x -5..5, z -4..4)

        void Saguao(PhaseKit kit)
        {
            kit.BookPlatform("S0 Saguao", new Vector3(0f, 0f, 0f), new Vector2(10f, 8f));
            kit.Spawn(new Vector3(0f, 0f, -1.5f), 0f);

            kit.Bookshelf("Saguao oeste", new Vector3(-4.7f, 0f, -4f), new Vector3(-4.7f, 0f, 4f), 4f);
            kit.Bookshelf("Saguao leste", new Vector3(4.7f, 0f, 4f), new Vector3(4.7f, 0f, -4f), 4f);
            kit.Bookshelf("Saguao fundo", new Vector3(-4.4f, 0f, -3.7f), new Vector3(4.4f, 0f, -3.7f), 4f);
            kit.Bookshelf("Saguao frente oeste", new Vector3(-4.4f, 0f, 3.4f), new Vector3(-1.8f, 0f, 3.4f), 4f);
            kit.Bookshelf("Saguao frente leste", new Vector3(1.8f, 0f, 3.4f), new Vector3(4.4f, 0f, 3.4f), 4f);
            // portal de 2,4 m: as laterais (x ±1,2..±1,8) encostam nas estantes da frente
            kit.ShelfArch("Saida do saguao", new Vector3(0f, 0f, 3.4f), 2.4f, 3.6f, 0f);

            kit.ReadingDesk("Balcao oeste", new Vector3(-3.2f, 0f, 0.5f), 90f);
            kit.ReadingDesk("Balcao leste", new Vector3(3.2f, 0f, 0.5f), 90f);
            kit.LibraryLadder(new Vector3(-3.6f, 0f, -1.8f), 3.5f, 90f);
            kit.ReadingLamp(new Vector3(-3.9f, 0f, -2.9f), 45f);
            kit.ReadingLamp(new Vector3(3.9f, 0f, -2.9f), -45f);
            kit.Carpet(new Vector3(0f, 0f, -0.2f), new Vector2(2.4f, 6f));
            kit.CeilingLight(new Vector3(0f, 4.4f, 0f), 0f, LuzVela);

            kit.Sign("FASE 8", new Vector3(0f, 4.7f, 3.0f), 0f, 0.09f);
            kit.Sign("SAGUAO DOS SUSSURROS", new Vector3(0f, 4.2f, 3.0f), 0f, 0.07f);
            kit.Sign("onde ha tapete, da para parar", new Vector3(0f, 3.3f, 4.1f), 0f, 0.035f);
        }

        // ------------------------------------------------------------------ P1..P3: livros empilhados (pulos de 1,5 / 2,0 / 2,0 m)

        void LivrosEmpilhados(PhaseKit kit)
        {
            kit.BookPlatform("P1 Pilha baixa", new Vector3(0f, 0f, 7f), new Vector2(3f, 3f));           // z 5,5..8,5
            kit.BookPlatform("P2 Pilha alta", new Vector3(0f, 0.5f, 12f), new Vector2(3f, 3f));         // z 10,5..13,5
            kit.BookPlatform("P3 Pilha media", new Vector3(0f, 0f, 17.25f), new Vector2(3f, 3.5f));     // z 15,5..19
            kit.Carpet(new Vector3(0f, 0f, 7f), new Vector2(2f, 2.4f));
            kit.Carpet(new Vector3(0f, 0.5f, 12f), new Vector2(2f, 2.4f));
            kit.Carpet(new Vector3(0f, 0f, 17.25f), new Vector2(2f, 2.9f));
            kit.CeilingLight(new Vector3(0f, 4f, 9.5f), 0f, LuzVela);
            kit.CeilingLight(new Vector3(0f, 4f, 16f), 0f, LuzVela);
        }

        // ------------------------------------------------------------------ F1: sala de leitura; mesas como degraus até o mezanino

        void SalaDeLeitura(PhaseKit kit)
        {
            kit.BookPlatform("F1 Sala de leitura", new Vector3(0f, 0f, 24f), new Vector2(8f, 8f));     // x -4..4, z 20..28
            kit.Bookshelf("Sala oeste", new Vector3(-3.7f, 0f, 20f), new Vector3(-3.7f, 0f, 28f), 3.4f);
            kit.Bookshelf("Sala leste", new Vector3(3.7f, 0f, 28f), new Vector3(3.7f, 0f, 20f), 3.4f);
            // tampo a 0,8 m; o mezanino está 0,8 m acima do tampo (z 26..26,8; o mezanino começa em z 27)
            kit.ReadingDesk("Degrau oeste", new Vector3(-1.4f, 0f, 26.4f), 0f);
            kit.ReadingDesk("Degrau leste", new Vector3(1.4f, 0f, 26.4f), 0f);
            kit.Carpet(new Vector3(0f, 0f, 23f), new Vector2(2.4f, 5f));
            kit.ReadingLamp(new Vector3(-3.0f, 0f, 20.8f), 30f);
            kit.ReadingLamp(new Vector3(3.0f, 0f, 20.8f), -30f);
            kit.CeilingLight(new Vector3(0f, 4.2f, 23f), 0f, LuzVela);
            kit.Sign("suba pelas mesas", new Vector3(0f, 3.7f, 26.9f), 0f, 0.04f);
        }

        // ------------------------------------------------------------------ M1: mezanino com duas fileiras de espinhos fixos

        void Mezanino(PhaseKit kit)
        {
            // x -3..3, z 27..37; a pilha desce até o piso da sala para a face parecer maciça
            kit.BookPlatform("M1 Mezanino", new Vector3(0f, YM, 32f), new Vector2(6f, 10f), 0f, 1.6f);
            kit.Bookshelf("Mezanino oeste", new Vector3(-2.7f, YM, 27f), new Vector3(-2.7f, YM, 37f), 3f);
            kit.Bookshelf("Mezanino leste", new Vector3(2.7f, YM, 37f), new Vector3(2.7f, YM, 27f), 3f);
            kit.Bookshelf("Mezanino frente oeste", new Vector3(-2.4f, YM, 36.7f), new Vector3(-0.85f, YM, 36.7f), 3f);
            kit.Bookshelf("Mezanino frente leste", new Vector3(0.85f, YM, 36.7f), new Vector3(2.4f, YM, 36.7f), 3f);

            // fileira 1 (z 30,8): passagem livre a leste (x 1,3..2,4); fileira 2 (z 33,6): a oeste (x -2,4..-1,3)
            kit.Spikes("Fixo 1A", new Vector3(-1.4f, YM, 30.8f));
            kit.Spikes("Fixo 1B", new Vector3(0.4f, YM, 30.8f));
            kit.Spikes("Fixo 2A", new Vector3(1.4f, YM, 33.6f));
            kit.Spikes("Fixo 2B", new Vector3(-0.4f, YM, 33.6f));

            kit.Carpet(new Vector3(0f, YM, 28.2f), new Vector2(4.4f, 1.6f));
            kit.Carpet(new Vector3(0f, YM, 35.4f), new Vector2(4.4f, 1.2f));
            kit.CeilingLight(new Vector3(0f, YM + 3.4f, 29f), 0f, LuzVela);
            kit.CeilingLight(new Vector3(0f, YM + 3.4f, 35f), 0f, LuzVela);
            kit.Sign("desvie, nao pule", new Vector3(0f, YM + 2.7f, 27.3f), 0f, 0.04f);
        }

        // ------------------------------------------------------------------ corredor do ritmo: dois pares de espinhos em ciclo

        void CorredorDoRitmo(PhaseKit kit)
        {
            // piso x -1,45..1,45, z 37..49; estantes em x ±1,15 deixam 1,7 m livres (os espinhos cobrem 1,8 m)
            kit.BookPlatform("Corredor do ritmo", new Vector3(0f, YM, 43f), new Vector2(2.9f, 12f));
            kit.ShelfArch("Entrada do ritmo", new Vector3(0f, YM, 37.6f), 1.7f, 3.4f, 0f);   // laterais z 37..38,2
            kit.Bookshelf("Ritmo oeste", new Vector3(-1.15f, YM, 38.2f), new Vector3(-1.15f, YM, 49f), 3f);
            kit.Bookshelf("Ritmo leste", new Vector3(1.15f, YM, 49f), new Vector3(1.15f, YM, 38.2f), 3f);

            // 1,2 s expostos e 1,8 s recolhidos (3,0 s); o par B está meio ciclo atrasado
            kit.Spikes("Ritmo A1", new Vector3(0f, YM, 40f), true, 1.2f, 1.8f, 0f);
            kit.Spikes("Ritmo A2", new Vector3(0f, YM, 42f), true, 1.2f, 1.8f, 0f);
            kit.Spikes("Ritmo B1", new Vector3(0f, YM, 46f), true, 1.2f, 1.8f, 1.5f);
            kit.Spikes("Ritmo B2", new Vector3(0f, YM, 48f), true, 1.2f, 1.8f, 1.5f);

            kit.Carpet(new Vector3(0f, YM, 44f), new Vector2(1.5f, 2f));   // refúgio entre os pares (z 42,9..45,1)
            kit.CeilingLight(new Vector3(0f, YM + 3.3f, 38.5f), 0f, LuzVela);
            kit.CeilingLight(new Vector3(0f, YM + 3.3f, 44f), 0f, LuzVela);
            kit.Sign("observe o ritmo", new Vector3(0f, YM + 3.0f, 36.9f), 0f, 0.04f);
        }

        // ------------------------------------------------------------------ R1: rotunda, livro aberto gigante com o Escudo

        void Rotunda(PhaseKit kit)
        {
            kit.OpenBookPlatform("R1 Rotunda", new Vector3(0f, YM, 53f), new Vector2(8f, 8f));       // x -4..4, z 49..57
            kit.Pickup(PickupKind.Shield, new Vector3(2.8f, YM, 53f));   // opcional: absorve um toque
            kit.Carpet(new Vector3(0f, YM, 53f), new Vector2(2.4f, 7f));
            kit.ReadingLamp(new Vector3(-3.3f, YM, 49.7f), 45f);
            kit.ReadingLamp(new Vector3(-3.3f, YM, 56.3f), 135f);
            kit.ReadingLamp(new Vector3(3.3f, YM, 56.3f), -135f);
            kit.CeilingLight(new Vector3(0f, YM + 3.6f, 53f), 0f, LuzVela);
            kit.Sign("espere o pendulo passar", new Vector3(4.2f, YM + 2.8f, 53f), 90f, 0.04f);
        }

        // ------------------------------------------------------------------ galeria do pêndulo (avança em +X)

        void GaleriaDoPendulo(PhaseKit kit)
        {
            kit.BookPlatform("Galeria do pendulo", new Vector3(10f, YM, 53f), new Vector2(12f, 3f));  // x 4..16, z 51,5..54,5
            // balança em Z (yaw 90), atravessando a passarela inteira; 50°, 3,2 s
            kit.Pendulum("Pendulo da galeria", new Vector3(10f, YM, 53f), 50f, 3.2f, 0f, 90f);
            // viga de z 49,4 a 56,6; os apoios (sem colisão) ficam a 1,6 m das bordas da passarela
            kit.BookPlatform("Viga do pendulo", new Vector3(10f, YM + 4.3f, 53f), new Vector2(0.6f, 7.2f), 0f, 0.5f);
            kit.BackdropShelf("Apoio da viga sul", new Vector3(9.7f, -4f, 49.6f), new Vector3(10.3f, -4f, 49.6f), YM + 8.3f);
            kit.BackdropShelf("Apoio da viga norte", new Vector3(10.3f, -4f, 56.4f), new Vector3(9.7f, -4f, 56.4f), YM + 8.3f);
            kit.CeilingLight(new Vector3(6f, YM + 3.4f, 53f), 90f, LuzVela);
            kit.CeilingLight(new Vector3(14f, YM + 3.4f, 53f), 90f, LuzVela);
        }

        // ------------------------------------------------------------------ L1 → ponte de quatro livros que cedem → L2

        void PonteQueCede(PhaseKit kit)
        {
            kit.BookPlatform("L1 Patamar da ponte", new Vector3(19f, YM, 53f), new Vector2(4f, 4f));      // x 17..21, z 51..55
            kit.Carpet(new Vector3(19f, YM, 53f), new Vector2(2.4f, 3f));
            kit.ReadingLamp(new Vector3(20.5f, YM, 51.5f), -45f);

            // sem tapete: 2 x 2 m, vãos de 0,8 m; cada um cede 0,8 s depois de pisado
            for (int i = 0; i < 4; i++)
                kit.CrumblingPlatform($"Ponte que cede {i + 1}", new Vector3(19f, YM, 57f + 2.8f * i), new Vector2(2f, 2f), 0.8f);

            kit.BookPlatform("L2 Patamar do laser", new Vector3(19f, YM, 69.2f), new Vector2(4f, 4f));   // z 67,2..71,2
            kit.Carpet(new Vector3(19f, YM, 69.2f), new Vector2(2.4f, 3f));
            kit.ReadingLamp(new Vector3(20.5f, YM, 70.7f), -135f);
            kit.CeilingLight(new Vector3(19f, YM + 3.4f, 53f), 0f, LuzVela);
            kit.CeilingLight(new Vector3(19f, YM + 3.4f, 69.2f), 0f, LuzVela);
            kit.Sign("nao pare", new Vector3(19f, YM + 2.8f, 55.2f), 0f, 0.05f);
        }

        // ------------------------------------------------------------------ corredor do laser (avança em -X)

        void CorredorDoLaser(PhaseKit kit)
        {
            // piso x 9..17, z 67,25..71,15; estantes em z 67,55 e 70,85 deixam 2,7 m livres, a largura do feixe
            kit.BookPlatform("Corredor do laser", new Vector3(13f, YM, 69.2f), new Vector2(8f, 3.9f));
            kit.Bookshelf("Laser sul", new Vector3(9.5f, YM, 67.55f), new Vector3(16.5f, YM, 67.55f), 3f);
            kit.Bookshelf("Laser norte", new Vector3(16.5f, YM, 70.85f), new Vector3(9.5f, YM, 70.85f), 3f);
            kit.Laser("Laser do saguao", new Vector3(13f, YM, 69.2f), 90f, 1.4f, 2.0f, 0f);
            kit.CeilingLight(new Vector3(15.5f, YM + 3.3f, 69.2f), 90f, LuzVela);
            kit.CeilingLight(new Vector3(10.5f, YM + 3.3f, 69.2f), 90f, LuzVela);
            kit.Sign("espere apagar", new Vector3(16.8f, YM + 2.6f, 69.2f), -90f, 0.04f);
        }

        // ------------------------------------------------------------------ L3 (Pluma) → vão de 5 m → patamar da porta

        void SaltoDaPluma(PhaseKit kit)
        {
            kit.BookPlatform("L3 Patamar da Pluma", new Vector3(6f, YM, 69.2f), new Vector2(6f, 6f));   // x 3..9, z 66,2..72,2
            kit.Pickup(PickupKind.Gravity, new Vector3(5f, YM, 69.2f));
            kit.Carpet(new Vector3(6f, YM, 69.2f), new Vector2(2.4f, 5f));
            kit.ReadingLamp(new Vector3(3.5f, YM, 66.7f), 45f);
            kit.ReadingLamp(new Vector3(3.5f, YM, 71.7f), 135f);
            kit.CeilingLight(new Vector3(5f, YM + 3.4f, 69.2f), 0f, LuzPluma);
            kit.Sign("a pluma te leva", new Vector3(6f, YM + 2.8f, 72.4f), 0f, 0.05f);

            kit.OpenBookPlatform("GP Patamar da porta", new Vector3(6f, YM, 81.2f), new Vector2(8f, 8f)); // x 2..10, z 77,2..85,2
            kit.Carpet(new Vector3(6f, YM, 80.5f), new Vector2(2.4f, 5f));
            kit.ReadingLamp(new Vector3(2.6f, YM, 84.6f), 135f);
            kit.ReadingLamp(new Vector3(9.4f, YM, 84.6f), -135f);
            kit.CeilingLight(new Vector3(6f, YM + 3.6f, 80f), 0f, LuzVela);
            kit.Goal(new Vector3(6f, YM, 83.5f), 180f);
        }

        // ------------------------------------------------------------------ cenário fora do caminho

        void Cenario(PhaseKit kit)
        {
            kit.DreamClock("Relogio da rotunda", new Vector3(0f, 8.5f, 59f), 2.5f);
            kit.DreamClock("Relogio da porta", new Vector3(6f, 10f, 89f), 3f);
            kit.FloatingBooks("Sala oeste", new Vector3(-8.5f, 3f, 24f), 2f, 6);
            kit.FloatingBooks("Mezanino leste", new Vector3(7.5f, 4.5f, 33f), 2f, 5);
            kit.FloatingBooks("Ponte", new Vector3(24.5f, 3.5f, 61f), 1.8f, 6);
            kit.FloatingBooks("Vazio central", new Vector3(10f, 2f, 62f), 2.2f, 7);
            kit.FloatingPages("Rotunda", new Vector3(0f, 7.5f, 53f), 2.5f, 12);
            kit.FloatingPages("Porta", new Vector3(6f, 7f, 81f), 2.5f, 10);
            kit.DreamMotes("Saguao", new Vector3(0f, 2.5f, 0f), new Vector3(8f, 4f, 7f), null, 80);
        }
    }
}
