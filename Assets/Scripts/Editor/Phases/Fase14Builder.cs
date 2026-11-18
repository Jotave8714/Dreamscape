using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 14 — "Torre do Relógio": subida em espiral em volta de um núcleo de estantes de 14 m.
    /// Ideia central: tudo bate no mesmo tique de 3 s. Lasers (1,5 ligado / 1,5 apagado), espinhos (1,2 / 1,8),
    /// prensas (ciclo de 3 s) e pêndulos (período de 3 s) dividem o mesmo relógio, e os offsets formam ondas para quem
    /// corre a 5 m/s. Quem não pega a onda espera nos cantos (patamares) e nos vãos entre armadilhas.
    ///
    /// A passarela tem 3 m de largura, com o núcleo de um lado e o vazio do outro. São 10 lados de 12 m (2,5 voltas),
    /// cada um com dois trechos de 6 m. Cada trecho sobe 0,6 m e cada volta sobe 4,8 m, então a volta de cima passa
    /// 4,3 m acima da de baixo. Os trechos com pêndulo viram sacadas de 5,6 m para o peso não entrar nas estantes.
    /// Volta 1 apresenta cada armadilha sozinha; a volta 2 combina em ondas; a volta 3 é o desafio.
    /// No último canto, o Pulo Duplo vence o degrau de 1,6 m até o mostrador no topo, onde fica a porta.
    /// Medidas, grafo e contas em docs/Fase14.md.
    /// </summary>
    public sealed class Fase14Builder : IPhaseBuilder
    {
        public int Order => 14;
        public string SceneName => "Fase14_TorreDoRelogio";
        public PhaseTheme Theme => Tema;

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        // ainda noite (depois do roxo da 13): anil profundo com destaque de latão
        static readonly PhaseTheme Tema = new PhaseTheme
        {
            Name = "Fase14", Floor = C(0.50f, 0.40f, 0.52f), Wall = C(0.24f, 0.18f, 0.30f), Accent = C(1f, 0.78f, 0.35f),
            Sky = C(0.08f, 0.07f, 0.18f), Equator = C(0.16f, 0.13f, 0.26f), Ground = C(0.05f, 0.04f, 0.08f),
            Fog = C(0.12f, 0.10f, 0.22f), FogDensity = 0.022f, Sun = C(0.95f, 0.85f, 1f), SunIntensity = 1.1f,
            SunEuler = new Vector3(52f, 135f, 0f), Bloom = 0.9f, Vignette = 0.3f, Saturation = 16f, PostExposure = 0.09f,
            Tint = C(1f, 0.95f, 0.95f)
        };

        static readonly Color LuzLatao = new Color(1f, 0.80f, 0.45f);  // refúgios e patamares
        static readonly Color LuzAnil = new Color(0.62f, 0.55f, 1f);   // trechos com armadilha

        const float Raio = 7.5f;        // linha central da passarela (núcleo em 6, borda em 9)
        const float Tique = 3f;         // período comum de todas as armadilhas
        const float Volta = 4.8f;       // subida por volta (8 degraus de 0,6 m)
        const int Lados = 10;

        // ------------------------------------------------------------------ geometria da espiral

        // d = lado (0 oeste subindo em +Z, 1 norte em +X, 2 leste em -Z, 3 sul em -X)
        // s = posição ao longo do lado (-6 no início, +6 no fim, 7,5 no centro do canto seguinte)
        // u = deslocamento lateral a partir da linha central (positivo = para fora, longe do núcleo)
        static Vector3 P(int d, float s, float u, float y)
        {
            switch (d)
            {
                case 0: return new Vector3(-Raio - u, y, s);
                case 1: return new Vector3(s, y, Raio + u);
                case 2: return new Vector3(Raio + u, y, -s);
                default: return new Vector3(-s, y, -Raio - u);
            }
        }

        static Vector2 Tamanho(int d, float aoLongo, float lateral) =>
            d % 2 == 0 ? new Vector2(lateral, aoLongo) : new Vector2(aoLongo, lateral);

        // yaw de laser e pêndulo para atravessarem a passarela
        static float Atravessa(int d) => d % 2 == 0 ? 0f : 90f;

        static float NivelA(int i) => 1.2f * i;
        static float NivelB(int i) => 1.2f * i + 0.6f;
        static float NivelCanto(int i) => 1.2f * (i + 1);

        static float Onda(float distancia) => Mathf.Repeat(-distancia / 5f, Tique);

        public void Build(PhaseKit kit)
        {
            Praca(kit);
            Nucleo(kit);
            for (int i = 0; i < Lados; i++)
            {
                Lado(kit, i);
                Canto(kit, i);
            }
            Topo(kit);
            Atmosfera(kit);

            kit.FallZone(-8f);
            kit.LibraryBackdrop(new Vector2(-14f, -14f), new Vector2(12f, 12f), -8f, 32f, 14f);
        }

        // ------------------------------------------------------------------ praça de entrada (x -14..-6, z -14..-6)

        void Praca(PhaseKit kit)
        {
            kit.Platform("Praca do relogio", new Vector3(-10f, 0f, -10f), new Vector2(8f, 8f));
            kit.Spawn(new Vector3(-7.5f, 0f, -11.5f), 0f);
            kit.Carpet(new Vector3(-7.5f, 0f, -10f), new Vector2(2.4f, 6f));
            kit.Bookshelf("Praca oeste", new Vector3(-14.3f, 0f, -14f), new Vector3(-14.3f, 0f, -6f), 4f);
            kit.Bookshelf("Praca sul", new Vector3(-14f, 0f, -14.3f), new Vector3(-6f, 0f, -14.3f), 4f);
            kit.ReadingDesk("Mesa da praca", new Vector3(-12.6f, 0f, -12f), 90f);
            kit.ReadingLamp(new Vector3(-12.6f, 0.8f, -11.5f), 90f);
            kit.LibraryLadder(new Vector3(-13.7f, 0f, -8f), 3.8f, -90f);
            kit.CeilingLight(new Vector3(-10f, 4.1f, -10f), 0f, LuzLatao);

            // lidas da praça, olhando para +Z (sob o canto SW da primeira volta)
            kit.Sign("FASE 14", new Vector3(-7.5f, 3.65f, -6.5f), 0f, 0.08f);
            kit.Sign("TORRE DO RELOGIO", new Vector3(-7.5f, 3.1f, -6.5f), 0f, 0.065f);
            kit.Sign("TUDO BATE NO MESMO TIQUE", new Vector3(-7.5f, 2.6f, -6.5f), 0f, 0.035f);
        }

        // ------------------------------------------------------------------ núcleo: torre de estantes (faces em |x|,|z| = 6)

        void Nucleo(PhaseKit kit)
        {
            const float baseY = -1.5f, altura = 14.6f; // topo em 13,1, sob o mostrador
            kit.Bookshelf("Nucleo oeste", new Vector3(-5.7f, baseY, -6f), new Vector3(-5.7f, baseY, 6f), altura);
            kit.Bookshelf("Nucleo norte", new Vector3(-6f, baseY, 5.7f), new Vector3(6f, baseY, 5.7f), altura);
            kit.Bookshelf("Nucleo leste", new Vector3(5.7f, baseY, 6f), new Vector3(5.7f, baseY, -6f), altura);
            kit.Bookshelf("Nucleo sul", new Vector3(6f, baseY, -5.7f), new Vector3(-6f, baseY, -5.7f), altura);
        }

        // ------------------------------------------------------------------ trechos de cada lado

        // trecho de 6 m: A em s -6..0, B em s 0..6. A sacada tem 5,6 m (u -1,5..4,1) e pivô em u = 1,3
        void Trecho(PhaseKit kit, int i, bool b, bool sacada)
        {
            int d = i % 4;
            float y = b ? NivelB(i) : NivelA(i);
            float s = b ? 3f : -3f;
            string nome = $"S{i + 1}{(b ? "B" : "A")}";
            kit.BookPlatform(nome, P(d, s, sacada ? 1.3f : 0f, y), Tamanho(d, 6f, sacada ? 5.6f : 3f), 0f, 0.5f);
            // a luz fica logo abaixo da volta de cima (4,3 m acima do piso)
            kit.CeilingLight(P(d, s, 0f, y + 4.25f), Atravessa(d), sacada ? LuzAnil : LuzLatao);
        }

        // grade de estante na borda externa (u 1,5..2,1, 3 m ao longo do caminho, 2,5 m de altura): com ela o centro
        // do corpo fica entre u -1,22 (núcleo) e 1,22 (grade), e o laser ou a fileira de espinhos cobre a largura toda.
        // Contornar a grade pelo ar exigiria sair além de u 2,38 por 3 m de voo e voltar, o que o controle aéreo não permite
        void Grade(PhaseKit kit, int i, bool b, float s, string nome)
        {
            int d = i % 4;
            float y = b ? NivelB(i) : NivelA(i);
            kit.Bookshelf($"S{i + 1} grade {nome} {s}", P(d, s - 1.5f, 1.8f, y), P(d, s + 1.5f, 1.8f, y), 2.5f);
        }

        // feixe de 2,7 m (u -1,35..1,35): o corpo precisaria de |u| > 1,63 para passar ao lado
        void Laser(PhaseKit kit, int i, bool b, float s, float offset)
        {
            int d = i % 4;
            kit.Laser($"S{i + 1} laser {s}", P(d, s, 0f, b ? NivelB(i) : NivelA(i)), Atravessa(d), 1.5f, 1.5f, offset);
            Grade(kit, i, b, s, "laser");
        }

        // fileira de dois campos (u ±0,5): cobre u -1,4..1,4; o corpo precisaria de |u| > 1,68
        void Espinhos(PhaseKit kit, int i, bool b, float s, float offset)
        {
            int d = i % 4;
            float y = b ? NivelB(i) : NivelA(i);
            kit.Spikes($"S{i + 1} espinhos {s} a", P(d, s, -0.5f, y), true, 1.2f, 1.8f, offset);
            kit.Spikes($"S{i + 1} espinhos {s} b", P(d, s, 0.5f, y), true, 1.2f, 1.8f, offset);
            Grade(kit, i, b, s, "espinhos");
        }

        // pêndulo de 52° na sacada: alcança 2,09 + 0,62 m dos dois lados do pivô, de u -1,41 a 4,01
        void Pendulo(PhaseKit kit, int i, bool b, float s, float offset)
        {
            int d = i % 4;
            float y = b ? NivelB(i) : NivelA(i);
            kit.Pendulum($"S{i + 1} pendulo {s}", P(d, s, 1.3f, y), 52f, Tique, offset, Atravessa(d));
            kit.Platform($"S{i + 1} eixo {s}", P(d, s, 1.3f, y + 4.2f), Tamanho(d, 0.6f, 5.6f));
        }

        // prensa num portão de estantes entre s0 e s1: estante interna em u -1,1 (face em -0,8) e externa em u 1,3
        // (face em 1,0, além da borda da passarela). Sobram 0,2 m ao lado do bloco, menos que o corpo
        void Prensa(PhaseKit kit, int i, bool b, float s, float s0, float s1, float offset)
        {
            int d = i % 4;
            float y = b ? NivelB(i) : NivelA(i);
            kit.Bookshelf($"S{i + 1} portao {s} dentro", P(d, s0, -1.1f, y), P(d, s1, -1.1f, y), 3f);
            kit.Bookshelf($"S{i + 1} portao {s} fora", P(d, s0, 1.3f, y), P(d, s1, 1.3f, y), 3f);
            // 1,4 erguida + 0,2 descendo + 0,6 embaixo + 0,8 subindo = 3 s; fere de 1,4 a 2,2
            kit.Press($"S{i + 1} prensa {s}", P(d, s, 0f, y), 1.4f, 0.6f, offset, Atravessa(d));
        }

        void Lado(PhaseKit kit, int i)
        {
            switch (i)
            {
                // ---------------- volta 1: apresentar, uma armadilha por trecho
                case 0:
                    Trecho(kit, i, false, false); Laser(kit, i, false, -3f, 0f);
                    Trecho(kit, i, true, false); Laser(kit, i, true, 3f, 1.5f);
                    break;
                case 1:
                    Trecho(kit, i, false, false); Espinhos(kit, i, false, -3f, 0f);
                    Trecho(kit, i, true, true); Pendulo(kit, i, true, 3f, 0f);
                    break;
                case 2:
                    Trecho(kit, i, false, false); Prensa(kit, i, false, -3f, -4f, -2f, 0f);
                    Trecho(kit, i, true, false); Laser(kit, i, true, 1.5f, 0f); Espinhos(kit, i, true, 4.5f, Onda(3f));
                    break;
                case 3:
                    Trecho(kit, i, false, true); Pendulo(kit, i, false, -3f, 0f);
                    Trecho(kit, i, true, true); Pendulo(kit, i, true, 3f, 1.5f);
                    break;

                // ---------------- volta 2: testar, ondas e pares
                case 4:
                    Trecho(kit, i, false, false); Laser(kit, i, false, -4.5f, 0f); Laser(kit, i, false, -1.5f, Onda(3f));
                    Trecho(kit, i, true, true); Pendulo(kit, i, true, 3f, 0f);
                    break;
                case 5:
                    Trecho(kit, i, false, false);
                    Prensa(kit, i, false, -4.5f, -5.5f, -3f, 0f);
                    Prensa(kit, i, false, -1.5f, -3f, -0.5f, Onda(3f));
                    // fileiras alternadas: atravessa a primeira, espera no meio (1,2 m livres), atravessa a segunda
                    Trecho(kit, i, true, false); Espinhos(kit, i, true, 1.2f, 0f); Espinhos(kit, i, true, 4.8f, 1.5f);
                    break;
                case 6:
                    Trecho(kit, i, false, true); Pendulo(kit, i, false, -4.3f, 0f); Pendulo(kit, i, false, -1.3f, 0.75f);
                    Trecho(kit, i, true, false); Laser(kit, i, true, 1.5f, 0f); Prensa(kit, i, true, 4.5f, 3.5f, 5.5f, 0.6f);
                    break;
                case 7:
                    Trecho(kit, i, false, false); Trecho(kit, i, true, false);
                    Laser(kit, i, false, -4.5f, Onda(0f)); Laser(kit, i, false, -1.5f, Onda(3f));
                    Laser(kit, i, true, 1.5f, Onda(6f)); Laser(kit, i, true, 4.5f, Onda(9f));
                    break;

                // ---------------- volta 3: desafio
                case 8:
                    Trecho(kit, i, false, true); Trecho(kit, i, true, true);
                    // carrilhão: cada pêndulo passa pelo centro 1 s depois do anterior
                    Pendulo(kit, i, false, -4.3f, 0f); Pendulo(kit, i, false, -1.3f, 1f); Pendulo(kit, i, true, 3f, 2f);
                    break;
                default:
                    Trecho(kit, i, false, false); Laser(kit, i, false, -4.5f, 0f); Espinhos(kit, i, false, -2f, Onda(2.5f));
                    Trecho(kit, i, true, false); Prensa(kit, i, true, 1.5f, 0.5f, 2.5f, 0f); Laser(kit, i, true, 4.5f, Onda(9f));
                    break;
            }
        }

        // canto 3 x 3 depois do lado i, 0,6 m acima do trecho B: patamar de descanso
        void Canto(PhaseKit kit, int i)
        {
            int d = i % 4;
            float y = NivelCanto(i);
            kit.BookPlatform($"C{i + 1} canto", P(d, 7.5f, 0f, y), new Vector2(3f, 3f), 0f, 0.5f);
            kit.CeilingLight(P(d, 7.5f, 0f, y + 4.25f), 0f, LuzLatao);
        }

        // ------------------------------------------------------------------ topo: último degrau e mostrador (y 13,6)

        void Topo(PhaseKit kit)
        {
            // C10 (canto NE, y 12,0) guarda o Pulo Duplo; o degrau S11 fica 1,6 m acima
            kit.Pickup(PickupKind.DoubleJump, P(1, 7.5f, 0f, NivelCanto(9)));
            kit.BookPlatform("S11 Ultimo degrau", new Vector3(7.5f, 13.6f, 3.5f), new Vector2(3f, 5f), 0f, 0.5f);
            kit.Sign("ULTIMO DEGRAU: PULO DUPLO", new Vector3(7.5f, 15.6f, 5.9f), 180f, 0.04f);

            kit.OpenBookPlatform("Mostrador do topo", new Vector3(0f, 13.6f, 0f), new Vector2(12f, 12f));
            // parapeito de 0,9 m em volta do topo; aberto só a leste, de z 1 a 5,4, onde chega o S11
            const float y = 13.6f, h = 0.9f;
            kit.Bookshelf("Parapeito norte", new Vector3(-6f, y, 5.7f), new Vector3(6f, y, 5.7f), h);
            kit.Bookshelf("Parapeito oeste", new Vector3(-5.7f, y, -6f), new Vector3(-5.7f, y, 5.4f), h);
            kit.Bookshelf("Parapeito sul", new Vector3(-5.4f, y, -5.7f), new Vector3(6f, y, -5.7f), h);
            kit.Bookshelf("Parapeito leste", new Vector3(5.7f, y, -5.4f), new Vector3(5.7f, y, 1f), h);

            kit.Goal(new Vector3(-2.5f, y, 0f), 90f);
            kit.Pole(new Vector3(-3.5f, y, 2.2f), LuzLatao);
            kit.Pole(new Vector3(-3.5f, y, -2.2f), LuzLatao);
            kit.ReadingDesk("Mesa do relojoeiro", new Vector3(2f, y, -4f), 0f);
            kit.ReadingLamp(new Vector3(2.5f, y + 0.8f, -4f), 0f);
            // mostrador gigante sobre a porta, voltado para quem chega pelo leste
            kit.DreamClock("Mostrador gigante", new Vector3(-1f, 21.5f, 0f), 4.5f, -90f, 6f);
        }

        // ------------------------------------------------------------------ cenário fora do caminho

        void Atmosfera(PhaseKit kit)
        {
            // anel de relógios em volta da torre, subindo como a espiral, todos voltados para o núcleo
            for (int k = 0; k < 12; k++)
            {
                float ang = k * 30f * Mathf.Deg2Rad;
                float raio = 20f + (k % 3) * 2f;
                Vector3 pos = new Vector3(Mathf.Sin(ang) * raio, 2f + k * 1.5f, Mathf.Cos(ang) * raio);
                float yaw = Mathf.Atan2(pos.x, pos.z) * Mathf.Rad2Deg;
                kit.DreamClock($"Relogio {k + 1}", pos, 1.8f + (k % 3) * 0.7f, yaw, (k % 2 == 0 ? 4f : -6f));
            }
            kit.FloatingBooks("Orbita baixa", new Vector3(16f, 4f, -4f), 2f, 6);
            kit.FloatingBooks("Orbita media", new Vector3(-16f, 9f, 4f), 2f, 7);
            kit.FloatingBooks("Orbita alta", new Vector3(3f, 17f, 16.5f), 2f, 6);
            kit.FloatingPages("Engrenagens de papel", new Vector3(0f, 25f, 0f), 6f, 18);
            kit.DreamMotes("Poeira do relogio", new Vector3(0f, -3f, 0f), new Vector3(30f, 4f, 30f), LuzLatao, 200);
        }
    }
}
