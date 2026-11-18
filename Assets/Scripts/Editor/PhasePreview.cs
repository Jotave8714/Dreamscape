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
