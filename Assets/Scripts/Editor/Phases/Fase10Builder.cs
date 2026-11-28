using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 10 — "Escadaria de Livros": subida vertical em espiral (sentido anti-horário visto de cima) em volta de uma
    /// torre de estantes de 19 m, com um relógio no topo. Regra da fase: cada volta tem uma regra própria e cada nova
    /// volta passa 3,5 m ou mais acima da anterior, então não há atalho entre voltas.
    ///   Volta 1 (sem poder): escada de livros, serra no corredor leste, livro que cede.
    ///   Volta 2 (Pulo duplo): degraus de +1,7 m sobre livros com espinhos em ciclo; um coletável em cada quina.
    ///   Volta 3: corredor fechado com duas prensas, escada de livros que cedem, Pluma e salto para a ilha da porta.
    /// A pista tem 3 m de largura: quinas de 3 x 3 m e trechos do meio de 4 m. Contas em docs/Fase10.md.
    /// </summary>
    public sealed class Fase10Builder : IPhaseBuilder
    {
        public int Order => 10;
        public string SceneName => "Fase10_EscadariaDeLivros";
        public PhaseTheme Theme => Tema;

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        // meio da noite indo para a madrugada; destaque azul-celeste
        static readonly PhaseTheme Tema = new PhaseTheme
        {
            Name = "Fase10", Floor = C(0.40f, 0.34f, 0.30f), Wall = C(0.18f, 0.18f, 0.26f), Accent = C(0.45f, 0.72f, 1f),
            Sky = C(0.06f, 0.07f, 0.13f), Equator = C(0.12f, 0.13f, 0.22f), Ground = C(0.03f, 0.03f, 0.05f),
            Fog = C(0.08f, 0.10f, 0.18f), FogDensity = 0.026f, Sun = C(0.75f, 0.85f, 1f), SunIntensity = 0.95f,
            SunEuler = new Vector3(55f, 20f, 0f), Bloom = 0.7f, Vignette = 0.33f, Saturation = 8f, PostExposure = 0f,
            Tint = C(0.94f, 0.97f, 1f)
        };

        // centros das quinas (SO, SE, NE, NO) e dos trechos do meio (S, L, N, O) da pista
        static readonly Vector2 SO = new Vector2(-3.5f, -3.5f), SE = new Vector2(3.5f, -3.5f);
        static readonly Vector2 NE = new Vector2(3.5f, 3.5f), NO = new Vector2(-3.5f, 3.5f);

        const float AlturaTorre = 19f;
        static readonly Color LuzPluma = new Color(0.55f, 0.85f, 1f);
        static readonly Color LuzAlerta = new Color(1f, 0.45f, 0.35f);

        static Vector3 P(Vector2 xz, float y) => new Vector3(xz.x, y, xz.y);

        public void Build(PhaseKit kit)
        {
            Torre(kit);
            Inicio(kit);
            Volta1(kit);
            Volta2(kit);
            Volta3(kit);
            IlhaDaPorta(kit);
            Cenario(kit);

            kit.FallZone(-6f);
            kit.LibraryBackdrop(new Vector2(-16.5f, -11f), new Vector2(5f, 6.5f), -4f, 30f, 6f);
        }

        // ------------------------------------------------------------------ torre central (x -2..2, z -2..2), sólida

        void Torre(PhaseKit kit)
        {
            kit.Bookshelf("Torre sul", new Vector3(-2f, 0f, -1.7f), new Vector3(2f, 0f, -1.7f), AlturaTorre);
            kit.Bookshelf("Torre leste", new Vector3(1.7f, 0f, -1.4f), new Vector3(1.7f, 0f, 1.4f), AlturaTorre);
            kit.Bookshelf("Torre norte", new Vector3(2f, 0f, 1.7f), new Vector3(-2f, 0f, 1.7f), AlturaTorre);
            kit.Bookshelf("Torre oeste", new Vector3(-1.7f, 0f, 1.4f), new Vector3(-1.7f, 0f, -1.4f), AlturaTorre);
            kit.BookPlatform("Topo da torre", new Vector3(0f, AlturaTorre + 0.5f, 0f), new Vector2(4.4f, 4.4f), 0f, 1f);
        }

        // ------------------------------------------------------------------ S0: início (x -6,5..-0,5, z -11..-6)

        void Inicio(PhaseKit kit)
        {
            kit.BookPlatform("S0 Inicio", new Vector3(-3.5f, 0f, -8.5f), new Vector2(6f, 5f));
            kit.Spawn(new Vector3(-3.5f, 0f, -9.3f), 0f);
            kit.Carpet(new Vector3(-3.5f, 0f, -8.5f), new Vector2(2.4f, 4f));
            kit.ReadingLamp(new Vector3(-6f, 0f, -10.5f), 45f);
            kit.ReadingLamp(new Vector3(-1f, 0f, -10.5f), -45f);
            kit.Pole(new Vector3(-6f, 0f, -6.5f));
            kit.Sign("FASE 10", new Vector3(-3.5f, 3.6f, -6.2f), 0f, 0.09f);
            kit.Sign("ESCADARIA DE LIVROS", new Vector3(-3.5f, 3.1f, -6.2f), 0f, 0.07f);
            kit.Sign("suba em volta da torre", new Vector3(-3.5f, 2.6f, -6.2f), 0f, 0.04f);
        }

        // ------------------------------------------------------------------ volta 1 (0,5 -> 4,0), sem poder

        void Volta1(PhaseKit kit)
        {
            kit.BookPlatform("V1 Quina SO", P(SO, 0.5f), new Vector2(3f, 3f));
            // escada de livros contígua: +0,5 a cada degrau
            kit.BookPlatform("V1 Degrau S1", new Vector3(-1f, 1.0f, -3.5f), new Vector2(2f, 3f));
            kit.BookPlatform("V1 Degrau S2", new Vector3(1f, 1.5f, -3.5f), new Vector2(2f, 3f));
            kit.BookPlatform("V1 Quina SE", P(SE, 2.0f), new Vector2(3f, 3f));

            // trecho leste (x 2..5, z -2..2): a serra vai e volta de x 2,8 a 4,2; passe pelo lado oposto a ela
            kit.BookPlatform("V1 Corredor da serra", new Vector3(3.5f, 2.0f, 0f), new Vector2(3f, 4f));
            kit.Saw("Serra da volta 1", new Vector3(2.8f, 2.0f, 0f), new Vector3(1.4f, 0f, 0f), 3f);

            kit.BookPlatform("V1 Quina NE", P(NE, 2.5f), new Vector2(3f, 3f));
            // trecho norte: um único livro que cede, com vãos de 1 m dos dois lados
            kit.CrumblingPlatform("V1 Livro que cede", new Vector3(0f, 3.0f, 3.5f), new Vector2(2f, 3f), 0.8f);
            kit.BookPlatform("V1 Quina NO", P(NO, 3.5f), new Vector2(3f, 3f));
            kit.BookPlatform("V1 Trecho oeste", new Vector3(-3.5f, 4.0f, 0f), new Vector2(3f, 4f));

            kit.ReadingLamp(new Vector3(4.6f, 2.0f, -4.6f), -45f);
            kit.ReadingLamp(new Vector3(4.6f, 2.5f, 4.6f), -135f);
            kit.ReadingLamp(new Vector3(-4.6f, 3.5f, 4.6f), 135f);
            kit.CeilingLight(new Vector3(5.4f, 5.0f, 0f), 90f, LuzAlerta);
            kit.CeilingLight(new Vector3(0f, 2.6f, -5.4f));
        }

        // ------------------------------------------------------------------ volta 2 (4,5 -> 12,8), Pulo duplo

        void Volta2(PhaseKit kit)
        {
            // cada quina tem um Pulo duplo; o livro seguinte fica 1,7 m acima (vão de 0,5 m) e a quina depois dele, +0,9
            kit.BookPlatform("V2 Quina SO", P(SO, 4.5f), new Vector2(3f, 3f));
            kit.Pickup(PickupKind.DoubleJump, P(SO, 4.5f));
            kit.BookPlatform("V2 Livro S", new Vector3(0f, 6.2f, -3.5f), new Vector2(3f, 2f));          // x -1,5..1,5
            kit.Spikes("V2 Espinhos S", new Vector3(0f, 6.2f, -3.5f), true, 1.0f, 2.2f, 0f);

            kit.BookPlatform("V2 Quina SE", P(SE, 7.1f), new Vector2(3f, 3f));
            kit.Pickup(PickupKind.DoubleJump, P(SE, 7.1f));
            kit.BookPlatform("V2 Livro L", new Vector3(3.5f, 8.8f, 0f), new Vector2(2f, 3f));           // z -1,5..1,5

            kit.BookPlatform("V2 Quina NE", P(NE, 9.7f), new Vector2(3f, 3f));
            kit.Pickup(PickupKind.DoubleJump, P(NE, 9.7f));
            kit.BookPlatform("V2 Livro N", new Vector3(0f, 11.4f, 3.5f), new Vector2(3f, 2f));          // x -1,5..1,5
            kit.Spikes("V2 Espinhos N", new Vector3(0f, 11.4f, 3.5f), true, 1.2f, 2.0f, 0f);

            kit.BookPlatform("V2 Quina NO", P(NO, 12.3f), new Vector2(3f, 3f));
            kit.BookPlatform("V2 Trecho oeste", new Vector3(-3.5f, 12.8f, 0f), new Vector2(3f, 4f));

            kit.CeilingLight(new Vector3(-5.4f, 7.5f, -5.4f), 0f, LuzPluma);
            kit.CeilingLight(new Vector3(5.4f, 10.1f, -5.4f), 0f, LuzPluma);
            kit.CeilingLight(new Vector3(5.4f, 12.7f, 5.4f), 0f, LuzPluma);
            kit.ReadingLamp(new Vector3(-4.6f, 12.3f, 4.6f), 135f);
            kit.Sign("pule duas vezes", new Vector3(-3.5f, 7.4f, -5.2f), 0f, 0.045f);
        }

        // ------------------------------------------------------------------ volta 3 (13,3 -> 16,0)

        void Volta3(PhaseKit kit)
        {
            // corredor sul fechado: torre ao norte, estante de 3,4 m ao sul; piso x -5..5, z -3,6..-2 (1,6 m livres)
            kit.BookPlatform("V3 Corredor das prensas", new Vector3(0f, 13.3f, -2.8f), new Vector2(10f, 1.6f));
            kit.Bookshelf("V3 Parapeito do corredor", new Vector3(-5.3f, 13.3f, -3.9f), new Vector3(5.3f, 13.3f, -3.9f), 3.4f);
            // ciclo de 3,4 s (1,6 erguida, 0,2 descendo, 0,8 embaixo, 0,8 subindo); a segunda está meio ciclo adiantada
            kit.Press("V3 Prensa oeste", new Vector3(-1.6f, 13.3f, -2.8f), 1.6f, 0.8f, 0f);
            kit.Press("V3 Prensa leste", new Vector3(1.6f, 13.3f, -2.8f), 1.6f, 0.8f, 1.7f);
            kit.CeilingLight(new Vector3(-3.5f, 16.5f, -2.8f), 90f);
            kit.CeilingLight(new Vector3(3.5f, 16.5f, -2.8f), 90f);

            // trecho leste: dois livros que cedem formando degraus de 0,9 m até a quina NE
            kit.CrumblingPlatform("V3 Degrau que cede 1", new Vector3(3.5f, 14.2f, -1.0f), new Vector2(3f, 1.6f), 0.8f);
            kit.CrumblingPlatform("V3 Degrau que cede 2", new Vector3(3.5f, 15.1f, 1.0f), new Vector2(3f, 1.6f), 0.8f);

            kit.BookPlatform("V3 Quina NE", P(NE, 16.0f), new Vector2(3f, 3f));
            kit.BookPlatform("V3 Trecho norte", new Vector3(0f, 16.0f, 3.5f), new Vector2(4f, 3f));
            kit.BookPlatform("V3 Quina NO", P(NO, 16.0f), new Vector2(3f, 3f));
            kit.Pickup(PickupKind.Gravity, P(NO, 16.0f));

            kit.Carpet(new Vector3(0f, 16.0f, 3.5f), new Vector2(4f, 2f));
            kit.ReadingLamp(new Vector3(4.6f, 16.0f, 4.6f), -135f);
            kit.CeilingLight(new Vector3(-5.4f, 19.0f, 3.5f), 0f, LuzPluma);
            kit.Sign("a pluma leva ao oeste", new Vector3(-5.2f, 18.6f, 3.5f), -90f, 0.045f);
        }

        // ------------------------------------------------------------------ ilha da porta (x -16,5..-10,5, z 0,5..6,5)

        void IlhaDaPorta(PhaseKit kit)
        {
            kit.OpenBookPlatform("Ilha da porta", new Vector3(-13.5f, 16.0f, 3.5f), new Vector2(6f, 6f));
            kit.Carpet(new Vector3(-13.5f, 16.0f, 3.5f), new Vector2(4f, 2.4f), 90f);
            kit.ReadingLamp(new Vector3(-16f, 16.0f, 1f), 45f);
            kit.ReadingLamp(new Vector3(-16f, 16.0f, 6f), 135f);
            kit.CeilingLight(new Vector3(-13.5f, 19.4f, 3.5f));
            // voltada para o leste, de frente para quem chega da torre
            kit.Goal(new Vector3(-15f, 16.0f, 3.5f), 90f);
        }

        // ------------------------------------------------------------------ cenário

        void Cenario(PhaseKit kit)
        {
            kit.DreamClock("Relogio da torre", new Vector3(0f, 23.5f, 0f), 3f);
            kit.FloatingBooks("Volta 1 sudoeste", new Vector3(-9f, 3f, -6f), 2f, 6);
            kit.FloatingBooks("Volta 2 leste", new Vector3(9f, 9f, 2f), 1.8f, 6);
            kit.FloatingBooks("Abaixo da ilha", new Vector3(-12f, 10f, -4f), 2f, 6);
            kit.FloatingPages("Volta 3 sudeste", new Vector3(9f, 15f, -7f), 1.8f, 10);
            kit.FloatingPages("Acima da ilha", new Vector3(-13.5f, 22f, 3.5f), 2.5f, 12);
            kit.DreamMotes("Poco da escadaria", new Vector3(0f, 9f, 0f), new Vector3(14f, 18f, 14f), null, 220);
        }
    }
}
