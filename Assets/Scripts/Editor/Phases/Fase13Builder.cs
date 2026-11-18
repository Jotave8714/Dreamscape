using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 13 — "Ala Proibida": a seção trancada da biblioteca, vigiada por feixes e pêndulos, sob neblina roxa.
    /// Ideia central: os feixes andam em onda. Quem corre no ritmo certo passa direto; quem não pegou o ritmo para
    /// entre dois feixes, sob a luz quente (luz quente marca refúgio, luz roxa marca perigo).
    ///
    /// Sequência (+Z): antessala → corredor dos feixes em onda (apresenta) → galeria dos pêndulos (apresenta)
    /// → patamar da escolha. Rota principal (oeste, sem poder): corredor das prensas → corredor das grades
    /// (laser + espinhos em ciclo) → corredor dos pêndulos. Rota opcional (centro, Pluma): duas ilhas de livros
    /// sobre o abismo, a segunda é uma página que cede. As duas chegam ao salão do cofre → cofre de teto baixo
    /// (onda de lasers e espinhos que não se pula) → ponte estreita com dois pêndulos → páginas que cedem → livro final.
    /// Medidas, grafo e contas em docs/Fase13.md.
    /// </summary>
    public sealed class Fase13Builder : IPhaseBuilder
    {
        public int Order => 13;
        public string SceneName => "Fase13_AlaProibida";
        public PhaseTheme Theme => Tema;

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        // continua a noite da Fase 12 (saturação ~12, exposição ~0,05): roxo intenso, neblina densa
        static readonly PhaseTheme Tema = new PhaseTheme
        {
            Name = "Fase13", Floor = C(0.42f, 0.28f, 0.55f), Wall = C(0.22f, 0.14f, 0.30f), Accent = C(0.86f, 0.40f, 1f),
            Sky = C(0.10f, 0.05f, 0.16f), Equator = C(0.18f, 0.10f, 0.26f), Ground = C(0.05f, 0.02f, 0.07f),
            Fog = C(0.15f, 0.06f, 0.21f), FogDensity = 0.03f, Sun = C(0.80f, 0.62f, 1f), SunIntensity = 1.0f,
            SunEuler = new Vector3(55f, -50f, 0f), Bloom = 0.85f, Vignette = 0.34f, Saturation = 14f, PostExposure = 0.06f,
            Tint = C(0.98f, 0.92f, 1f)
        };

        static readonly Color LuzRoxa = new Color(0.80f, 0.50f, 1f);    // trecho perigoso
        static readonly Color LuzQuente = new Color(1f, 0.82f, 0.55f);  // refúgio
        static readonly Color LuzPluma = new Color(0.75f, 0.62f, 1f);   // rota da Pluma

        // estantes das paredes: 4,5 m, mais alto que qualquer pulo da fase (Pluma: 2,2 m + 0,3 de degrau)
        const float AlturaEstante = 4.5f;

        // offset de uma armadilha para que um corredor a 5 m/s encontre a mesma fase do ciclo em todas
        static float Onda(float distancia, float periodo) => Mathf.Repeat(-distancia / 5f, periodo);

        public void Build(PhaseKit kit)
        {
            Antessala(kit);
            CorredorDosFeixes(kit);
            GaleriaDosPendulos(kit);
            PatamarDaEscolha(kit);
            AtalhoDaPluma(kit);
            CorredorDasPrensas(kit);
            CorredorDasGrades(kit);
            CorredorDosPendulos(kit);
            SalaoDoCofre(kit);
            Cofre(kit);
            PonteDoPendulo(kit);
            LivroFinal(kit);
            Atmosfera(kit);

            kit.FallZone(-10f);
            kit.LibraryBackdrop(new Vector2(-20f, -4f), new Vector2(5f, 111f), -8f, 18f, 6f);
        }

        // ------------------------------------------------------------------ E0: antessala (x -4..4, z -3..3)

        void Antessala(PhaseKit kit)
        {
            kit.Platform("E0 Antessala", new Vector3(0f, 0f, 0f), new Vector2(8f, 6f));
            kit.Spawn(new Vector3(0f, 0f, -0.8f), 0f);
            kit.Carpet(new Vector3(0f, 0f, 0f), new Vector2(2.4f, 5.4f));

            kit.Bookshelf("Antessala fundo", new Vector3(-4.3f, 0f, -3.3f), new Vector3(4.3f, 0f, -3.3f), AlturaEstante);
            kit.Bookshelf("Antessala oeste", new Vector3(-4.3f, 0f, -3f), new Vector3(-4.3f, 0f, 3f), AlturaEstante);
            kit.Bookshelf("Antessala leste", new Vector3(4.3f, 0f, -3f), new Vector3(4.3f, 0f, 3f), AlturaEstante);
            kit.Bookshelf("Antessala frente oeste", new Vector3(-4.3f, 0f, 3.3f), new Vector3(-1.95f, 0f, 3.3f), AlturaEstante);
            kit.Bookshelf("Antessala frente leste", new Vector3(1.95f, 0f, 3.3f), new Vector3(4.3f, 0f, 3.3f), AlturaEstante);
            // o portão emoldura a boca do corredor: laterais em x ±1,65, as mesmas estantes do corredor
            kit.ShelfArch("Portao da ala", new Vector3(0f, 0f, 3f), 2.7f, 3.6f, 0f);

            kit.ReadingDesk("Mesa da antessala", new Vector3(-3.2f, 0f, -1.4f), 90f);
            kit.ReadingLamp(new Vector3(-3.2f, 0.8f, -0.9f), 90f);
            kit.CeilingLight(new Vector3(0f, 4.3f, -0.5f), 0f, LuzQuente);

            kit.Sign("FASE 13", new Vector3(0f, 5.6f, 2.6f), 0f, 0.09f);
            kit.Sign("ALA PROIBIDA", new Vector3(0f, 4.95f, 2.6f), 0f, 0.075f);
            kit.Sign("OS FEIXES ANDAM EM ONDA", new Vector3(0f, 4.35f, 2.6f), 0f, 0.04f);
        }

        // ------------------------------------------------------------------ A: corredor dos feixes (z 3..20)

        void CorredorDosFeixes(PhaseKit kit)
        {
            // largura interna 2,7 m = largura do feixe; piso 3,9 m para as estantes ficarem apoiadas
            kit.Platform("A Corredor dos feixes", new Vector3(0f, 0f, 11.5f), new Vector2(3.9f, 17f));
            kit.Carpet(new Vector3(0f, 0f, 11.75f), new Vector2(1.6f, 16f));
            kit.Bookshelf("Feixes oeste", new Vector3(-1.65f, 0f, 3.6f), new Vector3(-1.65f, 0f, 20f), AlturaEstante);
            kit.Bookshelf("Feixes leste", new Vector3(1.65f, 0f, 3.6f), new Vector3(1.65f, 0f, 20f), AlturaEstante);

            // 1,2 s ligado, 2,0 s apagado (período 3,2 s); a cada 4 m a onda atrasa 0,8 s, o tempo de correr 4 m
            for (int k = 0; k < 3; k++)
            {
                float z = 7f + 4f * k;
                kit.Laser($"Feixe {k + 1}", new Vector3(0f, 0f, z), 0f, 1.2f, 2.0f, Onda(4f * k, 3.2f));
            }

            // refúgios entre os feixes sob luz quente; luz roxa sobre os feixes
            kit.CeilingLight(new Vector3(0f, 4.2f, 5f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, 4.2f, 9f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, 4.2f, 13f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, 4.2f, 17f), 0f, LuzQuente);
            kit.ReadingLamp(new Vector3(-1.65f, AlturaEstante, 11f), 90f, LuzRoxa);
            kit.ReadingLamp(new Vector3(1.65f, AlturaEstante, 15f), -90f, LuzRoxa);
        }

        // ------------------------------------------------------------------ B: galeria dos pêndulos (z 20..34)

        void GaleriaDosPendulos(PhaseKit kit)
        {
            // largura interna 6 m (x -3..3): o pêndulo de 55° alcança 2,17 + 0,62 = 2,79 m sem entrar nas estantes
            kit.Platform("B Galeria dos pendulos", new Vector3(0f, 0f, 27f), new Vector2(7.2f, 14f));
            kit.Bookshelf("Galeria oeste", new Vector3(-3.3f, 0f, 20f), new Vector3(-3.3f, 0f, 34f), 5f);
            kit.Bookshelf("Galeria leste", new Vector3(3.3f, 0f, 20f), new Vector3(3.3f, 0f, 34f), 5f);
            kit.Bookshelf("Galeria fundo oeste", new Vector3(-3.0f, 0f, 20.3f), new Vector3(-1.95f, 0f, 20.3f), 5f);
            kit.Bookshelf("Galeria fundo leste", new Vector3(1.95f, 0f, 20.3f), new Vector3(3.0f, 0f, 20.3f), 5f);

            // balançam em X, meio período defasados: quando um está à esquerda, o outro está à direita
            kit.Pendulum("Pendulo da galeria 1", new Vector3(0f, 0f, 24.5f), 55f, 2.4f, 0f, 0f);
            kit.Pendulum("Pendulo da galeria 2", new Vector3(0f, 0f, 29.5f), 55f, 2.4f, 1.2f, 0f);
            kit.Platform("Viga do pendulo 1", new Vector3(0f, 4.3f, 24.5f), new Vector2(6f, 0.6f));
            kit.Platform("Viga do pendulo 2", new Vector3(0f, 4.3f, 29.5f), new Vector2(6f, 0.6f));

            kit.CeilingLight(new Vector3(0f, 4.6f, 27f), 0f, LuzQuente);      // refúgio entre os pêndulos
            kit.CeilingLight(new Vector3(-2f, 4.6f, 21.5f), 0f, LuzRoxa);
            kit.CeilingLight(new Vector3(2f, 4.6f, 32.5f), 0f, LuzRoxa);
            kit.ReadingLamp(new Vector3(-3.3f, 5f, 27f), 90f);
            kit.ReadingLamp(new Vector3(3.3f, 5f, 27f), -90f);
            kit.Sign("PASSE PELO LADO VAZIO", new Vector3(0f, 3.3f, 22.6f), 0f, 0.045f);
        }

        // ------------------------------------------------------------------ C: patamar da escolha (x -4,5..4,5, z 34..42)

        void PatamarDaEscolha(PhaseKit kit)
        {
            kit.Platform("C Patamar da escolha", new Vector3(0f, 0f, 38f), new Vector2(9f, 8f));
            kit.Bookshelf("Patamar oeste", new Vector3(-4.8f, 0f, 34f), new Vector3(-4.8f, 0f, 38.6f), AlturaEstante);

            // pedestal da Pluma (degrau de 0,5 m) no canto leste, de frente para o abismo das ilhas
            kit.BookPlatform("C Pedestal da Pluma", new Vector3(3f, 0.5f, 36.5f), new Vector2(1.6f, 1.6f), 0f, 0.5f);
            kit.Pickup(PickupKind.Gravity, new Vector3(3f, 0.5f, 36.5f));
            kit.CeilingLight(new Vector3(3f, 5f, 36.5f), 0f, LuzPluma);

            kit.ReadingDesk("Mesa do patamar", new Vector3(-2.6f, 0f, 34.8f), 0f);
            kit.ReadingLamp(new Vector3(-3.1f, 0.8f, 34.8f), 0f);
            kit.CeilingLight(new Vector3(-1.5f, 4.6f, 38f), 0f, LuzQuente);

            kit.Sign("ATALHO: SO COM A PLUMA", new Vector3(2f, 2.3f, 41.8f), 0f, 0.045f);
            kit.Sign("ROTA DAS PRENSAS", new Vector3(-4.45f, 3.4f, 40f), -90f, 0.045f);
        }

        // ------------------------------------------------------------------ rota opcional: ilhas da Pluma (x 0,75..3,25)

        void AtalhoDaPluma(PhaseKit kit)
        {
            // vãos de 5,5 m no plano: só a Pluma (6,63 m) atravessa; o pulo comum chega a 3,32 m
            kit.BookPlatform("I1 Ilha de livros", new Vector3(2f, 0f, 48.75f), new Vector2(2.5f, 2.5f), 0f, 1.6f);
            kit.CrumblingPlatform("I2 Pagina solta", new Vector3(2f, 0f, 56.5f), new Vector2(2.5f, 2f), 0.8f);
            kit.CeilingLight(new Vector3(2f, 5f, 48.75f), 0f, LuzPluma);
            kit.CeilingLight(new Vector3(2f, 5f, 56.5f), 0f, LuzPluma);
        }

        // ------------------------------------------------------------------ W1: corredor das prensas (x -15,5..-4,5)

        void CorredorDasPrensas(PhaseKit kit)
        {
            // corredor de 1,6 m (estantes centradas em z 38,9 e 41,1): a prensa cobre a largura toda
            kit.Platform("W1 Corredor das prensas", new Vector3(-10f, 0f, 40f), new Vector2(11f, 3.4f));
            kit.Bookshelf("Prensas sul", new Vector3(-4.5f, 0f, 38.9f), new Vector3(-15.5f, 0f, 38.9f), AlturaEstante);
            kit.Bookshelf("Prensas norte", new Vector3(-4.5f, 0f, 41.1f), new Vector3(-15.5f, 0f, 41.1f), AlturaEstante);

            // ciclo de 3,2 s (1,4 erguida + 0,2 descendo + 0,8 embaixo + 0,8 subindo); onda para quem corre a 5 m/s em -X
            for (int k = 0; k < 3; k++)
            {
                float x = -7.5f - 3f * k;
                kit.Press($"Prensa {k + 1}", new Vector3(x, 0f, 40f), 1.4f, 0.8f, Onda(3f * k, 3.2f), 0f);
            }
            kit.CeilingLight(new Vector3(-6f, 4.2f, 40f), 90f, LuzQuente);
            kit.CeilingLight(new Vector3(-9f, 4.2f, 40f), 90f, LuzQuente);
            kit.CeilingLight(new Vector3(-12f, 4.2f, 40f), 90f, LuzQuente);

            // W2: curva
            kit.Platform("W2 Curva das prensas", new Vector3(-17.5f, 0f, 40f), new Vector2(4f, 4f));
            kit.Bookshelf("Curva baixa oeste", new Vector3(-19.8f, 0f, 37.7f), new Vector3(-19.8f, 0f, 42f), AlturaEstante);
            kit.Bookshelf("Curva baixa sul", new Vector3(-19.8f, 0f, 37.7f), new Vector3(-15.5f, 0f, 37.7f), AlturaEstante);
            kit.CeilingLight(new Vector3(-17.5f, 4.2f, 40f), 0f, LuzQuente);
        }

        // ------------------------------------------------------------------ W3: corredor das grades (z 42..60, x -17,5)

        void CorredorDasGrades(PhaseKit kit)
        {
            const float x = -17.5f;
            kit.Platform("W3 Corredor das grades", new Vector3(x, 0f, 51f), new Vector2(3.9f, 18f));
            kit.Bookshelf("Grades oeste", new Vector3(x - 1.65f, 0f, 42f), new Vector3(x - 1.65f, 0f, 60f), AlturaEstante);
            kit.Bookshelf("Grades leste", new Vector3(x + 1.65f, 0f, 42f), new Vector3(x + 1.65f, 0f, 60f), AlturaEstante);

            // período comum de 3,2 s; offsets em onda a partir do primeiro laser (z 45)
            kit.Laser("Grade 1", new Vector3(x, 0f, 45f), 0f, 1.2f, 2.0f, Onda(0f, 3.2f));
            // dois campos seguidos (z 48..52): 4,56 m de travessia, o pulo não os vence
            kit.Spikes("Grades espinhos 1", new Vector3(x, 0f, 49f), true, 1.2f, 2.0f, Onda(5f, 3.2f));
            kit.Spikes("Grades espinhos 2", new Vector3(x, 0f, 51f), true, 1.2f, 2.0f, Onda(5f, 3.2f));
            kit.Laser("Grade 2", new Vector3(x, 0f, 55f), 0f, 1.2f, 2.0f, Onda(10f, 3.2f));

            kit.CeilingLight(new Vector3(x, 4.2f, 46.6f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(x, 4.2f, 53.5f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(x, 4.2f, 50f), 0f, LuzRoxa);
            kit.CeilingLight(new Vector3(x, 4.2f, 58f), 0f, LuzRoxa);
        }

        // ------------------------------------------------------------------ W4 + W5: curva alta e corredor dos pêndulos (z 58,6..65,4)

        void CorredorDosPendulos(PhaseKit kit)
        {
            kit.Platform("W4 Curva alta", new Vector3(-17.5f, 0f, 62.7f), new Vector2(4f, 5.4f));
            kit.Bookshelf("Curva alta oeste", new Vector3(-19.8f, 0f, 60f), new Vector3(-19.8f, 0f, 65.4f), AlturaEstante);
            kit.CeilingLight(new Vector3(-17.5f, 4.2f, 62.5f), 0f, LuzQuente);

            // largura interna 5,6 m (z 59,2..64,8): o pêndulo de 55° não entra nas estantes
            kit.Platform("W5 Corredor dos pendulos", new Vector3(-10f, 0f, 62f), new Vector2(11f, 6.8f));
            kit.Bookshelf("Pendulos sul", new Vector3(-15.5f, 0f, 58.9f), new Vector3(-4.5f, 0f, 58.9f), 5f);
            kit.Bookshelf("Pendulos norte", new Vector3(-19.8f, 0f, 65.1f), new Vector3(-4.5f, 0f, 65.1f), 5f);

            // balançam em Z (yaw 90), meio período defasados
            kit.Pendulum("Pendulo do corredor 1", new Vector3(-12.5f, 0f, 62f), 55f, 2.4f, 0f, 90f);
            kit.Pendulum("Pendulo do corredor 2", new Vector3(-8.5f, 0f, 62f), 55f, 2.4f, 1.2f, 90f);
            kit.Platform("Viga do corredor 1", new Vector3(-12.5f, 4.3f, 62f), new Vector2(0.6f, 5.6f));
            kit.Platform("Viga do corredor 2", new Vector3(-8.5f, 4.3f, 62f), new Vector2(0.6f, 5.6f));

            kit.CeilingLight(new Vector3(-10.5f, 4.6f, 62f), 90f, LuzQuente);
            kit.CeilingLight(new Vector3(-14.5f, 4.6f, 62f), 90f, LuzRoxa);
            kit.CeilingLight(new Vector3(-6.5f, 4.6f, 62f), 90f, LuzRoxa);
        }

        // ------------------------------------------------------------------ M: salão do cofre (x -4,5..4,5, z 60..68)

        void SalaoDoCofre(PhaseKit kit)
        {
            kit.Platform("M Salao do cofre", new Vector3(0f, 0f, 64f), new Vector2(9f, 8f));
            kit.Bookshelf("Salao oeste", new Vector3(-4.8f, 0f, 65.4f), new Vector3(-4.8f, 0f, 68f), AlturaEstante);
            kit.Bookshelf("Salao norte oeste", new Vector3(-4.5f, 0f, 68.3f), new Vector3(-1.95f, 0f, 68.3f), AlturaEstante);
            kit.Bookshelf("Salao norte leste", new Vector3(1.95f, 0f, 68.3f), new Vector3(4.5f, 0f, 68.3f), AlturaEstante);

            kit.ReadingDesk("Mesa do salao", new Vector3(3.4f, 0f, 66.6f), 90f);
            kit.ReadingLamp(new Vector3(3.4f, 0.8f, 66.1f), -90f);
            kit.CeilingLight(new Vector3(-1.5f, 4.6f, 63.5f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(2f, 4.6f, 62f), 0f, LuzQuente);

            kit.Sign("O COFRE", new Vector3(0f, 3.75f, 67.6f), 0f, 0.08f);
            kit.Sign("TETO BAIXO: SO PASSA NO RITMO", new Vector3(0f, 3.15f, 67.6f), 0f, 0.035f);
        }

        // ------------------------------------------------------------------ V1: cofre de teto baixo (z 68..82)

        void Cofre(PhaseKit kit)
        {
            kit.Platform("V1 Cofre", new Vector3(0f, 0f, 75f), new Vector2(3.9f, 14f));
            kit.Bookshelf("Cofre oeste", new Vector3(-1.65f, 0f, 68f), new Vector3(-1.65f, 0f, 82f), 2.3f);
            kit.Bookshelf("Cofre leste", new Vector3(1.65f, 0f, 68f), new Vector3(1.65f, 0f, 82f), 2.3f);
            // teto a 2,3 m: o pé sobe no máximo 0,54 m, nem com a Pluma dá para saltar feixe ou espinho
            kit.BookPlatform("Teto do cofre", new Vector3(0f, 2.8f, 75f), new Vector2(3.9f, 14f), 0f, 0.5f);

            // 1,5 s ligado, 1,5 s apagado; feixes a cada 3,5 m e o campo de espinhos no fim, todos na mesma onda
            float[] zs = { 71f, 74.5f, 78f };
            for (int k = 0; k < zs.Length; k++)
                kit.Laser($"Cofre feixe {k + 1}", new Vector3(0f, 0f, zs[k]), 0f, 1.5f, 1.5f, Onda(zs[k] - 71f, 3f));
            kit.Spikes("Cofre espinhos", new Vector3(0f, 0f, 81f), true, 1.2f, 1.8f, Onda(10f, 3f));

            kit.CeilingLight(new Vector3(0f, 2.35f, 72.75f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, 2.35f, 76.25f), 0f, LuzQuente);
            kit.CeilingLight(new Vector3(0f, 2.35f, 79.4f), 0f, LuzRoxa);
        }

        // ------------------------------------------------------------------ V2: ponte do pêndulo (x -1..1, z 82..95,5)

        void PonteDoPendulo(PhaseKit kit)
        {
            kit.BookPlatform("V2 Ponte do pendulo", new Vector3(0f, 0f, 88.75f), new Vector2(2f, 13.5f), 0f, 0.8f);

            // 70° e 3 s: o peso atravessa a ponte rápido e passa muito tempo sobre o abismo
            kit.Pendulum("Pendulo da ponte 1", new Vector3(0f, 0f, 87f), 70f, 3f, 0f, 0f);
            kit.Pendulum("Pendulo da ponte 2", new Vector3(0f, 0f, 92.5f), 70f, 3f, 1.5f, 0f);
            foreach (float z in new[] { 87f, 92.5f })
            {
                kit.Platform($"Viga da ponte {z}", new Vector3(0f, 4.3f, z), new Vector2(7.6f, 0.6f));
                kit.Pillar($"Coluna da ponte oeste {z}", new Vector3(-3.8f, -8f, z), 12.3f);
                kit.Pillar($"Coluna da ponte leste {z}", new Vector3(3.8f, -8f, z), 12.3f);
            }
            kit.CeilingLight(new Vector3(0f, 5f, 89.75f), 0f, LuzQuente);

            // páginas que cedem (0,8 s), 1 m de vão entre elas
            kit.CrumblingPlatform("V3 Pagina 1", new Vector3(0f, 0f, 97.5f), new Vector2(2f, 2f), 0.8f);
            kit.CrumblingPlatform("V3 Pagina 2", new Vector3(0f, 0f, 100.5f), new Vector2(2f, 2f), 0.8f);
        }

        // ------------------------------------------------------------------ fim: livro aberto (z 102,5..110,5)

        void LivroFinal(PhaseKit kit)
        {
            kit.OpenBookPlatform("Livro final", new Vector3(0f, 0f, 106.5f), new Vector2(8f, 8f));
            kit.Goal(new Vector3(0f, 0f, 108.5f), 180f);
            kit.Bookshelf("Fim oeste", new Vector3(-4.3f, 0f, 103f), new Vector3(-4.3f, 0f, 110.5f), 6f);
            kit.Bookshelf("Fim leste", new Vector3(4.3f, 0f, 103f), new Vector3(4.3f, 0f, 110.5f), 6f);
            kit.Bookshelf("Fim fundo", new Vector3(-4.3f, 0f, 110.8f), new Vector3(4.3f, 0f, 110.8f), 6f);
            kit.Pole(new Vector3(-3.3f, 0f, 103.5f), LuzQuente);
            kit.Pole(new Vector3(3.3f, 0f, 103.5f), LuzQuente);
            kit.ReadingLamp(new Vector3(-4.3f, 6f, 107f), 90f, LuzRoxa);
            kit.ReadingLamp(new Vector3(4.3f, 6f, 107f), -90f, LuzRoxa);
            // marco visível desde o salão: relógio alto sobre a porta
            kit.DreamClock("Relogio do cofre", new Vector3(0f, 10f, 112f), 3.2f, 0f, 3f);
        }

        // ------------------------------------------------------------------ cenário fora do caminho

        void Atmosfera(PhaseKit kit)
        {
            kit.FloatingBooks("Abismo oeste", new Vector3(-7.5f, 1f, 90f), 2.5f, 7);
            kit.FloatingBooks("Abismo leste", new Vector3(7.5f, 1.5f, 95f), 2.5f, 6);
            kit.FloatingPages("Atalho", new Vector3(9f, 1f, 52f), 2.5f, 14);
            kit.FloatingPages("Sob as ilhas", new Vector3(2f, -4.5f, 52f), 3f, 16);
            kit.FloatingBooks("Sobre a galeria", new Vector3(0f, 8f, 27f), 3f, 8);
            kit.DreamClock("Relogio parado oeste", new Vector3(-12f, 9f, 28f), 2.6f, -90f, 2f);
            kit.DreamClock("Relogio parado leste", new Vector3(13f, 8f, 64f), 2.2f, 90f, 2f);
            kit.DreamMotes("Galeria", new Vector3(0f, 6.5f, 27f), new Vector3(6f, 2.5f, 14f), new Color(0.9f, 0.6f, 1f), 120);
            kit.DreamMotes("Abismo", new Vector3(2f, -3f, 75f), new Vector3(16f, 4f, 60f), new Color(0.85f, 0.55f, 1f), 260);
        }
    }
}
