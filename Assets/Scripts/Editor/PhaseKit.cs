using System;
using System.Collections.Generic;
using Dreamscape.Hazards;
using Dreamscape.PowerUps;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dreamscape.EditorTools
{
    public enum PickupKind { DoubleJump, Speed, Gravity, Shield }

    /// <summary>Uma fase gerada por código. Implemente e coloque na pasta Editor: o menu Dreamscape encontra sozinho.</summary>
    public interface IPhaseBuilder
    {
        /// <summary>Ordem da fase no jogo (3 = terceira fase).</summary>
        int Order { get; }
        string SceneName { get; }
        PhaseTheme Theme { get; }
        void Build(PhaseKit kit);
    }

    /// <summary>
    /// Peças para montar fases no estilo das existentes. Todas as medidas em metros, Y para cima.
    /// Os modelos vêm de Assets/Art/Environment (feitos no Blender) e os materiais seguem o tema da fase.
    ///
    /// Limites do jogador (CharacterMotor): pulo 1,1 m; correr 5 m/s (7,5 com Velocidade);
    /// alcance horizontal de um pulo correndo ~3,3 m (~5 m com Velocidade); pulo duplo soma ~1,1 m de altura.
    /// Projete com folga: vão máximo seguro 2,8 m e degrau máximo 0,9 m sem poder.
    /// </summary>
    public sealed class PhaseKit
    {
        const string EnvFolder = "Assets/Art/Environment/";
        const string PlayerPrefab = "Assets/Prefabs/FirstPersonPlayer.prefab";
        const string MaterialFolder = "Assets/Design/Fases";

        public Scene Scene { get; }
        public PhaseTheme Theme { get; }
        public string ScenePath { get; }

        readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        readonly Transform geometry, hazards, pickups, props;
        bool hasPlayer, hasGoal;

        PhaseKit(string scenePath, PhaseTheme theme)
        {
            ScenePath = scenePath;
            Theme = theme;
            Scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            geometry = Group("Geometria");
            hazards = Group("Armadilhas");
            pickups = Group("Power-ups");
            props = Group("Cenario");
        }

        public static PhaseKit Begin(string sceneName, PhaseTheme theme) =>
            new PhaseKit($"Assets/Scenes/{sceneName}.unity", theme);

        static Transform Group(string name) => new GameObject(name).transform;

        // ------------------------------------------------------------------ materiais

        Material ThemeMaterial(string role, Color color, bool glow)
        {
            string key = $"{Theme.Name}_{role}";
            if (materials.TryGetValue(key, out Material cached))
                return cached;
            // o friso brilha pouco: ele é fino e o bloom das fases claras o estouraria
            Material m = LoadOrCreateMaterial($"{MaterialFolder}/{key}.mat", color, glow ? color : (Color?)null, 0.7f);
            materials[key] = m;
            return m;
        }

        Material KitMaterial(string role, Color color, bool glow)
        {
            string key = "Kit_" + role;
            if (materials.TryGetValue(key, out Material cached))
                return cached;
            Material m = LoadOrCreateMaterial($"{MaterialFolder}/{key}.mat", color, glow ? color : (Color?)null);
            materials[key] = m;
            return m;
        }

        static Material LoadOrCreateMaterial(string path, Color color, Color? emission, float emissionScale = 1.4f)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Design"))
                AssetDatabase.CreateFolder("Assets", "Design");
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
                AssetDatabase.CreateFolder("Assets/Design", "Fases");

            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", 0.18f);
            if (emission.HasValue)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emission.Value * emissionScale);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            EditorUtility.SetDirty(m);
            return m;
        }

        // nomes dos materiais no Blender -> material URP (cores do tema onde faz sentido)
        Material Resolve(string blenderName)
        {
            switch (blenderName)
            {
                case "Seguro": return ThemeMaterial("Piso", Theme.Floor, false);
                case "Parede": return ThemeMaterial("Parede", Theme.Wall, false);
                case "PistaAzul": return ThemeMaterial("Friso", Theme.Accent, true);
                case "Chegada": return KitMaterial("Chegada", new Color(0.43f, 0.72f, 0.42f), true);
                case "Armadilha": return KitMaterial("Armadilha", new Color(0.85f, 0.35f, 0.25f), false);
                case "Poder": return KitMaterial("Poder", new Color(0.95f, 0.74f, 0.22f), true);
                case "Velocidade": return KitMaterial("Velocidade", new Color(1f, 0.8f, 0.2f), true);
                case "Temporario": return KitMaterial("Temporario", new Color(0.7f, 0.5f, 0.85f), false);
                case "Metal": return KitMaterial("Metal", new Color(0.35f, 0.37f, 0.42f), false);
                case "MetalEscuro": return KitMaterial("MetalEscuro", new Color(0.13f, 0.14f, 0.18f), false);
                case "Pufe": return KitMaterial("Pufe", new Color(0.55f, 0.25f, 0.6f), false);
                case "Gravidade": return KitMaterial("Gravidade", new Color(0.55f, 0.85f, 1f), true);
                case "Escudo": return KitMaterial("Escudo", new Color(0.4f, 0.9f, 0.7f), true);
                case "Brilho": return KitMaterial("Brilho", new Color(1f, 0.95f, 0.75f), true);
                default: return null;
            }
        }

        void Skin(GameObject root)
        {
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
            {
                Material[] current = r.sharedMaterials;
                for (int i = 0; i < current.Length; i++)
                {
                    string n = current[i] != null ? current[i].name.Replace(" (Instance)", "") : "";
                    Material replacement = Resolve(n);
                    if (replacement != null)
                        current[i] = replacement;
                }
                r.sharedMaterials = current;
            }
        }

        // ------------------------------------------------------------------ instanciar

        GameObject Model(string file, Transform parent, string name, Vector3 position, float yaw = 0f)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnvFolder + file + ".fbx");
            if (prefab == null)
                throw new InvalidOperationException($"Modelo não encontrado: {EnvFolder}{file}.fbx");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, Scene);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            foreach (Collider c in go.GetComponentsInChildren<Collider>())
                UnityEngine.Object.DestroyImmediate(c); // colisão é desenhada à parte, simples e previsível
            Skin(go);
            return go;
        }

        static GameObject Empty(string name, Transform parent, Vector3 position, float yaw = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            return go;
        }

        static BoxCollider Box(GameObject go, Vector3 localCenter, Vector3 size, bool trigger = false)
        {
            var b = go.AddComponent<BoxCollider>();
            b.center = localCenter;
            b.size = size;
            b.isTrigger = trigger;
            return b;
        }

        // ------------------------------------------------------------------ geometria

        /// <summary>
        /// Plataforma de 0,5 m de espessura. <paramref name="topCenter"/> é o centro da face de cima; <paramref name="size"/> é X por Z.
        /// </summary>
        public GameObject Platform(string name, Vector3 topCenter, Vector2 size, float yaw = 0f)
        {
            GameObject root = Empty(name, geometry, topCenter, yaw);
            Tile(root, "Plataforma_2x2", size, 0.5f, 1f, 0.5f);
            Box(root, new Vector3(0f, -0.25f, 0f), new Vector3(size.x, 0.5f, size.y));
            return root;
        }

        // cobre `size` com módulos de 2 x 2 m esticados para fechar sem sobra
        void Tile(GameObject root, string model, Vector2 size, float modelHeight, float scaleY, float unused)
        {
            int cx = Mathf.Max(1, Mathf.RoundToInt(size.x / 2f));
            int cz = Mathf.Max(1, Mathf.RoundToInt(size.y / 2f));
            float w = size.x / cx, d = size.y / cz;
            for (int ix = 0; ix < cx; ix++)
                for (int iz = 0; iz < cz; iz++)
                {
                    Vector3 local = new Vector3(-size.x / 2f + w * (ix + 0.5f), -modelHeight * scaleY, -size.y / 2f + d * (iz + 0.5f));
                    GameObject tile = Model(model, root.transform, $"{model} {ix}.{iz}", Vector3.zero);
                    tile.transform.localPosition = local;
                    tile.transform.localRotation = Quaternion.identity;
                    tile.transform.localScale = new Vector3(w / 2f, scaleY, d / 2f);
                }
        }

        /// <summary>Parede de <paramref name="from"/> até <paramref name="to"/> (altura Y de from é a base).</summary>
        public GameObject Wall(string name, Vector3 from, Vector3 to, float height = 3f)
        {
            Vector3 flat = new Vector3(to.x - from.x, 0f, to.z - from.z);
            float length = flat.magnitude;
            float yaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg - 90f; // o módulo corre ao longo do X local
            GameObject root = Empty(name, geometry, (from + new Vector3(to.x, from.y, to.z)) / 2f, yaw);
            int cells = Mathf.Max(1, Mathf.RoundToInt(length / 2f));
            float w = length / cells;
            for (int i = 0; i < cells; i++)
            {
                GameObject m = Model("Parede_Modulo", root.transform, $"Modulo {i}", Vector3.zero);
                m.transform.localPosition = new Vector3(-length / 2f + w * (i + 0.5f), 0f, 0f);
                m.transform.localRotation = Quaternion.identity;
                m.transform.localScale = new Vector3(w / 2f, height / 3f, 1f);
            }
            Box(root, new Vector3(0f, height / 2f, 0f), new Vector3(length, height, 0.3f));
            return root;
        }

        public GameObject Pillar(string name, Vector3 baseCenter, float height = 3f)
        {
            GameObject root = Model("Pilar", geometry, name, baseCenter);
            root.transform.localScale = new Vector3(1f, height / 3f, 1f);
            var c = root.AddComponent<BoxCollider>();
            c.center = new Vector3(0f, 1.5f, 0f);
            c.size = new Vector3(0.8f, 3f, 0.8f);
            return root;
        }

        /// <summary>Rampa de 2 m de comprimento e 0,5 m de subida, subindo para +Z local (gire com yaw).</summary>
        public GameObject Ramp(string name, Vector3 baseCenter, float yaw = 0f)
        {
            GameObject root = Model("Rampa", geometry, name, baseCenter, yaw);
            var c = root.AddComponent<BoxCollider>();
            float angle = Mathf.Atan2(0.5f, 2f) * Mathf.Rad2Deg;
            c.center = new Vector3(0f, 0.25f, 0f);
            c.size = new Vector3(2f, 0.2f, Mathf.Sqrt(4f + 0.25f));
            // a caixa fica inclinada pelo transform de um filho para não girar o modelo
            UnityEngine.Object.DestroyImmediate(c);
            GameObject slab = Empty("Colisor", root.transform, baseCenter + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 0.25f, 0f), yaw);
            slab.transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            Box(slab, Vector3.zero, new Vector3(2f, 0.2f, Mathf.Sqrt(4f + 0.25f)));
            return root;
        }

        // ------------------------------------------------------------------ power-ups

        public GameObject Pickup(PickupKind kind, Vector3 groundPos)
        {
            string model, label;
            switch (kind)
            {
                case PickupKind.DoubleJump: model = "Pickup_PuloDuplo"; label = "Pulo duplo"; break;
                case PickupKind.Speed: model = "Pickup_Velocidade"; label = "Velocidade"; break;
                case PickupKind.Gravity: model = "Pickup_Gravidade"; label = "Pluma"; break;
                default: model = "Pickup_Escudo"; label = "Escudo"; break;
            }
            GameObject go = Model(model, pickups, $"Pickup {label}", groundPos);
            var sphere = go.AddComponent<SphereCollider>();
            sphere.center = new Vector3(0f, 0.7f, 0f);
            sphere.radius = 0.6f;
            sphere.isTrigger = true;
            switch (kind)
            {
                case PickupKind.DoubleJump: go.AddComponent<DoubleJumpPickup>(); break;
                case PickupKind.Speed: go.AddComponent<SpeedPickup>(); break;
                case PickupKind.Gravity: go.AddComponent<GravityPickup>(); break;
                default: go.AddComponent<ShieldPickup>(); break;
            }
            go.AddComponent<Spinner>();
            return go;
        }

        // ------------------------------------------------------------------ armadilhas

        /// <summary>Campo de espinhos de 2 x 2 m sobre um piso. Com <paramref name="cycle"/> sobem e descem em ritmo fixo.</summary>
        public GameObject Spikes(string name, Vector3 topCenter, bool cycle = false, float timeOut = 1.2f, float timeIn = 1.5f, float offset = 0f)
        {
            GameObject root = Empty("Armadilha - " + name, hazards, topCenter);
            GameObject visual = Model("Trap_Espinhos", root.transform, "Espinhos", topCenter);
            Box(root, new Vector3(0f, 0.35f, 0f), new Vector3(1.8f, 0.6f, 1.8f), true);
            var trap = root.AddComponent<SpikeTrap>();
            var so = new SerializedObject(trap);
            so.FindProperty("spikes").objectReferenceValue = visual.transform;
            so.FindProperty("cycle").boolValue = cycle;
            so.FindProperty("timeOut").floatValue = timeOut;
            so.FindProperty("timeIn").floatValue = timeIn;
            so.FindProperty("offset").floatValue = offset;
            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        /// <summary>
        /// Serra que desliza sobre o piso por <paramref name="travel"/> (deslocamento mundial) a partir de <paramref name="groundPos"/>.
        /// O disco fica a 0,85 m do piso e gira em torno do próprio centro.
        /// </summary>
        public GameObject Saw(string name, Vector3 groundPos, Vector3 travel, float period = 3f, float yaw = 0f)
        {
            GameObject root = Model("Trap_Serra", hazards, "Armadilha - " + name, groundPos + Vector3.up * 0.85f, yaw);
            var sphere = root.AddComponent<SphereCollider>();
            sphere.radius = 0.78f;
            sphere.isTrigger = true;
            var trap = root.AddComponent<SawTrap>();
            var so = new SerializedObject(trap);
            so.FindProperty("travel").vector3Value = travel; // o pai não gira: o deslocamento é o mundial
            so.FindProperty("period").floatValue = period;
            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        /// <summary>Pêndulo cujo pivô fica em <paramref name="floorPos"/> + 3,8 m. <paramref name="yaw"/> define o plano do balanço (0 = balança em X).</summary>
        public GameObject Pendulum(string name, Vector3 floorPos, float angle = 55f, float period = 2.4f, float offset = 0f, float yaw = 0f)
        {
            GameObject pivot = Empty("Armadilha - " + name, hazards, floorPos + Vector3.up * 3.8f, yaw);
            GameObject visual = Model("Trap_Pendulo", pivot.transform, "Pendulo", floorPos, yaw);
            visual.transform.position = floorPos;
            var sphere = pivot.AddComponent<SphereCollider>();
            sphere.center = new Vector3(0f, -2.65f, 0f);
            sphere.radius = 0.62f;
            sphere.isTrigger = true;
            var trap = pivot.AddComponent<PendulumTrap>();
            var so = new SerializedObject(trap);
            so.FindProperty("angle").floatValue = angle;
            so.FindProperty("period").floatValue = period;
            so.FindProperty("offset").floatValue = offset;
            so.ApplyModifiedPropertiesWithoutUndo();
            return pivot;
        }

        /// <summary>Laser entre dois postes a 3 m um do outro, ao longo do X local (gire com yaw).</summary>
        public GameObject Laser(string name, Vector3 groundCenter, float yaw = 0f, float timeOn = 1.5f, float timeOff = 1.8f, float offset = 0f)
        {
            GameObject root = Model("Trap_Laser", hazards, "Armadilha - " + name, groundCenter, yaw);
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "Feixe";
            UnityEngine.Object.DestroyImmediate(beam.GetComponent<Collider>());
            beam.transform.SetParent(root.transform, false);
            beam.transform.localPosition = new Vector3(0f, 1f, 0f);
            beam.transform.localScale = new Vector3(2.7f, 0.06f, 0.06f);
            beam.GetComponent<Renderer>().sharedMaterial = KitMaterial("Laser", new Color(1f, 0.25f, 0.2f), true);
            Box(root, new Vector3(0f, 1f, 0f), new Vector3(2.7f, 0.35f, 0.25f), true);
            var trap = root.AddComponent<LaserTrap>();
            var so = new SerializedObject(trap);
            so.FindProperty("beam").objectReferenceValue = beam.GetComponent<Renderer>();
            so.FindProperty("timeOn").floatValue = timeOn;
            so.FindProperty("timeOff").floatValue = timeOff;
            so.FindProperty("offset").floatValue = offset;
            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        /// <summary>Prensa que desce sobre um corredor de 1,6 m de largura; passa-se por baixo quando está erguida.</summary>
        public GameObject Press(string name, Vector3 groundPos, float timeUp = 2f, float timeDown = 0.8f, float offset = 0f, float yaw = 0f)
        {
            GameObject root = Model("Trap_Prensa", hazards, "Armadilha - " + name, groundPos, yaw);
            Box(root, new Vector3(0f, 2.4f, 0f), new Vector3(1.6f, 0.9f, 1.6f), true);
            var trap = root.AddComponent<PressTrap>();
            var so = new SerializedObject(trap);
            so.FindProperty("drop").floatValue = 1.9f;
            so.FindProperty("timeUp").floatValue = timeUp;
            so.FindProperty("timeDown").floatValue = timeDown;
            so.FindProperty("offset").floatValue = offset;
            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        /// <summary>Plataforma que cede 0,8 s depois de pisada (a fase recarregada a restaura).</summary>
        public GameObject CrumblingPlatform(string name, Vector3 topCenter, Vector2 size, float delay = 0.8f)
        {
            GameObject root = Empty("Armadilha - " + name, hazards, topCenter);
            Tile(root, "Trap_PisoFalso", size, 0.4f, 1.25f, 0f);
            Box(root, new Vector3(0f, -0.25f, 0f), new Vector3(size.x, 0.5f, size.y));
            var floor = root.AddComponent<CrumblingFloor>();
            var so = new SerializedObject(floor);
            so.FindProperty("delay").floatValue = delay;
            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        // ------------------------------------------------------------------ marcos da fase

        /// <summary>Instancia o jogador (prefab em primeira pessoa) no ponto, olhando para <paramref name="yaw"/>, com o pufe do sono atrás.</summary>
        public GameObject Spawn(Vector3 groundPos, float yaw = 0f)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefab);
            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, Scene);
            player.name = "Player";
            player.transform.SetPositionAndRotation(groundPos + Vector3.up * 0.1f, Quaternion.Euler(0f, yaw, 0f));
            Model("Checkpoint_Pufe", props, "Checkpoint - pufe", groundPos + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 0f, -1.4f));
            hasPlayer = true;
            return player;
        }

        /// <summary>Porta de conclusão; ao tocá-la o jogo segue para a próxima fase do build.</summary>
        public GameObject Goal(Vector3 groundPos, float yaw = 0f, string nextScene = null)
        {
            GameObject go = Model("Porta_Conclusao", props, "Porta de conclusao", groundPos, yaw);
            Box(go, new Vector3(0f, 1.5f, 0f), new Vector3(1.6f, 3f, 1.2f), true);
            var goal = go.AddComponent<PhaseGoal>();
            if (!string.IsNullOrEmpty(nextScene))
            {
                var so = new SerializedObject(goal);
                so.FindProperty("nextScene").stringValue = nextScene;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            hasGoal = true;
            return go;
        }

        /// <summary>Volume abaixo da fase que reinicia a tentativa quando o jogador cai.</summary>
        public GameObject FallZone(float y, float halfExtent = 150f)
        {
            GameObject go = Empty("Zona de queda", props, new Vector3(0f, y, 0f));
            var b = go.AddComponent<BoxCollider>();
            b.size = new Vector3(halfExtent * 2f, 2f, halfExtent * 2f);
            b.isTrigger = true;
            go.AddComponent<PhaseFallZone>();
            return go;
        }

        public GameObject Pole(Vector3 groundPos, Color? lightColor = null)
        {
            GameObject go = Model("Poste_Luz", props, "Poste de luz", groundPos);
            var l = new GameObject("Luz do poste").AddComponent<Light>();
            l.transform.SetParent(go.transform, false);
            l.transform.localPosition = new Vector3(0f, 2.4f, 0f);
            l.type = LightType.Point;
            l.color = lightColor ?? new Color(1f, 0.92f, 0.7f);
            l.range = 8f;
            l.intensity = 2.2f;
            l.shadows = LightShadows.None;
            return go;
        }

        public GameObject CeilingLight(Vector3 ceilingPos, float yaw = 0f, Color? lightColor = null)
        {
            GameObject go = Model("Luminaria_Teto", props, "Luminaria de teto", ceilingPos - Vector3.up * 0.2f, yaw);
            var l = new GameObject("Luz da luminaria").AddComponent<Light>();
            l.transform.SetParent(go.transform, false);
            l.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            l.type = LightType.Point;
            l.color = lightColor ?? Theme.Accent;
            l.range = 7f;
            l.intensity = 1.8f;
            l.shadows = LightShadows.None;
            return go;
        }

        /// <summary>Texto 3D no cenário (nome da fase, dicas).</summary>
        public GameObject Sign(string text, Vector3 pos, float yaw = 0f, float size = 0.12f)
        {
            GameObject go = Empty("Letreiro - " + text, props, pos, yaw);
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.characterSize = size * 0.45f; // as fases pedem tamanhos pensados para uma escala maior
            tm.fontSize = 64;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.Lerp(Color.white, Theme.Accent, 0.4f);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tm.font = font;
            go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return go;
        }

        // ------------------------------------------------------------------ encerrar

        /// <summary>Aplica a iluminação do tema, grava a cena e a inclui no build (depois das que já existem).</summary>
        public void Finish()
        {
            if (!hasPlayer)
                Debug.LogWarning($"Dreamscape: a fase {Scene.name} não chamou Spawn.");
            if (!hasGoal)
                Debug.LogWarning($"Dreamscape: a fase {Scene.name} não chamou Goal.");

            PhaseLighting.Apply(Scene, Theme);
            EditorSceneManager.SaveScene(Scene, ScenePath);
            AddToBuild(ScenePath);
            AssetDatabase.SaveAssets();
        }

        public static void AddToBuild(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == path))
                return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
