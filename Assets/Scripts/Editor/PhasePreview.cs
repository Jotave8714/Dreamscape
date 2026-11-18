using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Renderiza imagens das fases (visão do jogador na entrada e uma visão aérea) para conferir iluminação e materiais
    /// sem abrir o jogo. Batchmode (sem -nographics):
    /// Unity -batchmode -quit -projectPath . -executeMethod Dreamscape.EditorTools.PhasePreview.CaptureBatch -previewDir /pasta
    /// </summary>
    public static class PhasePreview
    {
        const int Width = 1280, Height = 720;

        [MenuItem("Dreamscape/Fases/Capturar previews")]
        public static void CaptureBatch()
        {
            string dir = Path.Combine(Path.GetTempPath(), "dreamscape-previews");
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-previewDir")
                    dir = args[i + 1];
            Directory.CreateDirectory(dir);

            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);
                if (!name.StartsWith("Fase") && !name.StartsWith("DS07") && !name.StartsWith("DS03"))
                    continue;
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                Capture(Path.Combine(dir, name + "_jogador.png"), PlayerCamera());
                Capture(Path.Combine(dir, name + "_aerea.png"), OverviewCamera());
            }
            Debug.Log("Dreamscape: previews em " + dir);
        }

        /// <summary>Captura uma cena (nome sem extensão) em Temp/previews: visão do jogador, visão alta atrás dele e aérea.</summary>
        public static void CaptureScene(string sceneName)
        {
            string dir = Path.GetFullPath("Temp/previews");
            Directory.CreateDirectory(dir);
            string[] guids = AssetDatabase.FindAssets(sceneName + " t:Scene", new[] { "Assets/Scenes" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) != sceneName)
                    continue;
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                Capture(Path.Combine(dir, sceneName + "_jogador.png"), PlayerCamera());
                Capture(Path.Combine(dir, sceneName + "_alta.png"), ChaseCamera());
                Capture(Path.Combine(dir, sceneName + "_aerea.png"), OverviewCamera());
                Debug.Log("Dreamscape: previews de " + sceneName + " em " + dir);
                return;
            }
            Debug.LogWarning("Dreamscape: cena não encontrada para preview: " + sceneName);
        }

        // atrás e acima do jogador, olhando para a frente da fase
        static Camera ChaseCamera()
        {
            Camera main = Camera.main;
            if (main == null)
                return null;
            Transform player = main.transform.root;
            var go = new GameObject("Preview camera", typeof(Camera));
            Camera cam = go.GetComponent<Camera>();
            cam.CopyFrom(main);
            cam.fieldOfView = 70f;
            cam.farClipPlane = 600f;
            cam.transform.position = player.position - player.forward * 5f + Vector3.up * 5f;
            cam.transform.LookAt(player.position + player.forward * 14f);
            return cam;
        }

        static Camera PlayerCamera() => Camera.main;

        static Camera OverviewCamera()
        {
            Bounds bounds = new Bounds();
            bool first = true;
            foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude))
            {
                if (first) { bounds = r.bounds; first = false; }
                else bounds.Encapsulate(r.bounds);
            }
            var go = new GameObject("Preview camera", typeof(Camera));
            Camera cam = go.GetComponent<Camera>();
            cam.fieldOfView = 50f;
            cam.farClipPlane = 600f;
            Vector3 center = bounds.center;
            float length = Mathf.Max(bounds.size.x, bounds.size.z);
            cam.transform.position = center + new Vector3(-length * 0.55f, length * 0.55f + 8f, -length * 0.55f);
            cam.transform.LookAt(center);
            return cam;
        }

        static void Capture(string file, Camera cam)
        {
            if (cam == null)
                return;
            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            RenderTexture previous = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            tex.Apply();
            File.WriteAllBytes(file, tex.EncodeToPNG());
            cam.targetTexture = previous;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            if (cam.name == "Preview camera")
                Object.DestroyImmediate(cam.gameObject);
        }
    }
}
