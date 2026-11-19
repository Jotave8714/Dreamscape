using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 11 — "Sala de Leitura Flutuante": a sala de leitura da biblioteca se partiu em ilhas de mesas,
    /// pilhas de livros e livros abertos gigantes que flutuam no vazio, sob a luz turquesa de abajures mágicos.
    /// Ideia central: "cada ilha é uma pausa; o vazio entre elas é o desafio".
    /// Percurso: ilha do despertar → pulos entre ilhas → mesa-ilha da serra → encruzilhada
    ///   (rota oeste: dois saltos de 5,5 m com a Pluma | rota leste: passarela do pêndulo e espinhos em ciclo)
    /// → ilha do encontro → torre de livros (mesas como degraus) → túnel coberto de estantes (laser e espinhos em ciclo,
    /// teto baixo; Escudo opcional antes) → ponte de páginas que cedem sob o pêndulo → porta.
    /// Só a Pluma (e o Escudo) aparecem aqui: os efeitos dos poderes se somam, e a Pluma pode durar até o fim da fase,
    /// então toda a geometria depois da encruzilhada foi checada contra ela.
    /// Pontos, vãos e contas em docs/Fase11.md.
    /// </summary>
    public sealed class Fase11Builder : IPhaseBuilder
    {
        public int Order => 11;
        public string SceneName => "Fase11_SalaDeLeituraFlutuante";
        public PhaseTheme Theme => Tema;

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        // segundo arco: noite alta, azul de lua; um pouco mais de cor que a Fase 10 (sat. 10, exp. 0,02).
        // Destaque turquesa (abajur mágico), diferente do verde da 9, do azul-céu da 10 e do roxo da 13.
        static readonly PhaseTheme Tema = new PhaseTheme
        {
            Name = "Fase11", Floor = C(0.30f, 0.36f, 0.46f), Wall = C(0.16f, 0.18f, 0.26f), Accent = C(0.20f, 1f, 0.88f),
            Sky = C(0.04f, 0.06f, 0.11f), Equator = C(0.09f, 0.12f, 0.18f), Ground = C(0.02f, 0.03f, 0.05f),
            Fog = C(0.06f, 0.09f, 0.15f), FogDensity = 0.016f, Sun = C(0.70f, 0.82f, 1f), SunIntensity = 0.95f,
            SunEuler = new Vector3(35f, 160f, 0f), Bloom = 0.7f, Vignette = 0.32f, Saturation = 10f, PostExposure = 0.02f,
            Tint = C(0.93f, 0.98f, 1f)
        };

        // alturas dos níveis
        const float Y0 = 0f;    // ilha do despertar
        const float Y1 = 0.5f;  // ilhas do meio (I2..I6, D1)
        const float YB = 1f;    // passarela leste (B2, B3)
        const float YD2 = 2.1f; // segundo andar da torre (+1,6, com a mesa no meio)
        const float YT = 3.7f;  // topo da torre, túnel, ponte e porta (+1,6)

        static readonly Color LuzLua = new Color(0.62f, 0.85f, 1f);
        static readonly Color LuzTurquesa = new Color(0.45f, 1f, 0.9f);
        static readonly Color LuzPluma = new Color(0.55f, 0.85f, 1f);

        public void Build(PhaseKit kit)
        {
            IlhaDoDespertar(kit);
            PrimeirasIlhas(kit);
            MesaIlhaDaSerra(kit);
            Encruzilhada(kit);
            RotaDaPluma(kit);
            RotaDasMesas(kit);
            IlhaDoEncontro(kit);
            TorreDeLivros(kit);
            TunelDeEstantes(kit);
            PonteFinal(kit);
            Cenario(kit);

            kit.FallZone(-8f);
            kit.LibraryBackdrop(new Vector2(-5f, -4f), new Vector2(13f, 102f), -10f, 22f, 6f);
        }

        // ------------------------------------------------------------------ I0: ilha do despertar (x -4..4, z -4..4)

        void IlhaDoDespertar(PhaseKit kit)
        {
            kit.BookPlatform("I0 Ilha do despertar", new Vector3(0f, Y0, 0f), new Vector2(8f, 8f), 0f, 2.5f);
            kit.Spawn(new Vector3(0f, Y0, -2f), 0f);

            kit.Bookshelf("Despertar fundo", new Vector3(-3.7f, Y0, -3.7f), new Vector3(3.7f, Y0, -3.7f), 3.5f);
            kit.ReadingDesk("Despertar oeste", new Vector3(-2.6f, Y0, 1f), 90f);
            kit.ReadingDesk("Despertar leste", new Vector3(2.6f, Y0, 1f), 90f);
            kit.ReadingLamp(new Vector3(-3.4f, Y0, -2.8f), 45f, LuzTurquesa);
            kit.ReadingLamp(new Vector3(3.4f, Y0, -2.8f), -45f, LuzTurquesa);
            kit.Carpet(new Vector3(0f, Y0, 0f), new Vector2(2.4f, 7f));

            kit.Sign("FASE 11", new Vector3(0f, 4.6f, 4.5f), 0f, 0.09f);
            kit.Sign("SALA DE LEITURA FLUTUANTE", new Vector3(0f, 4.1f, 4.5f), 0f, 0.07f);
            kit.Sign("cada ilha e uma pausa", new Vector3(0f, 3.4f, 4.5f), 0f, 0.035f);
        }

        // ------------------------------------------------------------------ I1, I2: pulos de 2,0 m e 2,2 m (+0,5)

        void PrimeirasIlhas(PhaseKit kit)
        {
            kit.BookPlatform("I1 Pilha baixa", new Vector3(0f, Y0, 7.5f), new Vector2(3f, 3f), 0f, 2f);     // z 6..9
            kit.BookPlatform("I2 Pilha alta", new Vector3(0f, Y1, 12.7f), new Vector2(3f, 3f), 0f, 2.5f);   // z 11,2..14,2
            kit.Carpet(new Vector3(0f, Y0, 7.5f), new Vector2(1.8f, 2.2f));
            kit.Carpet(new Vector3(0f, Y1, 12.7f), new Vector2(1.8f, 2.2f));
        }

        // ------------------------------------------------------------------ I3: livro aberto com a serra (x -3..3, z 16,5..23,5)

        void MesaIlhaDaSerra(PhaseKit kit)
        {
            kit.OpenBookPlatform("I3 Mesa-ilha", new Vector3(0f, Y1, 20f), new Vector2(6f, 7f));
            // a serra varre o livro em X (x -2,2..2,2), 3,2 s ida e volta; faixa de perigo z 19,44..21,56
            kit.Saw("Serra da mesa-ilha", new Vector3(-2.2f, Y1, 20.5f), new Vector3(4.4f, 0f, 0f), 3.2f, 0f);
            // mesas no fundo, fora da linha de pouso; deixam 2,6 m livres no meio para sair
            kit.ReadingDesk("Mesa-ilha oeste", new Vector3(-2.1f, Y1, 23f), 0f);
            kit.ReadingDesk("Mesa-ilha leste", new Vector3(2.1f, Y1, 23f), 0f);
            kit.ReadingLamp(new Vector3(-2.7f, Y1, 16.9f), 30f, LuzTurquesa);
            kit.ReadingLamp(new Vector3(2.7f, Y1, 16.9f), -30f, LuzTurquesa);
            kit.Sign("espere a serra passar", new Vector3(0f, Y1 + 3.2f, 16.3f), 0f, 0.04f);
        }

        // ------------------------------------------------------------------ I4: encruzilhada (x -5..5, z 25,5..32,5)

        void Encruzilhada(PhaseKit kit)
        {
            kit.BookPlatform("I4 Encruzilhada", new Vector3(0f, Y1, 29f), new Vector2(10f, 7f), 0f, 2.5f);
            // pedestal da Pluma a oeste (+0,5 m): rota curta e arriscada
            kit.BookPlatform("I4 Pedestal da Pluma", new Vector3(-3.5f, Y1 + 0.5f, 27.5f), new Vector2(2f, 2f), 0f, 0.5f);
            kit.Pickup(PickupKind.Gravity, new Vector3(-3.5f, Y1 + 0.5f, 27.5f));
            kit.ReadingDesk("Encruzilhada", new Vector3(3f, Y1, 25.3f), 0f);
            kit.Carpet(new Vector3(0f, Y1, 28.5f), new Vector2(2.4f, 5f));
            kit.ReadingLamp(new Vector3(-4.5f, Y1, 32f), 135f, LuzPluma);
            kit.ReadingLamp(new Vector3(4.5f, Y1, 32f), -135f, LuzTurquesa);

            kit.Sign("PLUMA: PELO VAZIO", new Vector3(-3.5f, Y1 + 3.4f, 29.6f), 0f, 0.04f);
            kit.Sign("MESAS: PELO PENDULO", new Vector3(4.6f, Y1 + 3.0f, 29f), 90f, 0.04f);
        }

        // ------------------------------------------------------------------ rota oeste: A1, dois saltos de 5,5 m com a Pluma

        void RotaDaPluma(PhaseKit kit)
        {
            kit.OpenBookPlatform("A1 Livro do meio do vazio", new Vector3(-3f, Y1, 40f), new Vector2(4f, 4f)); // x -5..-1, z 38..42
            kit.ReadingLamp(new Vector3(-4.6f, Y1, 41.6f), 135f, LuzPluma);
        }

        // ------------------------------------------------------------------ rota leste: B2 (pêndulo) e B3 (espinhos em ciclo)

        void RotaDasMesas(PhaseKit kit)
        {
            // B2: x 6,5..10,5, z 30..42, +0,5 sobre a encruzilhada
            kit.BookPlatform("B2 Passarela do pendulo", new Vector3(8.5f, YB, 36f), new Vector2(4f, 12f), 0f, 2.5f);
            kit.Pendulum("Pendulo da passarela", new Vector3(8.5f, YB, 36f), 50f, 2.6f, 0f, 0f);   // balança em X
            kit.BookPlatform("Viga do pendulo leste", new Vector3(8.5f, YB + 4.3f, 36f), new Vector2(9f, 0.6f), 0f, 0.5f); // x 4..13
            kit.BackdropShelf("Coluna da viga oeste", new Vector3(4f, -10f, 35.7f), new Vector3(4f, -10f, 36.3f), 10f + YB + 4.3f);
            kit.BackdropShelf("Coluna da viga leste", new Vector3(13f, -10f, 36.3f), new Vector3(13f, -10f, 35.7f), 10f + YB + 4.3f);
            kit.Carpet(new Vector3(8.5f, YB, 32f), new Vector2(2f, 2.6f));

            // B3: x 6,5..10,5, z 44..52; dois campos em ciclo cobrem a largura toda (fresta central de 0,2 m)
            kit.BookPlatform("B3 Ilha dos espinhos", new Vector3(8.5f, YB, 48f), new Vector2(4f, 8f), 0f, 2.5f);
            kit.Spikes("Ciclo leste A", new Vector3(7.5f, YB, 47.5f), true, 1.2f, 1.6f, 0f);
            kit.Spikes("Ciclo leste B", new Vector3(9.5f, YB, 47.5f), true, 1.2f, 1.6f, 0f);
            // estante a oeste antes dos espinhos: impede pular de B3 direto para a ilha do encontro sem passar por eles
            kit.Bookshelf("B3 Guarda", new Vector3(6.8f, YB, 44f), new Vector3(6.8f, YB, 48.6f), 3f);
            kit.Carpet(new Vector3(8.8f, YB, 45f), new Vector2(2.4f, 1.6f));
            kit.ReadingLamp(new Vector3(10.1f, YB, 51.6f), -135f, LuzTurquesa);
            kit.Sign("observe o ritmo", new Vector3(8.5f, YB + 2.4f, 43.7f), 0f, 0.035f);
        }

        // ------------------------------------------------------------------ I6: ilha do encontro (x -4..5, z 47,5..55,5)

        void IlhaDoEncontro(PhaseKit kit)
        {
            kit.BookPlatform("I6 Ilha do encontro", new Vector3(0.5f, Y1, 51.5f), new Vector2(9f, 8f), 0f, 2.5f);
            kit.Carpet(new Vector3(0.5f, Y1, 52.5f), new Vector2(2.4f, 5.4f));
            kit.ReadingLamp(new Vector3(-3.6f, Y1, 55.1f), 135f, LuzTurquesa);
            kit.ReadingLamp(new Vector3(4.6f, Y1, 55.1f), -135f, LuzTurquesa);
            kit.Sign("SUBA PELAS MESAS", new Vector3(0.5f, Y1 + 3.6f, 56f), 0f, 0.045f);
        }

        // ------------------------------------------------------------------ D1..D3: torre de livros; as mesas são os degraus

        void TorreDeLivros(PhaseKit kit)
        {
            // cada ilha sobe 1,6 m (impossível sem ajuda); a mesa na borda norte (0,8 m) divide o degrau em dois de 0,8 m
            kit.BookPlatform("D1 Base da torre", new Vector3(0.5f, Y1, 59.5f), new Vector2(4f, 4f), 0f, 3f);      // x -1,5..2,5, z 57,5..61,5
            kit.ReadingDesk("Degrau D1", new Vector3(0.5f, Y1, 61f), 0f);                                          // z 60,6..61,4, tampo 1,3
            kit.BookPlatform("D2 Torre de livros 1", new Vector3(0.5f, YD2, 63.5f), new Vector2(3f, 3f), 0f, 4f);  // z 62..65
            kit.ReadingDesk("Degrau D2", new Vector3(0.5f, YD2, 64.6f), 0f);                                       // z 64,2..65, tampo 2,9
            kit.BookPlatform("D3 Torre de livros 2", new Vector3(0.5f, YT, 67.5f), new Vector2(3f, 3f), 0f, 5.5f); // z 66..69
            kit.ReadingLamp(new Vector3(-1.1f, Y1, 57.9f), 45f, LuzTurquesa);

            // prateleira lateral com o Escudo (opcional, 1 m ao lado do topo da torre)
            kit.BookPlatform("E0 Prateleira do Escudo", new Vector3(4f, YT, 67.5f), new Vector2(2f, 2f), 0f, 1.5f); // x 3..5
            kit.Pickup(PickupKind.Shield, new Vector3(4f, YT, 67.5f));
            kit.Carpet(new Vector3(0.5f, YT, 67.5f), new Vector2(1.8f, 2.2f));
        }

        // ------------------------------------------------------------------ E1: túnel coberto (x -1,45..2,45, z 71..83)

        void TunelDeEstantes(PhaseKit kit)
        {
            kit.BookPlatform("E1 Piso do tunel", new Vector3(0.5f, YT, 77f), new Vector2(3.9f, 12f), 0f, 2f);
            // estantes de 3,4 m (faces internas em x -0,85 e 1,85: 2,7 m livres, a largura do feixe)
            kit.Bookshelf("Tunel oeste", new Vector3(-1.15f, YT, 71.3f), new Vector3(-1.15f, YT, 82.7f), 3.4f);
            kit.Bookshelf("Tunel leste", new Vector3(2.15f, YT, 82.7f), new Vector3(2.15f, YT, 71.3f), 3.4f);
            // teto de livros a 2,3 m do piso (z 73,5..82,7): ninguém salta o laser nem os espinhos, nem com a Pluma
            kit.BookPlatform("E1 Teto baixo", new Vector3(0.5f, YT + 2.8f, 78.1f), new Vector2(2.7f, 9.2f), 0f, 0.5f);

            kit.Laser("Laser do tunel", new Vector3(0.5f, YT, 75.6f), 0f, 1.4f, 1.8f, 0f);
            kit.Spikes("Ciclo do tunel", new Vector3(0.5f, YT, 80.1f), true, 1.2f, 1.6f, 0f);
            kit.Carpet(new Vector3(0.5f, YT, 77.45f), new Vector2(1.8f, 2.4f));   // refúgio entre o laser e os espinhos

            kit.CeilingLight(new Vector3(0.5f, YT + 2.3f, 77.4f), 0f, LuzTurquesa);
            kit.CeilingLight(new Vector3(0.5f, YT + 2.3f, 82.3f), 0f, LuzTurquesa);
            kit.Sign("teto baixo: espere o feixe", new Vector3(0.5f, YT + 3.0f, 70.8f), 0f, 0.035f);
        }

        // ------------------------------------------------------------------ ponte de quatro páginas que cedem, pêndulo na terceira → porta

        void PonteFinal(PhaseKit kit)
        {
            for (int i = 0; i < 4; i++)   // z 83,8..85,8 / 86,6..88,6 / 89,4..91,4 / 92,2..94,2, vãos de 0,8 m
                kit.CrumblingPlatform($"Pagina que cede {i + 1}", new Vector3(0.5f, YT, 84.8f + 2.8f * i), new Vector2(2f, 2f), 0.8f);

            kit.Pendulum("Pendulo da ponte", new Vector3(0.5f, YT, 90.4f), 55f, 2.4f, 0f, 0f);   // balança em X
            kit.BookPlatform("Viga da ponte", new Vector3(0.5f, YT + 4.3f, 90.4f), new Vector2(9f, 0.6f), 0f, 0.5f); // x -4..5
            kit.BackdropShelf("Coluna da ponte oeste", new Vector3(-4f, -10f, 90.1f), new Vector3(-4f, -10f, 90.7f), 10f + YT + 4.3f);
            kit.BackdropShelf("Coluna da ponte leste", new Vector3(5f, -10f, 90.7f), new Vector3(5f, -10f, 90.1f), 10f + YT + 4.3f);

            kit.OpenBookPlatform("G Patamar da porta", new Vector3(0.5f, YT, 98.5f), new Vector2(8f, 7f)); // x -3,5..4,5, z 95..102
            kit.Carpet(new Vector3(0.5f, YT, 98f), new Vector2(2.4f, 4.6f));
            kit.ReadingLamp(new Vector3(-3f, YT, 101.5f), 135f, LuzTurquesa);
            kit.ReadingLamp(new Vector3(4f, YT, 101.5f), -135f, LuzTurquesa);
            kit.Pole(new Vector3(-2.8f, YT, 95.5f), LuzLua);
            kit.Pole(new Vector3(3.8f, YT, 95.5f), LuzLua);
            kit.Goal(new Vector3(0.5f, YT, 100.7f), 180f);
            kit.Sign("nao pare na ponte", new Vector3(0.5f, YT + 2.5f, 83.3f), 0f, 0.04f);
        }

        // ------------------------------------------------------------------ cenário sem colisão, fora do volume jogável

        void Cenario(PhaseKit kit)
        {
            // marco do fim: relógio alto atrás da porta, visível por cima de toda a fase
            kit.DreamClock("Relogio da porta", new Vector3(0.5f, 13f, 106f), 3.5f);
            kit.DreamClock("Relogio da encruzilhada", new Vector3(-8f, 9f, 32f), 2.2f, -60f, -3f);

            // livros e páginas no vazio, abaixo das ilhas ou a mais de 3 m das bordas
            kit.FloatingBooks("Abaixo de I1", new Vector3(0f, -4f, 10f), 2.2f, 6);
            kit.FloatingBooks("Vazio oeste", new Vector3(-8f, 2.5f, 18f), 1.8f, 6);
            kit.FloatingBooks("Entre as rotas", new Vector3(2f, -3.5f, 40f), 2.2f, 7);
            kit.FloatingBooks("Vazio leste", new Vector3(16f, 3f, 46f), 2f, 6);
            kit.FloatingBooks("Abaixo da torre", new Vector3(-5f, -2f, 70f), 2f, 6);
            kit.FloatingBooks("Ao lado do tunel", new Vector3(7f, 5f, 77f), 1.8f, 5);
            kit.FloatingPages("Sobre a encruzilhada", new Vector3(0f, 7.5f, 29f), 2.5f, 12);
            kit.FloatingPages("Rastro da Pluma", new Vector3(-3f, 7.5f, 40f), 2f, 10);
            kit.FloatingPages("Sobre a porta", new Vector3(0.5f, 10.5f, 98.5f), 2.5f, 12);
            kit.DreamMotes("Despertar", new Vector3(0f, 2.5f, 0f), new Vector3(7f, 4f, 7f), null, 80);
            kit.DreamMotes("Vazio central", new Vector3(3f, -4f, 50f), new Vector3(18f, 6f, 40f), LuzTurquesa, 200);
        }
    }
}
