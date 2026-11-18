using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Decora as Fases 1 e 2 (cenas feitas à mão) com a estética da biblioteca do sonho, sem mexer na jogabilidade.
    ///
    /// Regras seguidas (veja docs/BIBLIOTECA_SONHO.md):
    /// - só peças sem colisão (estantes de fundo, livros e páginas flutuando, relógios, abajures, escada, tapetes, partículas);
    /// - nada no volume jogável: as peças laterais ficam a pelo menos ~4 m das bordas andáveis (além do alcance da câmera
    ///   em terceira pessoa, que tem 4 m de distância e não colide com cenário sem colisor), as de cima a pelo menos 2,5 m
    ///   acima do pulo mais alto, e tapetes só sobre o patamar de entrada/chegada (largos e sem armadilha);
    /// - nada entre o jogador e as placas, as pistas ou o portal; as notas de revisão deitadas a leste (x ≈ 12..37,
    ///   z ≈ 5..16) continuam livres, por isso a fileira de estantes do lado leste só começa depois delas;
    /// - tudo fica sob o objeto "Cenario biblioteca": rodar de novo apaga e recria (PhaseKit.Dress).
    ///
    /// As estantes de fundo são módulos de 3,6 m repetidos: o kit gera a malha de cada estante (milhares de livros) e ela
    /// fica gravada dentro da cena. Duplicar o módulo com Object.Instantiate reaproveita a mesma malha, o que mantém o
    /// arquivo .unity pequeno (cada estante única de 12 m de altura custaria vários MB).
    /// </summary>
    public static class ExistingPhaseDressing
    {
        const string PhaseOnePath = "Assets/Scenes/DS07_Fase1_Jogavel.unity";
        const string PhaseTwoPath = "Assets/Scenes/DS09_Fase2_Jogavel.unity";

        const float ModuleLength = 3.6f;   // 3 vãos de 1,2 m
        const float BackdropBase = -2.5f;  // a base some na neblina abaixo dos pisos
        const float LowShelfHeight = 1.5f; // balcões baixos que seguram os abajures

        [MenuItem("Dreamscape/Fases/Decorar Fases 1 e 2 (biblioteca)")]
        public static void DressAll()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Dress(PhaseOnePath, PhaseTheme.Fase1, DressPhaseOne);
            Dress(PhaseTwoPath, PhaseTheme.Fase2, DressPhaseTwo);
            AssetDatabase.SaveAssets();
        }

        static void Dress(string path, PhaseTheme theme, System.Action<PhaseKit> dress)
        {
            if (!System.IO.File.Exists(path))
            {
                Debug.LogWarning($"Dreamscape: cena não encontrada para decorar: {path}");
                return;
            }
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            PhaseKit kit = PhaseKit.Dress(scene, theme);
            dress(kit);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Dreamscape: cenário de biblioteca aplicado em {path}.");
        }

        // ------------------------------------------------------------------ Fase 1

        /// <summary>
        /// Fase 1 (DS07). Arena fechada por muros em x = ±10 (topo 2,5 m) de z = -0,5 a 29,5; pisos com topo de 0 a 2 m;
        /// patamar final (x -9..9, z 23..29, topo 2) e porta em (0, 2..5, 28). Pulo duplo leva a ~4,2 m de altura.
        /// A câmera passa por cima dos muros só perto do patamar e alcança no máximo |x| ≈ 14 e z ≈ 33,5.
        /// </summary>
        public static void DressPhaseOne(PhaseKit kit)
        {
            var shelves = new ShelfModules(kit, "Fase 1", 9.5f, 11.5f);

            // moldura de estantes: oeste, ao fundo (atrás da porta) e um trecho a leste depois das notas de revisão
            shelves.Row("Fundo oeste", new Vector3(-16f, BackdropBase, -6f), new Vector3(-16f, BackdropBase, 38.6f), 0);
            shelves.Row("Fundo norte", new Vector3(-16f, BackdropBase, 38.6f), new Vector3(16.3f, BackdropBase, 38.6f), 1);
            shelves.Row("Fundo leste", new Vector3(16f, BackdropBase, 30f), new Vector3(16f, BackdropBase, 38.6f), 2);

            // balcões baixos fora do muro oeste, com abajures voltados para a arena
            var low = new ShelfModules(kit, "Fase 1 balcao", LowShelfHeight);
            foreach (float z in new[] { 3f, 10f, 17f })
            {
                low.Place(0, $"Balcao oeste z{z:0}", new Vector3(-13.8f, 0f, z), -90f);
                kit.ReadingLamp(new Vector3(-13.8f, LowShelfHeight, z), 90f);
            }
            kit.LibraryLadder(new Vector3(-14.2f, 0f, 6.5f), 6f, 90f); // encostada na estante oeste (face em x = -15,7)

            // marcos no horizonte: relógio acima e atrás da porta, outro na parede oeste
            kit.DreamClock("Fase 1 horizonte", new Vector3(0f, 11f, 36.5f), 2.8f);
            kit.DreamClock("Fase 1 oeste", new Vector3(-14.3f, 8f, 21f), 1.4f, -90f);

            kit.FloatingBooks("Fase 1 oeste", new Vector3(-13.5f, 5.5f, 9f), 1f, 5);
            kit.FloatingBooks("Fase 1 patamar oeste", new Vector3(-14f, 9f, 27f), 1f, 5);
            kit.FloatingBooks("Fase 1 alem da porta oeste", new Vector3(-6f, 8.5f, 35.5f), 1.5f, 6);
            kit.FloatingBooks("Fase 1 alem da porta leste", new Vector3(7f, 9.5f, 35.5f), 1.5f, 6);
            kit.FloatingBooks("Fase 1 leste", new Vector3(15f, 8f, 34f), 1f, 4);

            kit.FloatingPages("Fase 1 salto duplo", new Vector3(-14f, 6.5f, 15f), 1.2f, 10);
            kit.FloatingPages("Fase 1 relogio", new Vector3(0f, 11f, 35.5f), 2.5f, 12);

            kit.DreamMotes("Fase 1 ceu", new Vector3(0f, 10.5f, 14.5f), new Vector3(20f, 4f, 30f), null, 140);
            kit.DreamMotes("Fase 1 oeste", new Vector3(-15f, 3f, 16f), new Vector3(2.5f, 12f, 44f), null, 120);
            kit.DreamMotes("Fase 1 fundo", new Vector3(0f, 5f, 35.5f), new Vector3(30f, 12f, 3f), null, 120);

            // tapete de chegada no patamar final (largo e seguro), parando antes do gatilho da porta (z 27,8)
            kit.Carpet(new Vector3(0f, 2f, 25.3f), new Vector2(2.6f, 3.6f));

            shelves.DiscardUnused();
            low.DiscardUnused();
        }

        // ------------------------------------------------------------------ Fase 2

        /// <summary>
        /// Fase 2 (DS09). Sem muros; todos os pisos com topo em y = 0: entrada e bifurcação (x -8..8, z -1..9),
        /// rota enganosa (x -2..2, z 9..17, termina no vazio), desvio e patamar de leitura (x -8..-4, z 9..19),
        /// pisos temporários (x -8..-4, z 19..27) e patamar final (x -8..2, z 27..33) com o portal em (-3, 0..3, 32).
        /// A câmera alcança até 4 m além dessas bordas e no máximo y ≈ 6,4.
        /// O eixo da rota enganosa (x -3..3 depois de z = 17) fica sem marcos novos para não mudar a leitura da pegadinha,
        /// e nada ilumina os pisos temporários (as cores deles avisam o tempo da ponte).
        /// </summary>
        public static void DressPhaseTwo(PhaseKit kit)
        {
            var shelves = new ShelfModules(kit, "Fase 2", 9.5f, 11.5f);

            shelves.Row("Fundo oeste", new Vector3(-15f, BackdropBase, -9f), new Vector3(-15f, BackdropBase, 45f), 0);
            shelves.Row("Fundo norte", new Vector3(-15f, BackdropBase, 45f), new Vector3(10f, BackdropBase, 45f), 1);
            // a leste só depois de z = 28: nenhuma linha de visão do jogador até as notas de revisão cruza esse trecho
            shelves.Row("Fundo leste", new Vector3(10f, BackdropBase, 28f), new Vector3(10f, BackdropBase, 45f), 2);

            // abajures longe da ponte: um ao lado da entrada e outro ao lado do patamar final
            var low = new ShelfModules(kit, "Fase 2 balcao", LowShelfHeight);
            foreach (float z in new[] { 1f, 31f })
            {
                low.Place(0, $"Balcao oeste z{z:0}", new Vector3(-13.4f, 0f, z), -90f);
                kit.ReadingLamp(new Vector3(-13.4f, LowShelfHeight, z), 90f);
            }
            kit.LibraryLadder(new Vector3(-13.2f, 0f, 16f), 6f, 90f); // encostada na estante oeste (face em x = -14,7)

            kit.DreamClock("Fase 2 horizonte", new Vector3(-3f, 11.5f, 42.5f), 3.2f);
            kit.DreamClock("Fase 2 oeste", new Vector3(-13.8f, 8.5f, 20f), 1.4f, -90f);

            kit.FloatingBooks("Fase 2 oeste entrada", new Vector3(-13.5f, 5f, 3f), 1f, 5);
            kit.FloatingBooks("Fase 2 oeste ponte", new Vector3(-13.5f, 7f, 24f), 1f, 5);
            kit.FloatingBooks("Fase 2 leste entrada", new Vector3(13.5f, 5f, 2f), 1.2f, 5);
            kit.FloatingBooks("Fase 2 vazio leste", new Vector3(9f, 8.5f, 24f), 1.2f, 5);
            kit.FloatingBooks("Fase 2 alem do portal oeste", new Vector3(-8f, 9f, 39f), 1.5f, 6);
            kit.FloatingBooks("Fase 2 alem do portal leste", new Vector3(2f, 9.5f, 40f), 1.5f, 6);

            kit.FloatingPages("Fase 2 desvio", new Vector3(-13.5f, 6f, 12f), 1f, 10);
            kit.FloatingPages("Fase 2 relogio", new Vector3(-3f, 11.5f, 41.5f), 2.5f, 12);

            kit.DreamMotes("Fase 2 ceu", new Vector3(-1f, 10f, 16f), new Vector3(18f, 4f, 36f), null, 140);
            kit.DreamMotes("Fase 2 oeste", new Vector3(-14f, 3f, 18f), new Vector3(2f, 12f, 52f), null, 120);
            kit.DreamMotes("Fase 2 fundo", new Vector3(-3f, 5f, 40f), new Vector3(24f, 12f, 4f), null, 120);

            // tapetes só na entrada (sob o spawn, antes do coletável em z = 3) e no patamar final,
            // longe dos pisos temporários, da faixa de acionamento e do gatilho "Chegada da ponte" (z 27,4..28,2)
            kit.Carpet(new Vector3(0f, 0f, 0.6f), new Vector2(3.2f, 2.2f));
            kit.Carpet(new Vector3(-3f, 0f, 30f), new Vector2(2.6f, 3.4f));

            shelves.DiscardUnused();
            low.DiscardUnused();
        }

        // ------------------------------------------------------------------ módulos de estante

        /// <summary>
        /// Algumas estantes-modelo (sem colisão) de <see cref="ModuleLength"/> m, uma por altura. A primeira vez que um
        /// modelo é usado ele mesmo vai para o lugar; as próximas são cópias que dividem a mesma malha.
        /// </summary>
        sealed class ShelfModules
        {
            readonly GameObject[] masters;
            readonly bool[] used;

            public ShelfModules(PhaseKit kit, string tag, params float[] heights)
            {
                masters = new GameObject[heights.Length];
                used = new bool[heights.Length];
                for (int i = 0; i < heights.Length; i++)
                    masters[i] = kit.BackdropShelf($"{tag} modulo {i + 1}", Vector3.zero, new Vector3(ModuleLength, 0f, 0f), heights[i]);
            }

            /// <summary>Coloca o módulo <paramref name="variant"/> com a base centrada em <paramref name="baseCenter"/>; ele corre ao longo do X local.</summary>
            public GameObject Place(int variant, string name, Vector3 baseCenter, float yaw)
            {
                GameObject master = masters[variant];
                GameObject go;
                if (!used[variant])
                {
                    go = master;
                    used[variant] = true;
                }
                else
                {
                    go = Object.Instantiate(master, master.transform.parent);
                }
                go.name = "Estante - " + name;
                go.transform.SetPositionAndRotation(baseCenter, Quaternion.Euler(0f, yaw, 0f));
                return go;
            }

            /// <summary>
            /// Fileira de módulos de <paramref name="from"/> a <paramref name="to"/> (base em from.y), centrada no trecho.
            /// Alterna as alturas e afunda alguns módulos para o topo da fileira não ficar reto.
            /// </summary>
            public void Row(string name, Vector3 from, Vector3 to, int seed)
            {
                Vector3 flat = new Vector3(to.x - from.x, 0f, to.z - from.z);
                float length = flat.magnitude;
                int count = Mathf.Max(1, Mathf.FloorToInt(length / ModuleLength));
                Vector3 dir = flat / Mathf.Max(length, 0.0001f);
                float yaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg - 90f; // o mesmo cálculo de PhaseKit.Bookshelf
                float start = (length - count * ModuleLength) / 2f;
                float[] drops = { 0f, -0.7f, -0.3f, -1.1f, -0.5f };
                for (int i = 0; i < count; i++)
                {
                    int k = i + seed;
                    int variant = masters.Length > 1 && (k * 7 + 3) % 3 == 0 ? 1 : 0;
                    Vector3 c = from + dir * (start + ModuleLength * (i + 0.5f));
                    c.y = from.y + drops[k % drops.Length];
                    Place(variant, $"{name} {i + 1}", c, yaw);
                }
            }

            /// <summary>Apaga os modelos que não chegaram a ser colocados (ficariam na origem, no meio da fase).</summary>
            public void DiscardUnused()
            {
                for (int i = 0; i < masters.Length; i++)
                    if (!used[i] && masters[i] != null)
                        Object.DestroyImmediate(masters[i]);
            }
        }
    }
}
