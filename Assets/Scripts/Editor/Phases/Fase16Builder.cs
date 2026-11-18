using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 16 — "Observatório dos Sonhos": o topo da biblioteca se abre para um céu estrelado. O caminho sobe de
    /// y 0 a y 9,6 por plataformas altas sobre o vazio. Ideia central: <b>as estrelas marcam o chão</b>. Cada piso
    /// firme tem uma "estrela" (luminária azul) pendurada acima; as estrelas cadentes (pisos que cedem) não têm.
    ///
    /// Sequência (+Z):
    ///  1. Mirante de entrada (spawn).
    ///  2. Escada da constelação: cinco pilhas em zigue-zague, subindo 0,6 m, duas delas estrelas cadentes.
    ///  3. Ponte dos cometas: ponte de 2 m com dois pêndulos defasados e um refúgio entre eles.
    ///  4. Mapa celeste (Pluma) e Ilhas das estrelas: três vãos de 5,1 m e um de 5,0 m, só com a Pluma.
    ///  5. Galeria das lunetas: teto baixo com laser, espinhos e laser em onda; refúgio a céu aberto;
    ///     corredor de 1,6 m com duas prensas em onda.
    ///  6. Escadaria da luneta (Pulo Duplo): quatro degraus de +1,7 m até o mirante alto (y 9,6).
    ///  7. Trilho do cometa: corredor de estantes com uma serra que corre ao longo dele. Não há lado livre:
    ///     a serra é saltada com o pulo duplo. Depois, duas estrelas cadentes e a cúpula com a porta.
    /// Cada tipo de poder aparece uma vez só (o efeito de um segundo coletável igual seria desfeito pelo primeiro).
    /// A lua (relógio gigante) flutua atrás da porta e aparece desde o spawn. Medidas e contas em docs/Fase16.md.
    /// </summary>
    public sealed class Fase16Builder : IPhaseBuilder
    {
        public int Order => 16;
        public string SceneName => "Fase16_ObservatorioDosSonhos";
        public PhaseTheme Theme => Tema;

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        // noite estrelada colorida, mais clara que a Fase 15 (saturação 20, exposição 0,15): índigo com destaque azul-estrela
        static readonly PhaseTheme Tema = new PhaseTheme
        {
            Name = "Fase16", Floor = C(0.34f, 0.40f, 0.66f), Wall = C(0.18f, 0.18f, 0.36f), Accent = C(0.62f, 0.70f, 1f),
            Sky = C(0.07f, 0.08f, 0.22f), Equator = C(0.16f, 0.16f, 0.34f), Ground = C(0.04f, 0.04f, 0.10f),
            Fog = C(0.12f, 0.12f, 0.30f), FogDensity = 0.015f, Sun = C(0.74f, 0.80f, 1f), SunIntensity = 1.1f,
            SunEuler = new Vector3(60f, -20f, 0f), Bloom = 1.0f, Vignette = 0.26f, Saturation = 25f, PostExposure = 0.2f,
            Tint = C(0.94f, 0.95f, 1f)
        };

        static readonly Color LuzEstrela = new Color(0.70f, 0.80f, 1f);  // piso firme
        static readonly Color LuzQuente = new Color(1f, 0.82f, 0.55f);   // refúgio
        static readonly Color LuzPluma = new Color(0.80f, 0.66f, 1f);    // trecho da Pluma
        static readonly Color LuzPulo = new Color(0.50f, 0.85f, 1f);     // trecho do pulo duplo
        static readonly Color Estrelas = new Color(0.85f, 0.90f, 1f);

        // alturas do topo dos pisos
        const float YPonte = 1.8f;
        const float YGaleria = 2.8f;
        const float YCupula = 9.6f;

        // offset de uma armadilha para que um corredor a 5 m/s encontre a mesma fase do ciclo em todas
        static float Onda(float distancia, float periodo) => Mathf.Repeat(-distancia / 5f, periodo);

        public void Build(PhaseKit kit)
        {
            MiranteDeEntrada(kit);
            EscadaDaConstelacao(kit);
            PonteDosCometas(kit);
            MapaCeleste(kit);
            IlhasDasEstrelas(kit);
            GaleriaDasLunetas(kit);
            EscadariaDaLuneta(kit);
            TrilhoDoCometa(kit);
            Cupula(kit);
            Ceu(kit);

            kit.FallZone(-10f);
            kit.LibraryBackdrop(new Vector2(-6f, -4f), new Vector2(6f, 155f), -12f, 26f, 9f);
        }

        // estrela pendurada sobre um piso firme (luminária azul, sem colisão)
        static void Estrela(PhaseKit kit, Vector3 pisoTopo, float altura = 4.6f, Color? cor = null) =>
            kit.CeilingLight(pisoTopo + Vector3.up * altura, 0f, cor ?? LuzEstrela);

        // ------------------------------------------------------------------ E0: mirante de entrada (x -4..4, z -3..3)

        void MiranteDeEntrada(PhaseKit kit)
        {
            kit.Platform("E0 Mirante de entrada", new Vector3(0f, 0f, 0f), new Vector2(8f, 6f));
            kit.Spawn(new Vector3(0f, 0f, -1f), 0f);
            kit.Carpet(new Vector3(0f, 0f, 0.2f), new Vector2(2.2f, 4.6f));

            // estantes baixas (3 m): o céu fica à vista desde o começo
            kit.Bookshelf("Entrada fundo", new Vector3(-4.3f, 0f, -3.3f), new Vector3(4.3f, 0f, -3.3f), 3f);
            kit.Bookshelf("Entrada oeste", new Vector3(-4.3f, 0f, -3f), new Vector3(-4.3f, 0f, 3f), 3f);
            kit.Bookshelf("Entrada leste", new Vector3(4.3f, 0f, -3f), new Vector3(4.3f, 0f, 3f), 3f);
            kit.ShelfArch("Portal do observatorio", new Vector3(0f, 0f, 2.4f), 2.4f, 3.6f, 0f);

            kit.ReadingDesk("Mesa do mirante", new Vector3(-2.8f, 0f, -1.8f), 90f);
            kit.ReadingLamp(new Vector3(-2.8f, 0.8f, -1.3f), 90f);
            kit.CeilingLight(new Vector3(0f, 4.2f, -0.6f), 0f, LuzQuente);

            kit.Sign("FASE 16", new Vector3(0f, 5.5f, 2.4f), 0f, 0.09f);
            kit.Sign("OBSERVATORIO DOS SONHOS", new Vector3(0f, 4.9f, 2.4f), 0f, 0.07f);
            kit.Sign("AS ESTRELAS MARCAM O CHAO", new Vector3(0f, 4.35f, 2.4f), 0f, 0.04f);
        }

        // ------------------------------------------------------------------ S1: escada da constelação (z 5..25,8)

        void EscadaDaConstelacao(PhaseKit kit)
        {
            // vãos de 2,0 m; sobe 0,6 m a cada pilha firme (alcance a +0,6: 2,78 m)
            kit.BookPlatform("P1 Estrela", new Vector3(-0.8f, 0.6f, 6.2f), new Vector2(2.4f, 2.4f), 0f, 1.6f);
            kit.CrumblingPlatform("P2 Estrela cadente", new Vector3(0.8f, 0.6f, 10.6f), new Vector2(2.4f, 2.4f), 0.8f);
            kit.BookPlatform("P3 Estrela", new Vector3(-0.8f, 1.2f, 15f), new Vector2(2.4f, 2.4f), 0f, 2.2f);
            kit.CrumblingPlatform("P4 Estrela cadente", new Vector3(0.8f, 1.2f, 19.4f), new Vector2(2.4f, 2.4f), 0.8f);
            kit.BookPlatform("P5 Patamar da ponte", new Vector3(0f, YPonte, 24.2f), new Vector2(4f, 3.2f), 0f, 2.8f);

            // só os pisos firmes têm estrela em cima
            Estrela(kit, new Vector3(-0.8f, 0.6f, 6.2f));
            Estrela(kit, new Vector3(-0.8f, 1.2f, 15f));
            Estrela(kit, new Vector3(0f, YPonte, 24.2f), 4.6f, LuzQuente);

            kit.Sign("ESTRELA CADENTE NAO ESPERA", new Vector3(-4.2f, 3.4f, 9.4f), 0f, 0.04f);
            kit.FloatingBooks("Sob a escada", new Vector3(4.5f, -2f, 13f), 2.2f, 6);
        }

        // ------------------------------------------------------------------ S2: ponte dos cometas (x -1..1, z 25,8..38,2)

        void PonteDosCometas(PhaseKit kit)
        {
            kit.BookPlatform("B Ponte dos cometas", new Vector3(0f, YPonte, 32f), new Vector2(2f, 12.4f), 0f, 0.8f);

            // balançam em X, cruzando a ponte inteira; meio período defasados
            float[] zs = { 29.5f, 34.5f };
            for (int k = 0; k < zs.Length; k++)
            {
                kit.Pendulum($"Cometa {k + 1}", new Vector3(0f, YPonte, zs[k]), 65f, 2.6f, 1.3f * k, 0f);
                kit.Platform($"Viga do cometa {k + 1}", new Vector3(0f, YPonte + 4.3f, zs[k]), new Vector2(7.6f, 0.6f));
                kit.Pillar($"Coluna do cometa oeste {k + 1}", new Vector3(-3.8f, -8f, zs[k]), YPonte + 4.3f + 8f);
                kit.Pillar($"Coluna do cometa leste {k + 1}", new Vector3(3.8f, -8f, zs[k]), YPonte + 4.3f + 8f);
            }
            // refúgio entre os pêndulos (z 30,4..33,6)
            kit.CeilingLight(new Vector3(0f, YPonte + 3.8f, 32f), 0f, LuzQuente);
            kit.Sign("PASSE LOGO DEPOIS DO COMETA", new Vector3(0f, YPonte + 4.9f, 26.4f), 0f, 0.04f);
        }

        // ------------------------------------------------------------------ S3: mapa celeste (x -3..3, z 38,2..44,2)

        void MapaCeleste(PhaseKit kit)
        {
            kit.OpenBookPlatform("M Mapa celeste", new Vector3(0f, YPonte, 41.2f), new Vector2(6f, 6f));
            kit.Pickup(PickupKind.Gravity, new Vector3(1.6f, YPonte, 42.6f));
            kit.CeilingLight(new Vector3(1.6f, YPonte + 4.6f, 42.6f), 0f, LuzPluma);

            // mesa no fundo do livro, longe da borda de salto
            kit.ReadingDesk("Mesa do mapa", new Vector3(-2.2f, YPonte, 39f), 0f);
            kit.ReadingLamp(new Vector3(-2.7f, YPonte + 0.8f, 39f), 0f);
            kit.Sign("A PLUMA LEVA AS ILHAS", new Vector3(-4.8f, YPonte + 2.2f, 43.5f), 0f, 0.045f);
            kit.Sign("NAO PARE: SAO 30 S", new Vector3(-4.8f, YPonte + 1.7f, 43.5f), 0f, 0.035f);
        }

        // ------------------------------------------------------------------ S4: ilhas das estrelas (z 49,3..67,3)

        void IlhasDasEstrelas(PhaseKit kit)
        {
            // vãos de 5,1 m (sobe 0,5 m duas vezes): só a Pluma (6,23 m a +0,5) atravessa
            kit.BookPlatform("I1 Ilha", new Vector3(0f, 2.3f, 50.6f), new Vector2(2.6f, 2.6f), 0f, 1.8f);
            kit.CrumblingPlatform("I2 Ilha cadente", new Vector3(-0.8f, 2.3f, 58.3f), new Vector2(2.6f, 2.6f), 0.8f);
            kit.BookPlatform("I3 Ilha", new Vector3(0.8f, YGaleria, 66f), new Vector2(2.6f, 2.6f), 0f, 2.2f);

            // estrelas a 6 m: acima do apogeu da Pluma (2,2 m) mais a cabeça (1,76 m)
            Estrela(kit, new Vector3(0f, 2.3f, 50.6f), 6f, LuzPluma);
            Estrela(kit, new Vector3(0.8f, YGaleria, 66f), 6f, LuzPluma);

            kit.FloatingPages("Sob as ilhas", new Vector3(0f, -2.5f, 58f), 4f, 18);
            kit.DreamMotes("Rastro da Pluma", new Vector3(0f, 10.5f, 58f), new Vector3(5f, 2f, 18f), LuzPluma, 140);
        }

        // ------------------------------------------------------------------ S5: galeria das lunetas (z 72,3..104,3)

        void GaleriaDasLunetas(PhaseKit kit)
        {
            // pátio de chegada (sem mesa: dela daria para subir no teto da galeria com a Pluma)
            kit.Platform("Pat Patio das lunetas", new Vector3(0f, YGaleria, 75.3f), new Vector2(8f, 6f));
            kit.Carpet(new Vector3(0f, YGaleria, 75f), new Vector2(2.2f, 4.6f));
            kit.Bookshelf("Patio oeste", new Vector3(-4.3f, YGaleria, 72.3f), new Vector3(-4.3f, YGaleria, 78.3f), 3.5f);
            kit.Bookshelf("Patio leste", new Vector3(4.3f, YGaleria, 72.3f), new Vector3(4.3f, YGaleria, 78.3f), 3.5f);
            kit.Bookshelf("Patio frente oeste", new Vector3(-4.3f, YGaleria, 78.6f), new Vector3(-1.95f, YGaleria, 78.6f), 3.5f);
            kit.Bookshelf("Patio frente leste", new Vector3(1.95f, YGaleria, 78.6f), new Vector3(4.3f, YGaleria, 78.6f), 3.5f);
            kit.ShelfArch("Boca da galeria", new Vector3(0f, YGaleria, 77.7f), 2.7f, 2.3f, 0f);
            kit.ReadingLamp(new Vector3(-4.3f, YGaleria + 3.5f, 75f), 90f);
            kit.ReadingLamp(new Vector3(4.3f, YGaleria + 3.5f, 75f), -90f);
            Estrela(kit, new Vector3(0f, YGaleria, 74.5f), 4.6f, LuzQuente);
            kit.Sign("GALERIA DAS LUNETAS", new Vector3(0f, YGaleria + 3.4f, 78.2f), 0f, 0.06f);
            kit.Sign("TETO BAIXO: SIGA A ONDA", new Vector3(0f, YGaleria + 2.95f, 78.2f), 0f, 0.035f);

            // G1: teto a 2,3 m (o pé sobe no máximo 0,54 m, nem com a Pluma); 2,7 m livres = largura do feixe
            kit.Platform("G1 Galeria baixa", new Vector3(0f, YGaleria, 84.3f), new Vector2(3.9f, 12f));
            kit.Bookshelf("Galeria baixa oeste", new Vector3(-1.65f, YGaleria, 78.3f), new Vector3(-1.65f, YGaleria, 90.3f), 2.3f);
            kit.Bookshelf("Galeria baixa leste", new Vector3(1.65f, YGaleria, 78.3f), new Vector3(1.65f, YGaleria, 90.3f), 2.3f);
            kit.BookPlatform("Teto da galeria", new Vector3(0f, YGaleria + 2.8f, 84.3f), new Vector2(3.9f, 12f), 0f, 0.5f);

            // ciclo comum de 3 s: laser 1,4 ligado / 1,6 apagado; espinhos 1,2 fora / 1,8 dentro
            kit.Laser("Luneta 1", new Vector3(0f, YGaleria, 81f), 0f, 1.4f, 1.6f, Onda(0f, 3f));
            kit.Spikes("Galeria espinhos", new Vector3(0f, YGaleria, 84.5f), true, 1.2f, 1.8f, Onda(3.5f, 3f));
            kit.Laser("Luneta 2", new Vector3(0f, YGaleria, 88f), 0f, 1.4f, 1.6f, Onda(7f, 3f));
            kit.CeilingLight(new Vector3(0f, YGaleria + 2.3f, 79.6f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, YGaleria + 2.3f, 82.7f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, YGaleria + 2.3f, 86.3f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, YGaleria + 2.3f, 89.5f), 0f, LuzEstrela);

            // G2: refúgio a céu aberto
            kit.Platform("G2 Refugio", new Vector3(0f, YGaleria, 91.8f), new Vector2(4f, 3f));
            Estrela(kit, new Vector3(0f, YGaleria, 91.8f), 4.6f, LuzQuente);

            // G3: corredor de 1,6 m (a prensa cobre a largura toda), estantes de 4,5 m
            kit.Platform("G3 Corredor das prensas", new Vector3(0f, YGaleria, 98.8f), new Vector2(3.4f, 11f));
            kit.Bookshelf("Prensas oeste", new Vector3(-1.1f, YGaleria, 93.3f), new Vector3(-1.1f, YGaleria, 104.3f), 4.5f);
            kit.Bookshelf("Prensas leste", new Vector3(1.1f, YGaleria, 93.3f), new Vector3(1.1f, YGaleria, 104.3f), 4.5f);
            // ciclo de 3,2 s (1,4 erguida + 0,2 descendo + 0,8 embaixo + 0,8 subindo)
            kit.Press("Prensa da luneta 1", new Vector3(0f, YGaleria, 96f), 1.4f, 0.8f, Onda(0f, 3.2f), 0f);
            kit.Press("Prensa da luneta 2", new Vector3(0f, YGaleria, 99.6f), 1.4f, 0.8f, Onda(3.6f, 3.2f), 0f);
            kit.CeilingLight(new Vector3(0f, YGaleria + 4.2f, 94.5f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, YGaleria + 4.2f, 97.8f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, YGaleria + 4.2f, 102.5f), 0f, LuzQuente);
            kit.ReadingLamp(new Vector3(-1.1f, YGaleria + 4.5f, 98f), 90f, LuzEstrela);
            kit.ReadingLamp(new Vector3(1.1f, YGaleria + 4.5f, 101f), -90f, LuzEstrela);
        }

        // ------------------------------------------------------------------ S6: escadaria da luneta (z 104,3..124,7)

        void EscadariaDaLuneta(PhaseKit kit)
        {
            kit.Platform("Pe Pe da luneta", new Vector3(0f, YGaleria, 106.3f), new Vector2(4f, 4f));
            kit.Pickup(PickupKind.DoubleJump, new Vector3(0f, YGaleria, 105.6f));
            kit.CeilingLight(new Vector3(0f, YGaleria + 4.6f, 105.6f), 0f, LuzPulo);
            kit.Sign("PULO DUPLO: SUBA A LUNETA", new Vector3(-3.9f, YGaleria + 2.4f, 107.5f), 0f, 0.045f);

            // degraus de +1,7 m (o pulo simples sobe 1,1 m; a borda alcançável sem poder é 1,4 m), vãos de 1,2 m
            kit.BookPlatform("T1 Degrau da luneta", new Vector3(1.5f, 4.5f, 110.7f), new Vector2(2.6f, 2.4f), 0f, 1.7f);
            kit.BookPlatform("T2 Degrau da luneta", new Vector3(-1.5f, 6.2f, 114.3f), new Vector2(2.6f, 2.4f), 0f, 1.7f);
            kit.BookPlatform("T3 Degrau da luneta", new Vector3(1.5f, 7.9f, 117.9f), new Vector2(2.6f, 2.4f), 0f, 1.7f);
            Estrela(kit, new Vector3(1.5f, 4.5f, 110.7f), 5f, LuzPulo);
            Estrela(kit, new Vector3(-1.5f, 6.2f, 114.3f), 5f, LuzPulo);
            Estrela(kit, new Vector3(1.5f, 7.9f, 117.9f), 5f, LuzPulo);

            // mirante alto (y 9,6)
            kit.OpenBookPlatform("Mir Mirante da cupula", new Vector3(0f, YCupula, 122.5f), new Vector2(6f, 4.4f));
            kit.Carpet(new Vector3(0f, YCupula, 122.4f), new Vector2(4.4f, 2.4f), 90f);
            Estrela(kit, new Vector3(-1.8f, YCupula, 121.6f), 4.6f, LuzQuente);
            kit.Sign("SALTE A SERRA", new Vector3(-4.8f, YCupula + 2.2f, 124f), 0f, 0.05f);
            kit.Sign("COM O PULO DUPLO", new Vector3(-4.8f, YCupula + 1.7f, 124f), 0f, 0.035f);

            kit.LibraryLadder(new Vector3(5.5f, 2f, 114f), 7f, -90f);
            kit.FloatingBooks("Ao lado da luneta", new Vector3(-7.2f, 7f, 112f), 2f, 6);
        }

        // ------------------------------------------------------------------ S7: trilho do cometa (z 124,7..138,7)

        void TrilhoDoCometa(PhaseKit kit)
        {
            // corredor de 1,6 m entre estantes de 5 m: a serra (1,56 m) não deixa lado livre
            kit.BookPlatform("Ponte Trilho do cometa", new Vector3(0f, YCupula, 131.7f), new Vector2(2.8f, 14f), 0f, 0.8f);
            kit.Bookshelf("Trilho oeste", new Vector3(-1.1f, YCupula, 124.7f), new Vector3(-1.1f, YCupula, 138.7f), 5f);
            kit.Bookshelf("Trilho leste", new Vector3(1.1f, YCupula, 124.7f), new Vector3(1.1f, YCupula, 138.7f), 5f);

            // vai de z 126,7 a 136,7 em 2,25 s e volta (período 4,5 s); o disco fica a 0,85 m do piso
            kit.Saw("Cometa de serra", new Vector3(0f, YCupula, 126.7f), new Vector3(0f, 0f, 10f), 4.5f, 90f);
            kit.ReadingLamp(new Vector3(-1.1f, YCupula + 5f, 129f), 90f, LuzEstrela);
            kit.ReadingLamp(new Vector3(1.1f, YCupula + 5f, 134f), -90f, LuzEstrela);
            kit.CeilingLight(new Vector3(0f, YCupula + 5.6f, 131.7f), 0f, LuzEstrela);
        }

        // ------------------------------------------------------------------ fim: estrelas cadentes e cúpula (z 139,7..154,2)

        void Cupula(PhaseKit kit)
        {
            kit.CrumblingPlatform("C1 Estrela cadente", new Vector3(0f, YCupula, 140.7f), new Vector2(2f, 2f), 0.8f);
            kit.CrumblingPlatform("C2 Estrela cadente", new Vector3(0f, YCupula, 143.7f), new Vector2(2f, 2f), 0.8f);

            kit.OpenBookPlatform("Fim Cupula das estrelas", new Vector3(0f, YCupula, 150.2f), new Vector2(8f, 8f));
            kit.Goal(new Vector3(0f, YCupula, 152.5f), 180f);
            kit.Bookshelf("Cupula fundo", new Vector3(-4.3f, YCupula, 154.5f), new Vector3(4.3f, YCupula, 154.5f), 4f);
            kit.Pole(new Vector3(-3.3f, YCupula, 147f), LuzQuente);
            kit.Pole(new Vector3(3.3f, YCupula, 147f), LuzQuente);
            kit.ReadingDesk("Mesa da cupula", new Vector3(3f, YCupula, 152.6f), 90f);
            kit.ReadingLamp(new Vector3(3f, YCupula + 0.8f, 152.2f), -90f);
            kit.ReadingLamp(new Vector3(-4.3f, YCupula + 4f, 154.5f), 0f, LuzEstrela);
            kit.ReadingLamp(new Vector3(4.3f, YCupula + 4f, 154.5f), 0f, LuzEstrela);

            // a lua: relógio gigante atrás da porta, visível desde o spawn
            kit.DreamClock("Lua do observatorio", new Vector3(0f, 21f, 162f), 5f, 0f, 1f);
        }

        // ------------------------------------------------------------------ céu estrelado (tudo fora do caminho)

        void Ceu(PhaseKit kit)
        {
            // poeira grande e alta como estrelas; a camada mais baixa fica a 15 m (o caminho chega a 9,6 + 4 m)
            kit.DreamMotes("Ceu estrelado", new Vector3(0f, 23f, 75f), new Vector3(36f, 6f, 175f), Estrelas, 900);
            kit.DreamMotes("Via lactea", new Vector3(0f, 17f, 125f), new Vector3(26f, 3f, 70f), new Color(0.80f, 0.75f, 1f), 400);

            // páginas distantes como estrelas maiores, entre o caminho e a moldura
            for (int k = 0; k < 8; k++)
            {
                float z = 5f + 20f * k;
                float y = 11f + (k % 3) * 3f;
                kit.FloatingPages($"Estrelas oeste {k + 1}", new Vector3(-11.5f, y, z), 2f, 8);
                kit.FloatingPages($"Estrelas leste {k + 1}", new Vector3(11.5f, y + 1.5f, z + 10f), 2f, 8);
            }

            kit.DreamClock("Relogio parado oeste", new Vector3(-11f, 9f, 45f), 2.5f, -90f, 2f);
            kit.DreamClock("Relogio parado leste", new Vector3(11f, 13f, 100f), 2.2f, 90f, 2f);
            kit.FloatingBooks("Constelacao de livros", new Vector3(7.5f, 5f, 30f), 2.5f, 7);
            kit.FloatingBooks("Livros sobre a galeria", new Vector3(-7.5f, 8f, 90f), 2.5f, 7);
        }
    }
}
