using System.Collections.Generic;
using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Fase 9 — "Estantes Sem Fim": um labirinto de estantes de 5 m (ninguém sobe nelas) numa grade de células de 3 m.
    /// Regra da fase: os abajures verdes em cima das estantes e as luminárias marcam a rota; becos são escuros e,
    /// em três deles, o chão é de papel e cede. Armadilhas da rota: espinhos em ciclo, uma serra que cruza o corredor
    /// entre dois nichos, e no fim uma escolha entre duas rotas (laser ou dois espinhos em ciclo).
    /// Depois do labirinto, a saída é andar no topo de estantes sobre o vazio, com um pêndulo, até a porta.
    /// O labirinto é descrito pelas ligações entre células; as paredes saem de onde não há ligação. Contas em docs/Fase9.md.
    /// </summary>
    public sealed class Fase9Builder : IPhaseBuilder
    {
        public int Order => 9;
        public string SceneName => "Fase9_EstantesSemFim";
        public PhaseTheme Theme => Tema;

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        // noite funda; destaque verde-esmeralda dos abajures de leitura
        static readonly PhaseTheme Tema = new PhaseTheme
        {
            Name = "Fase9", Floor = C(0.36f, 0.32f, 0.24f), Wall = C(0.18f, 0.20f, 0.16f), Accent = C(0.30f, 1f, 0.55f),
            Sky = C(0.04f, 0.07f, 0.06f), Equator = C(0.09f, 0.13f, 0.10f), Ground = C(0.02f, 0.03f, 0.02f),
            Fog = C(0.06f, 0.10f, 0.08f), FogDensity = 0.028f, Sun = C(0.8f, 1f, 0.85f), SunIntensity = 0.85f,
            SunEuler = new Vector3(45f, -60f, 0f), Bloom = 0.65f, Vignette = 0.35f, Saturation = 4f, PostExposure = -0.05f,
            Tint = C(0.95f, 1f, 0.95f)
        };

        const float Cell = 3f;          // célula do labirinto: 3 x 3 m; com estantes de 0,6 m sobram corredores de 2,4 m
        const float AlturaEstante = 5f; // mais que tudo o que um pulo alcança, com ou sem poder
        const int Largura = 7, Altura = 10;

        static readonly Vector2Int Entrada = new Vector2Int(3, 0); // aberta ao sul, para o vestíbulo
        static readonly Vector2Int Saida = new Vector2Int(5, 9);   // aberta ao norte, para a varanda

        // cadeias de células (pares i, j) ligadas em sequência
        static readonly int[] Rota =
        {
            3, 0, 3, 1, 2, 1, 1, 1, 1, 2, 1, 3, 1, 4, 2, 4, 3, 4, 4, 4, 5, 4, 5, 5, 5, 6,
            4, 6, 3, 6, 2, 6, 2, 7, 2, 8, 3, 8, 4, 8, 5, 8, 5, 9,
        };
        static readonly int[] RotaAlternativa = { 2, 8, 2, 9, 3, 9, 4, 9, 5, 9 };
        static readonly int[][] Becos =
        {
            new[] { 3, 1, 4, 1, 5, 1, 5, 2, 5, 3 },       // B1: segue reto para leste na primeira bifurcação
            new[] { 1, 4, 1, 5, 1, 6, 1, 7 },             // B2: segue reto para o norte
            new[] { 5, 6, 6, 6, 6, 7, 6, 8, 6, 9 },       // B3: parece ir direto para a saída
            new[] { 3, 3, 3, 4, 3, 5 },                   // nichos da serra (cruzam a rota em 3,4)
        };
        // chão de papel (cede 0,8 s depois de pisado), só nos becos
        static readonly int[] Cedem = { 5, 2, 5, 3, 1, 6, 1, 7, 6, 8, 6, 9 };

        static readonly Color LuzAlerta = new Color(1f, 0.45f, 0.35f);

        readonly HashSet<Vector2Int> usadas = new HashSet<Vector2Int>();
        readonly HashSet<(Vector2Int, Vector2Int)> abertas = new HashSet<(Vector2Int, Vector2Int)>();
        readonly HashSet<Vector2Int> horizontais = new HashSet<Vector2Int>(); // (i, j): linha z = 3j, x de 3i a 3i + 3
        readonly HashSet<Vector2Int> verticais = new HashSet<Vector2Int>();   // (i, j): linha x = 3i, z de 3j a 3j + 3

        static Vector3 Centro(int i, int j, float y = 0f) => new Vector3(Cell * i + Cell / 2f, y, Cell * j + Cell / 2f);

        public void Build(PhaseKit kit)
        {
            MontarGrafo();
            Vestibulo(kit);
            Labirinto(kit);
            Armadilhas(kit);
            Luzes(kit);
            Varanda(kit);
            Cenario(kit);

            kit.FallZone(-8f);
            kit.LibraryBackdrop(new Vector2(2f, -8f), new Vector2(22f, 60f), -4f, 16f, 6f);
        }

        // ------------------------------------------------------------------ grafo das células

        void MontarGrafo()
        {
            usadas.Clear();
            abertas.Clear();
            horizontais.Clear();
            verticais.Clear();
            Cadeia(Rota);
            Cadeia(RotaAlternativa);
            foreach (int[] beco in Becos)
                Cadeia(beco);

            for (int j = 0; j < Altura; j++)
                for (int i = 0; i < Largura; i++)
                {
                    var c = new Vector2Int(i, j);
                    if (!usadas.Contains(c))
                        continue;
                    if (!abertas.Contains((c, c + Vector2Int.down)) && c != Entrada)
                        horizontais.Add(new Vector2Int(i, j));
                    if (!abertas.Contains((c, c + Vector2Int.up)) && c != Saida)
                        horizontais.Add(new Vector2Int(i, j + 1));
                    if (!abertas.Contains((c, c + Vector2Int.left)))
                        verticais.Add(new Vector2Int(i, j));
                    if (!abertas.Contains((c, c + Vector2Int.right)))
                        verticais.Add(new Vector2Int(i + 1, j));
                }
        }

        void Cadeia(int[] c)
        {
            for (int k = 0; k + 3 < c.Length; k += 2)
            {
                var a = new Vector2Int(c[k], c[k + 1]);
                var b = new Vector2Int(c[k + 2], c[k + 3]);
                usadas.Add(a);
                usadas.Add(b);
                abertas.Add((a, b));
                abertas.Add((b, a));
            }
        }

        bool Cede(int i, int j)
        {
            for (int k = 0; k + 1 < Cedem.Length; k += 2)
                if (Cedem[k] == i && Cedem[k + 1] == j)
                    return true;
            return false;
        }

        // ------------------------------------------------------------------ vestíbulo (x 6..15, z -8..0)

        void Vestibulo(PhaseKit kit)
        {
            kit.BookPlatform("F0 Vestibulo", new Vector3(10.5f, 0f, -4f), new Vector2(9f, 8f));
            kit.Spawn(new Vector3(10.5f, 0f, -5.5f), 0f);
            kit.Bookshelf("Vestibulo oeste", new Vector3(6.3f, 0f, -8f), new Vector3(6.3f, 0f, 0f), AlturaEstante);
            kit.Bookshelf("Vestibulo leste", new Vector3(14.7f, 0f, 0f), new Vector3(14.7f, 0f, -8f), AlturaEstante);
            kit.Bookshelf("Vestibulo fundo", new Vector3(6.6f, 0f, -7.7f), new Vector3(14.4f, 0f, -7.7f), AlturaEstante);
            kit.Bookshelf("Vestibulo frente oeste", new Vector3(6.6f, 0f, 0f), new Vector3(9f, 0f, 0f), AlturaEstante);
            kit.Bookshelf("Vestibulo frente leste", new Vector3(12f, 0f, 0f), new Vector3(14.4f, 0f, 0f), AlturaEstante);
            kit.ShelfArch("Boca do labirinto", new Vector3(10.5f, 0f, -0.9f), 2.4f, 3.6f, 0f);

            kit.Carpet(new Vector3(10.5f, 0f, -4f), new Vector2(2.4f, 6.5f));
            kit.ReadingLamp(new Vector3(7.3f, 0f, -6.9f), 45f);
            kit.ReadingLamp(new Vector3(13.7f, 0f, -6.9f), -45f);
            // inclina para -X (yaw 90): o topo encosta na face da estante oeste (x 6,6)
            kit.LibraryLadder(new Vector3(7.7f, 0f, -3.5f), 4.5f, 90f);
            kit.CeilingLight(new Vector3(10.5f, 4.4f, -4f));

            kit.Sign("FASE 9", new Vector3(10.5f, 4.7f, -1.7f), 0f, 0.09f);
            kit.Sign("ESTANTES SEM FIM", new Vector3(10.5f, 4.2f, -1.7f), 0f, 0.07f);
            kit.Sign("siga os abajures verdes", new Vector3(10.5f, 3.3f, -1.7f), 0f, 0.04f);
        }

        // ------------------------------------------------------------------ labirinto: pisos e estantes

        void Labirinto(PhaseKit kit)
        {
            for (int j = 0; j < Altura; j++)
                for (int i = 0; i < Largura; i++)
                {
                    if (!usadas.Contains(new Vector2Int(i, j)))
                        continue;
                    if (Cede(i, j))
                        kit.CrumblingPlatform($"Papel {i},{j}", Centro(i, j), new Vector2(Cell, Cell), 0.8f);
                    else
                        kit.BookPlatform($"Piso {i},{j}", Centro(i, j), new Vector2(Cell, Cell), 0f, 0.5f);
                }

            // junta as arestas colineares em estantes contínuas
            for (int j = 0; j <= Altura; j++)
                for (int i = 0; i <= Largura; i++)
                {
                    if (!horizontais.Contains(new Vector2Int(i, j)) || horizontais.Contains(new Vector2Int(i - 1, j)))
                        continue;
                    int fim = i;
                    while (horizontais.Contains(new Vector2Int(fim + 1, j)))
                        fim++;
                    kit.Bookshelf($"Labirinto H{j} {i}-{fim}", new Vector3(i * Cell, 0f, j * Cell),
                        new Vector3((fim + 1) * Cell, 0f, j * Cell), AlturaEstante);
                }
            for (int i = 0; i <= Largura; i++)
                for (int j = 0; j <= Altura; j++)
                {
                    if (!verticais.Contains(new Vector2Int(i, j)) || verticais.Contains(new Vector2Int(i, j - 1)))
                        continue;
                    int fim = j;
                    while (verticais.Contains(new Vector2Int(i, fim + 1)))
                        fim++;
                    kit.Bookshelf($"Labirinto V{i} {j}-{fim}", new Vector3(i * Cell, 0f, j * Cell),
                        new Vector3(i * Cell, 0f, (fim + 1) * Cell), AlturaEstante);
                }
        }

        // ------------------------------------------------------------------ armadilhas

        void Armadilhas(PhaseKit kit)
        {
            // corredor (1,1)->(1,4): dois espinhos em ciclo cobrindo 1,8 dos 2,4 m livres; refúgio de 1,2 m entre eles
            kit.Spikes("Ritmo A", Centro(1, 2), true, 1.2f, 1.6f, 0f);
            kit.Spikes("Ritmo B", Centro(1, 3), true, 1.2f, 1.6f, 1.4f);

            // serra do cruzamento: vai e volta entre os nichos (3,3) e (3,5), cruzando a rota em (3,4); 4 s por ciclo
            kit.Saw("Serra do cruzamento", Centro(3, 3), new Vector3(0f, 0f, 6f), 4f);

            // escolha final: laser em (4,8) ou dois espinhos em ciclo em (3,9) e (4,9)
            kit.Laser("Laser da galeria", Centro(4, 8), 90f, 1.5f, 1.8f, 0f);
            kit.Spikes("Alternativa A", Centro(3, 9), true, 1.2f, 1.6f, 0f);
            kit.Spikes("Alternativa B", Centro(4, 9), true, 1.2f, 1.6f, 1.4f);
        }

        // ------------------------------------------------------------------ luzes: só a rota é iluminada

        void Luzes(PhaseKit kit)
        {
            // abajures verdes no topo das estantes, na quina da passagem certa de cada curva e bifurcação
            int[] passagens =
            {
                3, -1, 3, 0,   3, 1, 2, 1,   1, 1, 1, 2,   1, 4, 2, 4,   5, 4, 5, 5,   5, 6, 4, 6,
                2, 6, 2, 7,   2, 8, 3, 8,   2, 8, 2, 9,   5, 8, 5, 9,   5, 9, 5, 10,
            };
            for (int k = 0; k + 3 < passagens.Length; k += 4)
                AbajurNaPassagem(kit, new Vector2Int(passagens[k], passagens[k + 1]), new Vector2Int(passagens[k + 2], passagens[k + 3]));

            // luminárias no tom do tema, uma célula sim, outra não, ao longo da rota
            for (int k = 0; k + 1 < Rota.Length; k += 4)
                kit.CeilingLight(Centro(Rota[k], Rota[k + 1], 4.4f));
            kit.CeilingLight(Centro(5, 9, 4.4f));
            kit.CeilingLight(Centro(3, 9, 4.4f));
            // aviso sobre o cruzamento da serra
            kit.CeilingLight(Centro(3, 4, 4.4f), 0f, LuzAlerta);
        }

        void AbajurNaPassagem(PhaseKit kit, Vector2Int a, Vector2Int b)
        {
            Vector2Int p1, p2;
            if (a.x == b.x)
            {
                int j = Mathf.Max(a.y, b.y);
                p1 = new Vector2Int(a.x, j);
                p2 = new Vector2Int(a.x + 1, j);
            }
            else
            {
                int i = Mathf.Max(a.x, b.x);
                p1 = new Vector2Int(i, a.y);
                p2 = new Vector2Int(i, a.y + 1);
            }
            Vector2Int p;
            if (TemParede(p1)) p = p1;
            else if (TemParede(p2)) p = p2;
            else return;

            var pos = new Vector3(p.x * Cell, AlturaEstante, p.y * Cell);
            Vector3 alvo = new Vector3((p1.x + p2.x) * Cell / 2f, AlturaEstante, (p1.y + p2.y) * Cell / 2f);
            float yaw = Mathf.Atan2(alvo.x - pos.x, alvo.z - pos.z) * Mathf.Rad2Deg;
            kit.ReadingLamp(pos, yaw);
        }

        bool TemParede(Vector2Int p) =>
            horizontais.Contains(new Vector2Int(p.x - 1, p.y)) || horizontais.Contains(p) ||
            verticais.Contains(new Vector2Int(p.x, p.y - 1)) || verticais.Contains(p);

        // ------------------------------------------------------------------ saída: varanda, pontes de estantes e porta

        void Varanda(PhaseKit kit)
        {
            // E1: varanda logo ao norte da saída (x 14..19, z 30..35), com o Escudo opcional
            kit.BookPlatform("E1 Varanda", new Vector3(16.5f, 0f, 32.5f), new Vector2(5f, 5f));
            kit.Pickup(PickupKind.Shield, new Vector3(15f, 0f, 33.5f));
            kit.Carpet(new Vector3(16.5f, 0f, 32.5f), new Vector2(2.4f, 4f));
            kit.ReadingLamp(new Vector3(18.5f, 0f, 34.5f), -135f);
            kit.CeilingLight(new Vector3(16.5f, 4.4f, 32.5f));
            kit.Sign("ande sobre as estantes", new Vector3(16.5f, 3.4f, 35.2f), 0f, 0.045f);

            // pontes: estantes deitadas no vazio, com o topo (0,6 m de largura) como caminho
            kit.Bookshelf("Ponte 1", new Vector3(16.5f, -3f, 35f), new Vector3(16.5f, -3f, 42f), 3f);     // topo 0, z 35..42
            kit.Bookshelf("Ponte 2", new Vector3(16.5f, -2.5f, 44f), new Vector3(16.5f, -2.5f, 50f), 3f); // topo 0,5, z 44..50

            // pêndulo atravessando a Ponte 2 (balança em X); pivô a 4,3 m
            kit.Pendulum("Pendulo da ponte", new Vector3(16.5f, 0.5f, 47f), 50f, 2.8f, 0f, 0f);
            kit.BookPlatform("Viga do pendulo", new Vector3(16.5f, 4.8f, 47f), new Vector2(6.4f, 0.6f), 0f, 0.5f);
            kit.BackdropShelf("Apoio da viga oeste", new Vector3(13.6f, -4f, 46.7f), new Vector3(13.6f, -4f, 47.3f), 8.8f);
            kit.BackdropShelf("Apoio da viga leste", new Vector3(19.4f, -4f, 47.3f), new Vector3(19.4f, -4f, 46.7f), 8.8f);
            kit.CeilingLight(new Vector3(16.5f, 4.2f, 41f));

            // E2: patamar da porta (x 12,5..20,5, z 52..60)
            kit.OpenBookPlatform("E2 Patamar da porta", new Vector3(16.5f, 0.5f, 56f), new Vector2(8f, 8f));
            kit.Carpet(new Vector3(16.5f, 0.5f, 55.5f), new Vector2(2.4f, 5f));
            kit.ReadingLamp(new Vector3(13.2f, 0.5f, 59.3f), 135f);
            kit.ReadingLamp(new Vector3(19.8f, 0.5f, 59.3f), -135f);
            kit.CeilingLight(new Vector3(16.5f, 4.4f, 55f));
            kit.Goal(new Vector3(16.5f, 0.5f, 58.5f), 180f);
        }

        // ------------------------------------------------------------------ cenário

        void Cenario(PhaseKit kit)
        {
            // marco visível por cima das estantes de dentro do labirinto
            kit.DreamClock("Relogio do fim", new Vector3(16.5f, 10f, 66f), 3.5f);
            kit.FloatingBooks("Sobre o labirinto", new Vector3(10.5f, 8.5f, 15f), 4f, 8);
            kit.FloatingBooks("Sobre a saida", new Vector3(21f, 9f, 24f), 2.5f, 6);
            kit.FloatingPages("Sobre o labirinto", new Vector3(5f, 8f, 25f), 2.5f, 12);
            kit.FloatingBooks("Vazio das pontes", new Vector3(11.5f, 1.5f, 44f), 2f, 6);
            kit.FloatingPages("Vazio das pontes", new Vector3(21.5f, 2.5f, 41f), 2f, 10);
            kit.DreamMotes("Labirinto", new Vector3(12f, 3f, 15f), new Vector3(20f, 5f, 30f), null, 200);
        }
    }
}
