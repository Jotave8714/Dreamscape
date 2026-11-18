using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Peças da biblioteca onírica: estantes, pilhas de livros que servem de plataforma, livros abertos gigantes,
    /// livros e páginas flutuando, abajures de leitura, escadas, tapetes, relógios e partículas de sonho.
    /// Tudo é montado com primitivas e materiais do kit; os livros de cada peça viram uma única malha combinada.
    /// Peças "decorativas" não têm colisor; as que dizem "plataforma" ou "sólida" têm colisão simples em caixa.
    /// A aparência é determinística: o mesmo nome gera sempre os mesmos livros.
    /// </summary>
    public sealed partial class PhaseKit
    {
        // lombadas: vinho, marinho, verde-floresta, ocre, ameixa, petróleo, creme
        static readonly Color[] SpineColors =
        {
            new Color(0.48f, 0.12f, 0.16f), new Color(0.13f, 0.18f, 0.40f), new Color(0.12f, 0.32f, 0.20f),
            new Color(0.72f, 0.52f, 0.18f), new Color(0.38f, 0.18f, 0.42f), new Color(0.10f, 0.36f, 0.40f),
            new Color(0.85f, 0.78f, 0.62f),
        };

        static Mesh cubeMesh, sphereMesh, cylinderMesh, quadMesh;

        static Mesh Builtin(PrimitiveType type, ref Mesh cache)
        {
            if (cache == null)
            {
                GameObject temp = GameObject.CreatePrimitive(type);
                cache = temp.GetComponent<MeshFilter>().sharedMesh;
                Object.DestroyImmediate(temp);
            }
            return cache;
        }

        static Mesh Cube => Builtin(PrimitiveType.Cube, ref cubeMesh);
        static Mesh Sphere => Builtin(PrimitiveType.Sphere, ref sphereMesh);
        static Mesh Cylinder => Builtin(PrimitiveType.Cylinder, ref cylinderMesh);
        static Mesh Quad => Builtin(PrimitiveType.Quad, ref quadMesh);

        Material SpineMaterial(int i) => KitMaterial($"Livro_{i}", SpineColors[i], false);
        Material Wood => KitMaterial("Madeira", new Color(0.30f, 0.19f, 0.12f), false);
        Material DarkWood => KitMaterial("MadeiraEscura", new Color(0.16f, 0.10f, 0.07f), false);
        Material Paper => KitMaterial("Papel", new Color(0.93f, 0.89f, 0.78f), false);
        Material Brass => KitMaterial("Latao", new Color(0.78f, 0.60f, 0.28f), false);
        Material LampShade => KitMaterial("CupulaVerde", new Color(0.25f, 0.75f, 0.40f), true);
        Material Rug => KitMaterial("Tapete", new Color(0.42f, 0.10f, 0.14f), false);
        Material PageGlow => KitMaterial("PaginaBrilho", new Color(1f, 0.95f, 0.8f), true);
        Material Accent => ThemeMaterial("Friso", Theme.Accent, true);

        // ------------------------------------------------------------------ malha combinada

        /// <summary>Junta caixas, esferas e cilindros num só objeto (um submesh por material).</summary>
        sealed class MeshBatch
        {
            readonly Dictionary<Material, List<CombineInstance>> parts = new Dictionary<Material, List<CombineInstance>>();

            public void Add(Mesh mesh, Material material, Vector3 localPos, Quaternion localRot, Vector3 scale)
            {
                if (!parts.TryGetValue(material, out List<CombineInstance> list))
                    parts[material] = list = new List<CombineInstance>();
                list.Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(localPos, localRot, scale) });
            }

            public void Box(Material material, Vector3 localPos, Vector3 size, float yaw = 0f, float roll = 0f, float pitch = 0f) =>
                Add(PhaseKit.Cube, material, localPos, Quaternion.Euler(pitch, yaw, roll), size);

            public GameObject Build(string name, Transform parent)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                if (parts.Count == 0)
                    return go;

                var materials = new List<Material>();
                var subMeshes = new List<CombineInstance>();
                foreach (KeyValuePair<Material, List<CombineInstance>> pair in parts)
                {
                    var sub = new Mesh { indexFormat = IndexFormat.UInt32 };
                    sub.CombineMeshes(pair.Value.ToArray(), true, true);
                    materials.Add(pair.Key);
                    subMeshes.Add(new CombineInstance { mesh = sub, transform = Matrix4x4.identity });
                }
                var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(subMeshes.ToArray(), false, false);
                mesh.RecalculateBounds();
                foreach (CombineInstance c in subMeshes)
                    Object.DestroyImmediate(c.mesh);

                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = materials.ToArray();
                renderer.shadowCastingMode = ShadowCastingMode.On;
                return go;
            }
        }

        static System.Random Seeded(string name)
        {
            unchecked
            {
                int hash = (int)2166136261;
                foreach (char c in name)
                    hash = (hash ^ c) * 16777619;
                return new System.Random(hash);
            }
        }

        static float Range(System.Random r, float min, float max) => min + (float)r.NextDouble() * (max - min);

        // enche uma prateleira de `length` metros (ao longo de X local) com livros de pé e algumas pilhas deitadas
        void FillShelf(MeshBatch batch, System.Random r, float xStart, float length, float y, float z, float maxHeight, float facing)
        {
            float x = xStart;
            float end = xStart + length;
            while (x < end - 0.08f)
            {
                if (r.NextDouble() < 0.07)
                {
                    x += Range(r, 0.15f, 0.35f); // lacuna
                    continue;
                }
                if (r.NextDouble() < 0.08 && end - x > 0.45f)
                {
                    // pilha deitada
                    int count = r.Next(2, 5);
                    float w = Range(r, 0.32f, 0.42f);
                    float h = 0f;
                    for (int i = 0; i < count; i++)
                    {
                        float t = Range(r, 0.05f, 0.08f);
                        batch.Box(SpineMaterial(r.Next(SpineColors.Length)), new Vector3(x + w / 2f, y + h + t / 2f, z),
                            new Vector3(w, t, Range(r, 0.24f, 0.3f)), Range(r, -6f, 6f));
                        h += t;
                    }
                    x += w + 0.04f;
                    continue;
                }
                float width = Range(r, 0.05f, 0.11f);
                float height = Mathf.Min(maxHeight, Range(r, 0.26f, 0.44f));
                float depth = Range(r, 0.22f, 0.3f);
                float lean = r.NextDouble() < 0.06 ? Range(r, 6f, 14f) : 0f;
                batch.Box(SpineMaterial(r.Next(SpineColors.Length)),
                    new Vector3(x + width / 2f, y + height / 2f, z + facing * (0.15f - depth / 2f)),
                    new Vector3(width, height, depth), 0f, -lean);
                x += width + 0.005f;
            }
        }

        // ------------------------------------------------------------------ estantes

        /// <summary>
        /// Estante de dois lados, de <paramref name="from"/> a <paramref name="to"/> (altura Y de from é a base), cheia de livros.
        /// Com <paramref name="solid"/> tem colisão como uma parede de 0,6 m de espessura.
        /// </summary>
        public GameObject Bookshelf(string name, Vector3 from, Vector3 to, float height = 3f, bool solid = true)
        {
            Vector3 flat = new Vector3(to.x - from.x, 0f, to.z - from.z);
            float length = flat.magnitude;
            float yaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg - 90f; // a estante corre ao longo do X local
            GameObject root = Empty("Estante - " + name, solid ? geometry : props, (from + new Vector3(to.x, from.y, to.z)) / 2f, yaw);

            System.Random r = Seeded(name);
            var batch = new MeshBatch();
            const float depth = 0.6f;
            const float side = 0.08f;
            // laterais, fundo, topo e base
            batch.Box(Wood, new Vector3(-length / 2f + side / 2f, height / 2f, 0f), new Vector3(side, height, depth));
            batch.Box(Wood, new Vector3(length / 2f - side / 2f, height / 2f, 0f), new Vector3(side, height, depth));
            batch.Box(DarkWood, new Vector3(0f, height / 2f, 0f), new Vector3(length, height, 0.04f));
            batch.Box(Wood, new Vector3(0f, height - 0.05f, 0f), new Vector3(length + 0.12f, 0.1f, depth + 0.08f));
            batch.Box(Wood, new Vector3(0f, 0.06f, 0f), new Vector3(length, 0.12f, depth + 0.04f));
            // divisórias verticais a cada ~1,2 m
            int bays = Mathf.Max(1, Mathf.RoundToInt(length / 1.2f));
            float bay = length / bays;
            for (int i = 1; i < bays; i++)
                batch.Box(Wood, new Vector3(-length / 2f + bay * i, height / 2f, 0f), new Vector3(0.05f, height - 0.1f, depth));
            // prateleiras a cada 0,5 m
            int shelves = Mathf.Max(1, Mathf.FloorToInt((height - 0.3f) / 0.5f));
            float gap = (height - 0.22f) / shelves;
            for (int s = 0; s < shelves; s++)
            {
                float y = 0.12f + gap * s;
                if (s > 0)
                    batch.Box(Wood, new Vector3(0f, y - 0.02f, 0f), new Vector3(length - 0.1f, 0.04f, depth));
                for (int b = 0; b < bays; b++)
                {
                    float xs = -length / 2f + bay * b + 0.06f;
                    FillShelf(batch, r, xs, bay - 0.1f, y, 0.15f, gap - 0.06f, 1f);
                    FillShelf(batch, r, xs, bay - 0.1f, y, -0.15f, gap - 0.06f, -1f);
                }
            }
            GameObject visual = batch.Build("Livros e madeira", root.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            if (solid)
                Box(root, new Vector3(0f, height / 2f, 0f), new Vector3(length, height, depth));
            return root;
        }

        /// <summary>Estante alta só de cenário, para os limites da fase (sem colisão). Use longe do caminho jogável.</summary>
        public GameObject BackdropShelf(string name, Vector3 from, Vector3 to, float height = 8f) =>
            Bookshelf(name, from, to, height, false);

        // ------------------------------------------------------------------ plataformas de livros

        /// <summary>
        /// Plataforma feita de uma pilha de livros gigantes. Mesmo contrato de <see cref="Platform"/>: <paramref name="topCenter"/>
        /// é o centro da face de cima e <paramref name="size"/> é X por Z; a colisão é uma caixa de 0,5 m sob o topo.
        /// Os livros de baixo descem <paramref name="stackDepth"/> metros (só cenário).
        /// </summary>
        public GameObject BookPlatform(string name, Vector3 topCenter, Vector2 size, float yaw = 0f, float stackDepth = 1.2f)
        {
            GameObject root = Empty(name, geometry, topCenter, yaw);
            System.Random r = Seeded(name);
            var batch = new MeshBatch();
            float y = 0f;
            bool first = true;
            while (-y < stackDepth || first)
            {
                float thick = first ? 0.5f : Range(r, 0.3f, 0.55f);
                Vector2 s = first ? size : new Vector2(size.x * Range(r, 0.92f, 1.08f), size.y * Range(r, 0.92f, 1.08f));
                float twist = first ? 0f : Range(r, -7f, 7f);
                Vector3 offset = first ? Vector3.zero : new Vector3(Range(r, -0.15f, 0.15f), 0f, Range(r, -0.15f, 0.15f));
                GiantBook(batch, r, offset + new Vector3(0f, y - thick / 2f, 0f), new Vector3(s.x, thick, s.y), twist);
                y -= thick;
                first = false;
            }
            batch.Build("Pilha de livros", root.transform).transform.localRotation = Quaternion.identity;
            Box(root, new Vector3(0f, -0.25f, 0f), new Vector3(size.x, 0.5f, size.y));
            return root;
        }

        // livro fechado deitado: capa em cima e embaixo, miolo de papel recuado; a lombada fica do lado -X
        void GiantBook(MeshBatch batch, System.Random r, Vector3 center, Vector3 size, float yaw)
        {
            Material cover = SpineMaterial(r.Next(SpineColors.Length - 1));
            Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
            float c = Mathf.Min(0.06f, size.y * 0.15f);
            batch.Add(Cube, cover, center + rot * new Vector3(0f, size.y / 2f - c / 2f, 0f), rot, new Vector3(size.x, c, size.z));
            batch.Add(Cube, cover, center + rot * new Vector3(0f, -size.y / 2f + c / 2f, 0f), rot, new Vector3(size.x, c, size.z));
            batch.Add(Cube, cover, center + rot * new Vector3(-size.x / 2f + 0.04f, 0f, 0f), rot, new Vector3(0.08f, size.y, size.z));
            batch.Add(Cube, Paper, center + rot * new Vector3(0.02f, 0f, 0f), rot, new Vector3(size.x - 0.12f, size.y - 2f * c, size.z - 0.08f));
            // faixas douradas na lombada
            batch.Add(Cube, Brass, center + rot * new Vector3(-size.x / 2f + 0.005f, size.y * 0.25f, 0f), rot, new Vector3(0.09f, 0.04f, size.z + 0.01f));
            batch.Add(Cube, Brass, center + rot * new Vector3(-size.x / 2f + 0.005f, -size.y * 0.25f, 0f), rot, new Vector3(0.09f, 0.04f, size.z + 0.01f));
        }

        /// <summary>
        /// Livro gigante aberto que serve de plataforma (topo plano em <paramref name="topCenter"/>, <paramref name="size"/> X por Z,
        /// lombada ao longo do Z local). As páginas brilham de leve: bom para patamares de chegada e marcos.
        /// </summary>
        public GameObject OpenBookPlatform(string name, Vector3 topCenter, Vector2 size, float yaw = 0f)
        {
            GameObject root = Empty(name, geometry, topCenter, yaw);
            System.Random r = Seeded(name);
            var batch = new MeshBatch();
            Material cover = SpineMaterial(r.Next(SpineColors.Length - 1));
            float half = size.x / 2f;
            // capa (um pouco maior que as páginas)
            batch.Box(cover, new Vector3(0f, -0.42f, 0f), new Vector3(size.x + 0.3f, 0.12f, size.y + 0.3f));
            // dois blocos de páginas, levemente inclinados para o meio
            batch.Box(Paper, new Vector3(-half / 2f, -0.19f, 0f), new Vector3(half - 0.05f, 0.36f, size.y), 0f, 1.5f);
            batch.Box(Paper, new Vector3(half / 2f, -0.19f, 0f), new Vector3(half - 0.05f, 0.36f, size.y), 0f, -1.5f);
            // vinco e linhas de texto
            batch.Box(DarkWood, new Vector3(0f, -0.02f, 0f), new Vector3(0.06f, 0.04f, size.y));
            for (int side = -1; side <= 1; side += 2)
                for (float z = -size.y / 2f + 0.5f; z < size.y / 2f - 0.4f; z += 0.32f)
                {
                    float w = (half - 0.6f) * Range(r, 0.55f, 1f);
                    batch.Box(DarkWood, new Vector3(side * (0.3f + w / 2f), 0.005f, z), new Vector3(w, 0.01f, 0.06f));
                }
            // fita marcadora
            batch.Box(Accent, new Vector3(Range(r, -0.3f, 0.3f), -0.3f, size.y / 2f + 0.35f), new Vector3(0.18f, 0.6f, 0.02f));
            batch.Build("Livro aberto", root.transform).transform.localRotation = Quaternion.identity;
            Box(root, new Vector3(0f, -0.25f, 0f), new Vector3(size.x, 0.5f, size.y));
            return root;
        }

        // ------------------------------------------------------------------ cenário flutuante

        /// <summary>Livros voando devagar em volta de <paramref name="center"/> (cenário, sem colisão). Mantenha fora do caminho.</summary>
        public GameObject FloatingBooks(string name, Vector3 center, float radius = 3f, int count = 6)
        {
            GameObject root = Empty("Livros flutuantes - " + name, props, center);
            System.Random r = Seeded(name);
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count + Range(r, -0.3f, 0.3f);
                Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius * Range(r, 0.6f, 1f), Range(r, -0.8f, 0.8f),
                    Mathf.Sin(angle) * radius * Range(r, 0.6f, 1f));
                GameObject holder = Empty($"Livro {i + 1}", root.transform, pos, Range(r, 0f, 360f));
                var batch = new MeshBatch();
                Vector3 size = new Vector3(Range(r, 0.35f, 0.6f), Range(r, 0.06f, 0.12f), Range(r, 0.5f, 0.8f));
                GiantBook(batch, r, Vector3.zero, size, 0f);
                GameObject book = batch.Build("Livro", holder.transform);
                book.transform.localRotation = Quaternion.Euler(Range(r, -25f, 25f), 0f, Range(r, -35f, 35f));
                Animate(holder, Range(r, 8f, 22f) * (r.NextDouble() < 0.5 ? -1f : 1f), Range(r, 0.15f, 0.35f), Range(r, 0.4f, 0.9f));
            }
            return root;
        }

        /// <summary>Páginas soltas brilhando e girando no ar (cenário, sem colisão).</summary>
        public GameObject FloatingPages(string name, Vector3 center, float radius = 2.5f, int count = 10)
        {
            GameObject root = Empty("Paginas flutuantes - " + name, props, center);
            System.Random r = Seeded(name);
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = center + new Vector3(Range(r, -radius, radius), Range(r, -1f, 1.5f), Range(r, -radius, radius));
                GameObject holder = Empty($"Pagina {i + 1}", root.transform, pos, Range(r, 0f, 360f));
                var page = new GameObject("Folha", typeof(MeshFilter), typeof(MeshRenderer));
                page.transform.SetParent(holder.transform, false);
                page.transform.localRotation = Quaternion.Euler(Range(r, -60f, 60f), 0f, Range(r, -40f, 40f));
                page.transform.localScale = new Vector3(0.21f, 0.29f, 1f) * Range(r, 1f, 1.6f);
                page.GetComponent<MeshFilter>().sharedMesh = Cube;
                page.transform.localScale = new Vector3(page.transform.localScale.x, page.transform.localScale.y, 0.004f);
                var renderer = page.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = PageGlow;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                Animate(holder, Range(r, 15f, 45f), Range(r, 0.2f, 0.45f), Range(r, 0.5f, 1.2f));
            }
            return root;
        }

        void Animate(GameObject go, float degreesPerSecond, float bobHeight, float bobSpeed)
        {
            var spinner = go.AddComponent<Spinner>();
            var so = new SerializedObject(spinner);
            so.FindProperty("degreesPerSecond").floatValue = degreesPerSecond;
            so.FindProperty("bobHeight").floatValue = bobHeight;
            so.FindProperty("bobSpeed").floatValue = bobSpeed;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Relógio gigante flutuando, com ponteiros parados (cenário, sem colisão). Com yaw 0 o mostrador fica voltado para -Z, ou seja, para quem vem do início de uma fase que avança em +Z. Bom como marco no horizonte.</summary>
        public GameObject DreamClock(string name, Vector3 center, float radius = 1.6f, float yaw = 0f, float spin = 4f)
        {
            GameObject root = Empty("Relogio - " + name, props, center, yaw);
            var batch = new MeshBatch();
            batch.Add(Cylinder, Brass, Vector3.zero, Quaternion.Euler(90f, 0f, 0f), new Vector3(radius * 2.15f, 0.06f, radius * 2.15f));
            batch.Add(Cylinder, Paper, new Vector3(0f, 0f, -0.04f), Quaternion.Euler(90f, 0f, 0f), new Vector3(radius * 2f, 0.04f, radius * 2f));
            for (int h = 0; h < 12; h++)
            {
                float a = h * 30f * Mathf.Deg2Rad;
                batch.Box(DarkWood, new Vector3(Mathf.Sin(a) * radius * 0.82f, Mathf.Cos(a) * radius * 0.82f, -0.09f),
                    new Vector3(0.06f, h % 3 == 0 ? 0.3f : 0.16f, 0.02f), 0f, -h * 30f);
            }
            batch.Box(DarkWood, new Vector3(0.18f * radius, 0.2f * radius, -0.11f), new Vector3(0.07f, radius * 0.55f, 0.02f), 0f, -40f);
            batch.Box(DarkWood, new Vector3(-0.3f * radius, -0.05f * radius, -0.12f), new Vector3(0.05f, radius * 0.75f, 0.02f), 0f, 100f);
            batch.Build("Mostrador", root.transform).transform.localRotation = Quaternion.identity;
            Animate(root, spin, 0.2f, 0.4f);
            return root;
        }

        // ------------------------------------------------------------------ mobília

        /// <summary>Abajur de leitura de cúpula verde com luz quente (cenário, sem colisão). Pode ficar sobre plataformas.</summary>
        public GameObject ReadingLamp(Vector3 groundPos, float yaw = 0f, Color? lightColor = null)
        {
            GameObject root = Empty("Abajur de leitura", props, groundPos, yaw);
            var batch = new MeshBatch();
            batch.Add(Cylinder, Brass, new Vector3(0f, 0.03f, 0f), Quaternion.identity, new Vector3(0.35f, 0.03f, 0.35f));
            batch.Add(Cylinder, Brass, new Vector3(0f, 0.6f, 0f), Quaternion.identity, new Vector3(0.05f, 0.6f, 0.05f));
            batch.Add(Cylinder, Brass, new Vector3(0f, 1.2f, 0.12f), Quaternion.Euler(90f, 0f, 0f), new Vector3(0.04f, 0.13f, 0.04f));
            batch.Add(Sphere, LampShade, new Vector3(0f, 1.18f, 0.25f), Quaternion.identity, new Vector3(0.55f, 0.18f, 0.32f));
            batch.Build("Abajur", root.transform).transform.localRotation = Quaternion.identity;
            var light = new GameObject("Luz do abajur").AddComponent<Light>();
            light.transform.SetParent(root.transform, false);
            light.transform.localPosition = new Vector3(0f, 1.0f, 0.25f);
            light.type = LightType.Point;
            light.color = lightColor ?? new Color(1f, 0.85f, 0.55f);
            light.range = 6f;
            light.intensity = 2f;
            light.shadows = LightShadows.None;
            return root;
        }

        /// <summary>Escada de biblioteca encostada (cenário, sem colisão). <paramref name="height"/> é a altura que alcança.</summary>
        public GameObject LibraryLadder(Vector3 groundPos, float height = 3f, float yaw = 0f)
        {
            GameObject root = Empty("Escada de biblioteca", props, groundPos, yaw);
            var batch = new MeshBatch();
            const float lean = 14f;
            float length = height / Mathf.Cos(lean * Mathf.Deg2Rad);
            Quaternion tilt = Quaternion.Euler(-lean, 0f, 0f);
            Vector3 mid = tilt * new Vector3(0f, length / 2f, 0f);
            batch.Add(Cube, Wood, mid + new Vector3(-0.25f, 0f, 0f), tilt, new Vector3(0.06f, length, 0.06f));
            batch.Add(Cube, Wood, mid + new Vector3(0.25f, 0f, 0f), tilt, new Vector3(0.06f, length, 0.06f));
            for (float s = 0.3f; s < length - 0.1f; s += 0.32f)
                batch.Add(Cube, Brass, tilt * new Vector3(0f, s, 0f), tilt, new Vector3(0.5f, 0.04f, 0.05f));
            batch.Build("Escada", root.transform).transform.localRotation = Quaternion.identity;
            return root;
        }

        /// <summary>Tapete fino sobre uma superfície (cenário, sem colisão; fica 1 cm acima de <paramref name="topCenter"/>).</summary>
        public GameObject Carpet(Vector3 topCenter, Vector2 size, float yaw = 0f)
        {
            GameObject root = Empty("Tapete", props, topCenter + Vector3.up * 0.01f, yaw);
            var batch = new MeshBatch();
            batch.Box(Rug, Vector3.zero, new Vector3(size.x, 0.012f, size.y));
            batch.Box(Accent, new Vector3(0f, 0.002f, 0f), new Vector3(size.x - 0.3f, 0.012f, 0.05f));
            batch.Box(Brass, new Vector3(0f, 0.001f, size.y / 2f - 0.15f), new Vector3(size.x - 0.2f, 0.012f, 0.04f));
            batch.Box(Brass, new Vector3(0f, 0.001f, -size.y / 2f + 0.15f), new Vector3(size.x - 0.2f, 0.012f, 0.04f));
            GameObject visual = batch.Build("Tapete", root.transform);
            visual.transform.localRotation = Quaternion.identity;
            visual.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            return root;
        }

        /// <summary>Mesa de leitura com livro aberto e vela (sólida: colisão em caixa de 1,6 x 0,8 x 0,8 m).</summary>
        public GameObject ReadingDesk(string name, Vector3 groundPos, float yaw = 0f)
        {
            GameObject root = Empty("Mesa - " + name, geometry, groundPos, yaw);
            System.Random r = Seeded(name);
            var batch = new MeshBatch();
            batch.Box(Wood, new Vector3(0f, 0.76f, 0f), new Vector3(1.6f, 0.08f, 0.8f));
            foreach (float x in new[] { -0.7f, 0.7f })
                foreach (float z in new[] { -0.32f, 0.32f })
                    batch.Box(DarkWood, new Vector3(x, 0.36f, z), new Vector3(0.08f, 0.72f, 0.08f));
            batch.Box(Paper, new Vector3(-0.2f, 0.83f, 0f), new Vector3(0.5f, 0.04f, 0.36f), Range(r, -15f, 15f));
            batch.Box(SpineMaterial(r.Next(SpineColors.Length)), new Vector3(0.45f, 0.84f, 0.1f), new Vector3(0.3f, 0.08f, 0.22f), Range(r, -20f, 20f));
            batch.Add(Cylinder, Paper, new Vector3(0.55f, 0.92f, -0.22f), Quaternion.identity, new Vector3(0.05f, 0.08f, 0.05f));
            batch.Build("Mesa", root.transform).transform.localRotation = Quaternion.identity;
            var flame = new GameObject("Chama").AddComponent<Light>();
            flame.transform.SetParent(root.transform, false);
            flame.transform.localPosition = new Vector3(0.55f, 1.1f, -0.22f);
            flame.type = LightType.Point;
            flame.color = new Color(1f, 0.7f, 0.35f);
            flame.range = 3.5f;
            flame.intensity = 1.4f;
            flame.shadows = LightShadows.None;
            Box(root, new Vector3(0f, 0.4f, 0f), new Vector3(1.6f, 0.8f, 0.8f));
            return root;
        }

        /// <summary>Arco de estantes emoldurando uma passagem de <paramref name="width"/> m (sólido nas laterais; o vão é livre).</summary>
        public GameObject ShelfArch(string name, Vector3 groundCenter, float width = 2.4f, float height = 3.6f, float yaw = 0f)
        {
            Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
            GameObject root = Empty("Arco - " + name, geometry, groundCenter, yaw);
            float post = 1.2f;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 c = groundCenter + rot * new Vector3(side * (width / 2f + 0.3f), 0f, 0f);
                GameObject shelf = Bookshelf($"{name} lado {(side < 0 ? "E" : "D")}", c - rot * new Vector3(0f, 0f, post / 2f),
                    c + rot * new Vector3(0f, 0f, post / 2f), height);
                shelf.transform.SetParent(root.transform, true);
            }
            var batch = new MeshBatch();
            batch.Box(Wood, new Vector3(0f, height + 0.15f, 0f), new Vector3(width + 1.4f, 0.3f, post + 0.1f));
            batch.Box(Accent, new Vector3(0f, height + 0.02f, post / 2f + 0.06f), new Vector3(width + 1.2f, 0.06f, 0.03f));
            batch.Box(Accent, new Vector3(0f, height + 0.02f, -post / 2f - 0.06f), new Vector3(width + 1.2f, 0.06f, 0.03f));
            GameObject top = batch.Build("Verga", root.transform);
            top.transform.localPosition = Vector3.zero;
            top.transform.localRotation = Quaternion.identity;
            return root;
        }

        // ------------------------------------------------------------------ atmosfera

        /// <summary>Poeira luminosa de sonho: partículas lentas dentro de uma caixa (cenário).</summary>
        public GameObject DreamMotes(string name, Vector3 center, Vector3 size, Color? color = null, int maxParticles = 160)
        {
            GameObject go = Empty("Particulas de sonho - " + name, props, center);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.09f);
            Color c = color ?? Color.Lerp(Theme.Accent, Color.white, 0.5f);
            main.startColor = new ParticleSystem.MinMaxGradient(c, new Color(1f, 0.95f, 0.8f));
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.prewarm = true;
            var emission = ps.emission;
            emission.rateOverTime = maxParticles / 8f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = size;
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.25f;
            noise.frequency = 0.3f;
            var fade = ps.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = MoteMaterial();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return go;
        }

        Material MoteMaterial()
        {
            const string key = "Kit_ParticulaSonho";
            if (materials.TryGetValue(key, out Material cached))
                return cached;
            string path = $"{MaterialFolder}/{key}.mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                m = new Material(shader);
                m.SetFloat("_Surface", 1f); // transparente
                m.SetFloat("_Blend", 2f);   // aditivo
                m.SetOverrideTag("RenderType", "Transparent");
                m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)BlendMode.One);
                m.SetInt("_ZWrite", 0);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.EnableKeyword("_BLENDMODE_ADD");
                m.renderQueue = (int)RenderQueue.Transparent;
                m.SetTexture("_BaseMap", AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd"));
                AssetDatabase.CreateAsset(m, path);
            }
            materials[key] = m;
            return m;
        }

        /// <summary>
        /// Moldura de biblioteca para uma fase inteira: estantes altas de cenário ao redor do retângulo
        /// (<paramref name="min"/>..<paramref name="max"/> em X/Z), afastadas <paramref name="margin"/> m, mais partículas de sonho.
        /// Chame depois de montar a fase, com o retângulo que cobre todas as plataformas.
        /// </summary>
        public void LibraryBackdrop(Vector2 min, Vector2 max, float baseY = -4f, float height = 12f, float margin = 6f)
        {
            float x0 = min.x - margin, x1 = max.x + margin, z0 = min.y - margin, z1 = max.y + margin;
            BackdropShelf("Fundo oeste", new Vector3(x0, baseY, z0), new Vector3(x0, baseY, z1), height);
            BackdropShelf("Fundo leste", new Vector3(x1, baseY, z1), new Vector3(x1, baseY, z0), height);
            BackdropShelf("Fundo norte", new Vector3(x0, baseY, z1), new Vector3(x1, baseY, z1), height);
            BackdropShelf("Fundo sul", new Vector3(x1, baseY, z0), new Vector3(x0, baseY, z0), height);
            Vector3 center = new Vector3((x0 + x1) / 2f, baseY + height / 2f, (z0 + z1) / 2f);
            DreamMotes("Fase", center, new Vector3(x1 - x0, height, z1 - z0), null,
                Mathf.Clamp(Mathf.RoundToInt((x1 - x0) * (z1 - z0) / 6f), 120, 600));
        }
    }
}
