using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Iluminação de uma fase: luz principal com sombras suaves, ambiente em três tons, neblina colorida,
    /// pós-processamento (bloom, vinheta, saturação) e pontos de luz nos coletáveis. Serve para fases novas e existentes
    /// e pode rodar de novo sem duplicar nada.
    /// </summary>
    public static class PhaseLighting
    {
        const string VolumeFolder = "Assets/Settings/Fases";
        public const string SunName = "Luz principal";
        public const string VolumeName = "Pos-processamento";

        public static void Apply(Scene scene, PhaseTheme theme)
        {
            ApplyRenderSettings(theme);
            ApplySun(scene, theme);
            ApplyVolume(scene, theme);
            ApplyCameras(theme);
            ApplyGlowLights(scene, theme);
        }

        static void ApplyRenderSettings(PhaseTheme t)
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = t.Sky * 1.6f;
            RenderSettings.ambientEquatorColor = t.Equator;
            RenderSettings.ambientGroundColor = t.Ground;
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = t.Fog;
            RenderSettings.fogDensity = t.FogDensity;
            RenderSettings.reflectionIntensity = 0.6f;
        }

        static void ApplySun(Scene scene, PhaseTheme t)
        {
            Light sun = null;
            foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include))
            {
                if (l.gameObject.scene != scene || l.type != LightType.Directional)
                    continue;
                if (sun == null || l.name == SunName)
                    sun = l;
            }
            if (sun == null)
            {
                sun = new GameObject(SunName, typeof(Light)).GetComponent<Light>();
                sun.type = LightType.Directional;
                SceneManager.MoveGameObjectToScene(sun.gameObject, scene);
            }
            sun.gameObject.name = SunName;
            sun.color = t.Sun;
            sun.intensity = t.SunIntensity;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.85f;
            sun.shadowBias = 0.03f;
            sun.shadowNormalBias = 0.4f;
            sun.transform.rotation = Quaternion.Euler(t.SunEuler);
            RenderSettings.sun = sun;
        }

        static void ApplyVolume(Scene scene, PhaseTheme t)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                AssetDatabase.CreateFolder("Assets", "Settings");
            if (!AssetDatabase.IsValidFolder(VolumeFolder))
                AssetDatabase.CreateFolder("Assets/Settings", "Fases");

            string path = $"{VolumeFolder}/{t.Name}_Volume.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            profile.components.Clear();
            Bloom bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(t.Bloom);
            bloom.threshold.Override(0.9f);
            bloom.scatter.Override(0.65f);
            bloom.tint.Override(Color.Lerp(Color.white, t.Accent, 0.35f));

            Vignette vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(t.Vignette);
            vignette.smoothness.Override(0.5f);

            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.saturation.Override(t.Saturation);
            color.postExposure.Override(t.PostExposure);
            color.colorFilter.Override(t.Tint);
            color.contrast.Override(8f);

            Tonemapping tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);

            Volume volume = null;
            foreach (Volume v in Object.FindObjectsByType<Volume>(FindObjectsInactive.Include))
                if (v.gameObject.scene == scene && v.name == VolumeName)
                    volume = v;
            if (volume == null)
            {
                volume = new GameObject(VolumeName, typeof(Volume)).GetComponent<Volume>();
                SceneManager.MoveGameObjectToScene(volume.gameObject, scene);
            }
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.sharedProfile = profile;
        }

        static void ApplyCameras(PhaseTheme t)
        {
            foreach (Camera cam in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include))
            {
                if (cam.name.Contains("planta")) // câmera de revisão do layout
                    continue;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = t.Fog;
                UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = true;
                data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                data.antialiasingQuality = AntialiasingQuality.High;
            }
        }

        const string GlowRoot = "Luzes de destaque";

        // pontos de luz que guiam o olhar: coletáveis dourados, perigos avermelhados, porta de saída verde
        static void ApplyGlowLights(Scene scene, PhaseTheme t)
        {
            GameObject root = null;
            foreach (GameObject go in scene.GetRootGameObjects())
                if (go.name == GlowRoot)
                    root = go;
            if (root != null)
                Object.DestroyImmediate(root);

            root = new GameObject(GlowRoot);
            SceneManager.MoveGameObjectToScene(root, scene);

            foreach (Collider c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude))
            {
                if (c.gameObject.scene != scene)
                    continue;
                string n = c.name;
                if (n.Contains("[DS10]") || n.Contains("[DS11]") || n.Contains("Coletavel") || n.Contains("Pickup"))
                    AddGlow(root.transform, c, new Color(1f, 0.82f, 0.35f), 4.5f, 2.2f);
                else if (n.Contains("conclusao") || n.Contains("Conclusao") || n.Contains("Porta"))
                    AddGlow(root.transform, c, new Color(0.5f, 1f, 0.6f), 6f, 2.5f);
                else if (n.Contains("Armadilha") || n.Contains("Isca") || n.Contains("Chao falso"))
                    AddGlow(root.transform, c, new Color(1f, 0.4f, 0.3f), 4f, 1.4f);
            }
        }

        static void AddGlow(Transform parent, Collider source, Color color, float range, float intensity)
        {
            var go = new GameObject("Luz - " + source.name);
            go.transform.SetParent(parent, false);
            go.transform.position = source.bounds.center + Vector3.up * (source.bounds.extents.y + 0.8f);
            Light l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = range;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
        }
    }
}
