using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 12 — "Arquivo das Lembranças": o arquivo morto da biblioteca, onde as lembranças do estudante ficam
    /// guardadas em fichas. Prensas de carimbo descem em ritmo fixo e as fichas velhas (pisos que cedem) se apagam
    /// sob os pés. Ideia central: "o arquivo carimba no ritmo; a lembrança some se você parar".
    /// Percurso: recepção (ficha apagada sobre um fosso raso, sem risco) → corredor da primeira prensa
    /// → corredor de fichas que cedem → patamar (Escudo opcional) → galeria das três prensas em onda
    /// → sala do esquecimento (Velocidade, serra) → vão de 4,4 m e ponte de fichas → patamar do pulo duplo
    /// → escada de fichas que cedem (+1,6 m cada) → corredor alto (prensa, espinhos sob teto baixo, prensa)
    /// → ponte final com serra sob teto baixo → porta. Pontos, vãos e contas em docs/Fase12.md.
    /// </summary>
    public sealed class Fase12Builder : IPhaseBuilder
    {
        public int Order => 12;
        public string SceneName => "Fase12_ArquivoDasLembrancas";
        public PhaseTheme Theme => Tema;

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        // segundo arco: fim da madrugada, sépia de fotografia antiga (sat. 12, exp. 0,05), um passo antes do roxo da 13.
        // Destaque rosa-lembrança, diferente do turquesa da 11 e do violeta da 13.
        static readonly PhaseTheme Tema = new PhaseTheme
        {
            Name = "Fase12", Floor = C(0.48f, 0.38f, 0.34f), Wall = C(0.24f, 0.18f, 0.18f), Accent = C(1f, 0.46f, 0.60f),
            Sky = C(0.08f, 0.06f, 0.09f), Equator = C(0.17f, 0.12f, 0.13f), Ground = C(0.04f, 0.03f, 0.03f),
            Fog = C(0.13f, 0.09f, 0.11f), FogDensity = 0.02f, Sun = C(1f, 0.82f, 0.75f), SunIntensity = 1.0f,
            SunEuler = new Vector3(42f, -120f, 0f), Bloom = 0.75f, Vignette = 0.32f, Saturation = 12f, PostExposure = 0.05f,
            Tint = C(1f, 0.95f, 0.93f)
        };

        const float Y0 = 0f;    // recepção, primeira prensa, fichas que cedem
        const float YL = 0.8f;  // patamar, galeria das prensas, sala do esquecimento, ponte, patamar do pulo duplo
        const float YC1 = 2.4f; // escada de fichas (+1,6)
        const float YC2 = 4.0f; // (+1,6)
        const float YK = 5.6f;  // corredor alto, ponte final e porta (+1,6)

        static readonly Color LuzSepia = new Color(1f, 0.78f, 0.6f);
        static readonly Color LuzRosa = new Color(1f, 0.6f, 0.7f);
        static readonly Color LuzVelocidade = new Color(1f, 0.82f, 0.35f);

        public void Build(PhaseKit kit)
        {
            Recepcao(kit);
            PrimeiraPrensa(kit);
            FichasQueCedem(kit);
            Patamar(kit);
            GaleriaDasPrensas(kit);
            SalaDoEsquecimento(kit);
            PonteDeFichas(kit);
            EscadaDoPuloDuplo(kit);
            CorredorAlto(kit);
            PonteFinal(kit);
            Cenario(kit);

            kit.FallZone(-8f);
            kit.LibraryBackdrop(new Vector2(-5f, -4f), new Vector2(5f, 115f), -8f, 22f, 6f);
        }

        // ------------------------------------------------------------------ R0: recepção (x -4..4, z -4..4) com a ficha apagada

        void Recepcao(PhaseKit kit)
        {
            kit.Platform("R0 Piso", new Vector3(1f, Y0, 0f), new Vector2(6f, 8f));                 // x -2..4
            kit.Platform("R0 Canto", new Vector3(-3f, Y0, -2.75f), new Vector2(2f, 2.5f));         // z -4..-1,5
            kit.CrumblingPlatform("R0 Ficha apagada", new Vector3(-3f, Y0, -0.5f), new Vector2(2f, 2f), 0.8f); // z -1,5..0,5
            kit.Platform("R0 Frente", new Vector3(-3f, Y0, 2.25f), new Vector2(2f, 3.5f));         // z 0,5..4
            kit.Platform("F0 Fosso", new Vector3(-3f, Y0 - 0.8f, -0.5f), new Vector2(2f, 2f));     // seguro: sai-se com 0,8 m
            kit.Spawn(new Vector3(1f, Y0, -1.5f), 0f);

            // a estante oeste desce até o fundo do fosso e o fecha por fora
            kit.Bookshelf("Recepcao oeste", new Vector3(-4.3f, Y0 - 0.8f, -4.3f), new Vector3(-4.3f, Y0 - 0.8f, 4f), 4.8f);
            kit.Bookshelf("Recepcao leste", new Vector3(4.3f, Y0, 4f), new Vector3(4.3f, Y0, -4.3f), 4f);
            kit.Bookshelf("Recepcao fundo", new Vector3(-4f, Y0, -4.3f), new Vector3(4f, Y0, -4.3f), 4f);
            kit.Bookshelf("Recepcao frente oeste", new Vector3(-4f, Y0, 3.7f), new Vector3(-1.4f, Y0, 3.7f), 4f);
            kit.Bookshelf("Recepcao frente leste", new Vector3(1.4f, Y0, 3.7f), new Vector3(4f, Y0, 3.7f), 4f);
            // portal de 1,6 m: as laterais (x ±0,8..±1,4) continuam as estantes do corredor
            kit.ShelfArch("Saida da recepcao", new Vector3(0f, Y0, 3.7f), 1.6f, 3.4f, 0f);

            kit.ReadingDesk("Balcao do arquivista", new Vector3(3f, Y0, -2.5f), 90f);
            kit.ReadingLamp(new Vector3(3.7f, Y0, -3.7f), -45f, LuzSepia);
            kit.ReadingLamp(new Vector3(-1.6f, Y0, -3.7f), 45f, LuzSepia);
            kit.Carpet(new Vector3(1f, Y0, 0f), new Vector2(2.4f, 6f));
            kit.CeilingLight(new Vector3(1f, 4.2f, 0f), 0f, LuzSepia);

            kit.Sign("FASE 12", new Vector3(0f, 4.8f, 3.3f), 0f, 0.09f);
            kit.Sign("ARQUIVO DAS LEMBRANCAS", new Vector3(0f, 4.3f, 3.3f), 0f, 0.07f);
            kit.Sign("o arquivo carimba no ritmo", new Vector3(0f, 3.75f, 3.3f), 0f, 0.035f);
            kit.Sign("ficha escura se apaga", new Vector3(-3.95f, 2.4f, -0.5f), -90f, 0.035f);
        }

        // ------------------------------------------------------------------ K1: corredor da primeira prensa (z 4..14, 1,6 m livres)

        void PrimeiraPrensa(PhaseKit kit)
        {
            kit.Platform("K1 Corredor da prensa", new Vector3(0f, Y0, 9f), new Vector2(2.2f, 10f));
            kit.Bookshelf("K1 oeste", new Vector3(-1.1f, Y0, 4.3f), new Vector3(-1.1f, Y0, 14f), 3.2f);
            kit.Bookshelf("K1 leste", new Vector3(1.1f, Y0, 14f), new Vector3(1.1f, Y0, 4.3f), 3.2f);
            // ciclo de 4,0 s: 2,2 s erguida, 0,2 s descendo, 0,8 s embaixo, 0,8 s subindo
            kit.Press("Prensa de carimbo", new Vector3(0f, Y0, 9.5f), 2.2f, 0.8f, 0f, 0f);
            kit.CeilingLight(new Vector3(0f, 3.2f, 5.5f), 0f, LuzSepia);
            kit.CeilingLight(new Vector3(0f, 3.2f, 13.3f), 0f, LuzRosa);
        }

        // ------------------------------------------------------------------ F1: quatro fichas que cedem, sem pausa, sobre o vazio (z 14..22)

        void FichasQueCedem(PhaseKit kit)
        {
            for (int i = 0; i < 4; i++)
                kit.CrumblingPlatform($"Ficha que cede {i + 1}", new Vector3(0f, Y0, 15f + 2f * i), new Vector2(2.2f, 2f), 0.8f);
            // as estantes descem no vazio para parecerem apoiadas em algo
            kit.Bookshelf("F1 oeste", new Vector3(-1.1f, Y0 - 3f, 14f), new Vector3(-1.1f, Y0 - 3f, 22f), 6.2f);
            kit.Bookshelf("F1 leste", new Vector3(1.1f, Y0 - 3f, 22f), new Vector3(1.1f, Y0 - 3f, 14f), 6.2f);
            kit.Sign("CORRA: AS FICHAS SE APAGAM", new Vector3(0f, 2.9f, 13.6f), 0f, 0.035f);
        }

        // ------------------------------------------------------------------ L1: patamar (+0,8; x -3..3, z 22..28) com o Escudo opcional

        void Patamar(PhaseKit kit)
        {
            kit.BookPlatform("L1 Patamar", new Vector3(0f, YL, 25f), new Vector2(6f, 6f), 0f, 3f);
            kit.Pickup(PickupKind.Shield, new Vector3(2.2f, YL, 24.3f));   // fora da linha central: opcional
            kit.Bookshelf("Patamar oeste", new Vector3(-3.3f, YL, 22f), new Vector3(-3.3f, YL, 28f), 3.5f);
            kit.Bookshelf("Patamar leste", new Vector3(3.3f, YL, 28f), new Vector3(3.3f, YL, 22f), 3.5f);
            kit.Bookshelf("Patamar frente oeste", new Vector3(-3f, YL, 27.7f), new Vector3(-1.4f, YL, 27.7f), 3.5f);
            kit.Bookshelf("Patamar frente leste", new Vector3(1.4f, YL, 27.7f), new Vector3(3f, YL, 27.7f), 3.5f);
            kit.ShelfArch("Entrada da galeria", new Vector3(0f, YL, 27.7f), 1.6f, 3.3f, 0f);
            kit.Carpet(new Vector3(0f, YL, 24.8f), new Vector2(2.4f, 4.6f));
            kit.ReadingLamp(new Vector3(-2.6f, YL, 22.6f), 45f, LuzSepia);
            kit.ReadingLamp(new Vector3(-2.6f, YL, 27f), 135f, LuzSepia);
            kit.CeilingLight(new Vector3(0f, YL + 3.4f, 25f), 0f, LuzRosa);
            kit.Sign("tres prensas: espere entre elas", new Vector3(0f, YL + 3.75f, 27.0f), 0f, 0.035f);
        }

        // ------------------------------------------------------------------ K3: galeria das três prensas em onda (z 28..43)

        void GaleriaDasPrensas(PhaseKit kit)
        {
            kit.Platform("K3 Galeria das prensas", new Vector3(0f, YL, 35.5f), new Vector2(2.2f, 15f));
            kit.Bookshelf("K3 oeste", new Vector3(-1.1f, YL, 28.3f), new Vector3(-1.1f, YL, 43f), 3.2f);
            kit.Bookshelf("K3 leste", new Vector3(1.1f, YL, 43f), new Vector3(1.1f, YL, 28.3f), 3.2f);
            // mesmo ciclo de 4,0 s; descem uma depois da outra (perigo em t 0..1, 1,3..2,3 e 2,6..3,6 do ciclo)
            kit.Press("Prensa da onda 1", new Vector3(0f, YL, 31.5f), 2.2f, 0.8f, 2.2f, 0f);
            kit.Press("Prensa da onda 2", new Vector3(0f, YL, 35.5f), 2.2f, 0.8f, 0.9f, 0f);
            kit.Press("Prensa da onda 3", new Vector3(0f, YL, 39.5f), 2.2f, 0.8f, 3.6f, 0f);
            // refúgios (z 32,58..34,42 e 36,58..38,42) marcados por tapetes
            kit.Carpet(new Vector3(0f, YL, 33.5f), new Vector2(1.4f, 1.4f));
            kit.Carpet(new Vector3(0f, YL, 37.5f), new Vector2(1.4f, 1.4f));
            kit.CeilingLight(new Vector3(0f, YL + 3.2f, 33.5f), 0f, LuzRosa);
            kit.CeilingLight(new Vector3(0f, YL + 3.2f, 37.5f), 0f, LuzRosa);
        }

        // ------------------------------------------------------------------ M1: sala do esquecimento (x -4..4, z 43..51) com Velocidade e serra

        void SalaDoEsquecimento(PhaseKit kit)
        {
            kit.Platform("M1 Sala do esquecimento", new Vector3(0f, YL, 47f), new Vector2(8f, 8f));
            kit.Bookshelf("Esquecimento oeste", new Vector3(-4.3f, YL, 43f), new Vector3(-4.3f, YL, 51f), 4f);
            kit.Bookshelf("Esquecimento leste", new Vector3(4.3f, YL, 51f), new Vector3(4.3f, YL, 43f), 4f);
            kit.Bookshelf("Esquecimento sul oeste", new Vector3(-4f, YL, 43.3f), new Vector3(-1.4f, YL, 43.3f), 4f);
            kit.Bookshelf("Esquecimento sul leste", new Vector3(1.4f, YL, 43.3f), new Vector3(4f, YL, 43.3f), 4f);

            kit.Pickup(PickupKind.Speed, new Vector3(0f, YL, 44.8f));
            // a serra varre a sala em X (x -3..3), 3 s ida e volta; faixa de perigo z 46,44..48,56
            kit.Saw("Serra do esquecimento", new Vector3(-3f, YL, 47.5f), new Vector3(6f, 0f, 0f), 3f, 0f);
            // mesas no fundo da sala, longe da borda norte (não servem de trampolim para o vão)
            kit.ReadingDesk("Esquecimento oeste", new Vector3(-3f, YL, 44.4f), 0f);
            kit.ReadingDesk("Esquecimento leste", new Vector3(3f, YL, 44.4f), 0f);
            kit.Carpet(new Vector3(0f, YL, 49.8f), new Vector2(2.4f, 2f));
            kit.CeilingLight(new Vector3(0f, YL + 3.8f, 45f), 0f, LuzVelocidade);
            kit.CeilingLight(new Vector3(0f, YL + 3.8f, 50f), 0f, LuzRosa);
            kit.Sign("VELOCIDADE: SALTE E NAO PARE", new Vector3(0f, YL + 3.3f, 51.4f), 0f, 0.04f);
        }

        // ------------------------------------------------------------------ vão de 4,4 m (Velocidade) e quatro fichas que cedem até N1

        void PonteDeFichas(PhaseKit kit)
        {
            for (int i = 0; i < 4; i++)   // z 55,4..57,4 / 58..60 / 60,6..62,6 / 63,2..65,2
                kit.CrumblingPlatform($"Ponte de fichas {i + 1}", new Vector3(0f, YL, 56.4f + 2.6f * i), new Vector2(2.4f, 2f), 0.8f);
        }

        // ------------------------------------------------------------------ N1 (pulo duplo) e escada de fichas que cedem, +1,6 m cada

        void EscadaDoPuloDuplo(PhaseKit kit)
        {
            kit.BookPlatform("N1 Patamar do pulo duplo", new Vector3(0f, YL, 69f), new Vector2(6f, 6.4f), 0f, 3f); // z 65,8..72,2
            kit.Pickup(PickupKind.DoubleJump, new Vector3(0f, YL, 68.6f));
            kit.Carpet(new Vector3(0f, YL, 69.5f), new Vector2(2.4f, 4.6f));
            kit.ReadingLamp(new Vector3(-2.6f, YL, 66.3f), 45f, LuzSepia);
            kit.ReadingLamp(new Vector3(2.6f, YL, 66.3f), -45f, LuzSepia);
            kit.Sign("PULE DUAS VEZES, SEM DEMORA", new Vector3(0f, YK + 3.2f, 72.6f), 0f, 0.045f);

            kit.CrumblingPlatform("Degrau de ficha 1", new Vector3(0f, YC1, 74.5f), new Vector2(2.4f, 3f), 0.8f); // z 73..76
            kit.CrumblingPlatform("Degrau de ficha 2", new Vector3(0f, YC2, 78.5f), new Vector2(2.4f, 3f), 0.8f); // z 77..80
        }

        // ------------------------------------------------------------------ K4: corredor alto (z 81..96): prensa, espinhos sob teto baixo, prensa

        void CorredorAlto(PhaseKit kit)
        {
            kit.Platform("K4 Corredor alto", new Vector3(0f, YK, 88.5f), new Vector2(2.2f, 15f));
            // as estantes começam em z 82: o pouso do pulo duplo (z 81..82) fica sem paredes
            kit.Bookshelf("K4 oeste", new Vector3(-1.1f, YK, 82f), new Vector3(-1.1f, YK, 96f), 3.4f);
            kit.Bookshelf("K4 leste", new Vector3(1.1f, YK, 96f), new Vector3(1.1f, YK, 82f), 3.4f);

            // ciclo de 3,8 s (2,0 erguida, 0,2 descendo, 0,8 embaixo, 0,8 subindo); a B está meio ciclo à frente
            kit.Press("Prensa alta A", new Vector3(0f, YK, 85.5f), 2f, 0.8f, 0f, 0f);
            kit.Spikes("Ciclo do corredor alto", new Vector3(0f, YK, 89f), true, 1.2f, 1.6f, 0f);
            kit.Press("Prensa alta B", new Vector3(0f, YK, 92.5f), 2f, 0.8f, 1.9f, 0f);
            // teto de fichas a 2,3 m sobre os espinhos e os dois refúgios (z 86,6..91,4): ninguém salta os espinhos
            kit.BookPlatform("K4 Teto baixo", new Vector3(0f, YK + 2.8f, 89f), new Vector2(1.6f, 4.8f), 0f, 0.5f);

            kit.Carpet(new Vector3(0f, YK, 87.2f), new Vector2(1.4f, 1f));
            kit.Carpet(new Vector3(0f, YK, 90.8f), new Vector2(1.4f, 1f));
            kit.CeilingLight(new Vector3(0f, YK + 3.4f, 82.5f), 0f, LuzRosa);
            kit.CeilingLight(new Vector3(0f, YK + 3.4f, 95f), 0f, LuzRosa);
        }

        // ------------------------------------------------------------------ ponte final: quatro fichas que cedem sob teto baixo, com serra → porta

        void PonteFinal(PhaseKit kit)
        {
            for (int i = 0; i < 4; i++)   // z 96,6..98,6 / 99,2..101,2 / 101,8..103,8 / 104,4..106,4
                kit.CrumblingPlatform($"Ficha final {i + 1}", new Vector3(0f, YK, 97.6f + 2.6f * i), new Vector2(2.4f, 2f), 0.8f);
            // a serra varre a segunda ficha em X (x -2,5..2,5), 2,4 s ida e volta; faixa de perigo z 99,14..101,26
            kit.Saw("Serra da ponte final", new Vector3(-2.5f, YK, 100.2f), new Vector3(5f, 0f, 0f), 2.4f, 0f);
            // teto de fichas a 2,3 m (z 96,6..106,4): com Velocidade e pulo duplo ainda ativos, ninguém salta a serra
            // nem atravessa a ponte num pulo só
            kit.BookPlatform("Teto da ponte final", new Vector3(0f, YK + 2.8f, 101.5f), new Vector2(2.4f, 9.8f), 0f, 0.5f);

            kit.OpenBookPlatform("G Patamar da porta", new Vector3(0f, YK, 111f), new Vector2(8f, 8f)); // x -4..4, z 107..115
            kit.Carpet(new Vector3(0f, YK, 110.2f), new Vector2(2.4f, 5f));
            kit.ReadingLamp(new Vector3(-3.4f, YK, 114.4f), 135f, LuzRosa);
            kit.ReadingLamp(new Vector3(3.4f, YK, 114.4f), -135f, LuzRosa);
            kit.Bookshelf("Porta fundo oeste", new Vector3(-4f, YK, 115.3f), new Vector3(-1.2f, YK, 115.3f), 4f);
            kit.Bookshelf("Porta fundo leste", new Vector3(1.2f, YK, 115.3f), new Vector3(4f, YK, 115.3f), 4f);
            kit.CeilingLight(new Vector3(0f, YK + 4f, 110.6f), 0f, LuzSepia);
            kit.Goal(new Vector3(0f, YK, 113.4f), 180f);
            kit.Sign("espere a serra antes de pisar", new Vector3(0f, YK + 3.6f, 96.2f), 0f, 0.035f);
        }

        // ------------------------------------------------------------------ cenário sem colisão, fora do volume jogável

        void Cenario(PhaseKit kit)
        {
            // fileiras do arquivo morto dos dois lados (x ±5: 1,5 m ou mais além de qualquer piso dos trechos)
            kit.BackdropShelf("Arquivo oeste 1", new Vector3(-5.5f, -6f, 5f), new Vector3(-5.5f, -6f, 42f), 16f);
            kit.BackdropShelf("Arquivo leste 1", new Vector3(5.5f, -6f, 42f), new Vector3(5.5f, -6f, 5f), 16f);
            kit.BackdropShelf("Arquivo oeste 2", new Vector3(-5f, -6f, 53f), new Vector3(-5f, -6f, 103f), 20f);
            kit.BackdropShelf("Arquivo leste 2", new Vector3(5f, -6f, 103f), new Vector3(5f, -6f, 53f), 20f);

            // marco do fim: relógio grande acima da porta (lembrança de uma hora parada)
            kit.DreamClock("Relogio da lembranca", new Vector3(0f, 16f, 120f), 4f, 0f, 2f);
            kit.DreamClock("Relogio da sala", new Vector3(-2.5f, 8.5f, 47f), 1.8f, 0f, -4f);

            // fichas (páginas) e livros voando: acima de 5 m do piso ou abaixo das pontes
            kit.FloatingPages("Sobre a galeria", new Vector3(0f, YL + 6f, 35.5f), 1.5f, 14);
            kit.FloatingPages("Sobre a ponte", new Vector3(0f, YL + 6.5f, 60f), 2f, 12);
            kit.FloatingPages("Sobre a porta", new Vector3(0f, YK + 6f, 111f), 2.5f, 12);
            kit.FloatingBooks("Abaixo das fichas", new Vector3(0f, -4f, 18f), 2f, 6);
            kit.FloatingBooks("Abaixo da ponte", new Vector3(0f, -3.5f, 60f), 2.4f, 7);
            kit.FloatingBooks("Abaixo da escada", new Vector3(0f, -2f, 78f), 2f, 6);
            kit.DreamMotes("Recepcao", new Vector3(0f, 2.5f, 0f), new Vector3(7f, 4f, 7f), null, 80);
            kit.DreamMotes("Vazio das pontes", new Vector3(0f, -4f, 80f), new Vector3(10f, 5f, 50f), LuzRosa, 180);
        }
    }
}
