using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>
    /// Direção de arte de uma fase. O GDD pede uma paleta que começa escura e fica mais colorida a cada fase:
    /// <see cref="Saturation"/> e <see cref="Exposure"/> crescem de uma fase para a seguinte.
    /// </summary>
    public sealed class PhaseTheme
    {
        public string Name;
        public Color Floor;      // pisos
        public Color Wall;       // paredes e pilares
        public Color Accent;     // frisos e detalhes
        public Color Sky;        // fundo da câmera e ambiente superior
        public Color Equator;    // ambiente lateral
        public Color Ground;     // ambiente inferior
        public Color Fog;
        public float FogDensity = 0.02f;
        public Color Sun;
        public float SunIntensity = 1.1f;
        public Vector3 SunEuler = new Vector3(50f, -30f, 0f);
        public float Bloom = 0.6f;
        public float Vignette = 0.3f;
        public float Saturation;     // -100..100 no Color Adjustments
        public float PostExposure;   // em EV
        public Color Tint = Color.white;

        static Color C(float r, float g, float b) => new Color(r, g, b, 1f);

        // fases existentes: mais escuras, seguindo o começo do GDD
        public static readonly PhaseTheme Fase1 = new PhaseTheme
        {
            Name = "Fase1", Floor = C(0.22f, 0.51f, 0.57f), Wall = C(0.22f, 0.23f, 0.31f), Accent = C(0.05f, 0.95f, 1f),
            Sky = C(0.06f, 0.07f, 0.12f), Equator = C(0.10f, 0.12f, 0.18f), Ground = C(0.03f, 0.03f, 0.05f),
            Fog = C(0.05f, 0.06f, 0.10f), FogDensity = 0.035f, Sun = C(0.62f, 0.72f, 1f), SunIntensity = 0.9f,
            SunEuler = new Vector3(48f, -25f, 0f), Bloom = 0.5f, Vignette = 0.38f, Saturation = -22f, PostExposure = -0.2f,
            Tint = C(0.9f, 0.95f, 1f)
        };

        public static readonly PhaseTheme Fase2 = new PhaseTheme
        {
            Name = "Fase2", Floor = C(0.30f, 0.50f, 0.65f), Wall = C(0.24f, 0.22f, 0.34f), Accent = C(0.70f, 0.50f, 0.85f),
            Sky = C(0.09f, 0.08f, 0.16f), Equator = C(0.14f, 0.12f, 0.22f), Ground = C(0.04f, 0.03f, 0.06f),
            Fog = C(0.08f, 0.07f, 0.14f), FogDensity = 0.032f, Sun = C(0.78f, 0.7f, 1f), SunIntensity = 0.95f,
            SunEuler = new Vector3(52f, -35f, 0f), Bloom = 0.55f, Vignette = 0.35f, Saturation = -14f, PostExposure = -0.1f,
            Tint = C(0.97f, 0.93f, 1f)
        };

        public static readonly PhaseTheme Fase3 = new PhaseTheme
        {
            Name = "Fase3", Floor = C(0.25f, 0.55f, 0.52f), Wall = C(0.20f, 0.26f, 0.30f), Accent = C(0.35f, 0.95f, 0.85f),
            Sky = C(0.08f, 0.13f, 0.15f), Equator = C(0.13f, 0.20f, 0.22f), Ground = C(0.04f, 0.06f, 0.07f),
            Fog = C(0.07f, 0.12f, 0.14f), FogDensity = 0.028f, Sun = C(0.8f, 0.95f, 0.95f), SunIntensity = 1.0f,
            SunEuler = new Vector3(55f, -40f, 0f), Bloom = 0.6f, Vignette = 0.33f, Saturation = -6f, PostExposure = 0f,
            Tint = C(0.95f, 1f, 1f)
        };

        public static readonly PhaseTheme Fase4 = new PhaseTheme
        {
            Name = "Fase4", Floor = C(0.52f, 0.52f, 0.30f), Wall = C(0.30f, 0.27f, 0.20f), Accent = C(1f, 0.82f, 0.30f),
            Sky = C(0.14f, 0.12f, 0.08f), Equator = C(0.24f, 0.20f, 0.12f), Ground = C(0.06f, 0.05f, 0.03f),
            Fog = C(0.14f, 0.11f, 0.07f), FogDensity = 0.026f, Sun = C(1f, 0.88f, 0.65f), SunIntensity = 1.15f,
            SunEuler = new Vector3(45f, 30f, 0f), Bloom = 0.65f, Vignette = 0.3f, Saturation = 2f, PostExposure = 0.05f,
            Tint = C(1f, 0.97f, 0.9f)
        };

        public static readonly PhaseTheme Fase5 = new PhaseTheme
        {
            Name = "Fase5", Floor = C(0.55f, 0.35f, 0.62f), Wall = C(0.26f, 0.20f, 0.34f), Accent = C(1f, 0.45f, 0.75f),
            Sky = C(0.14f, 0.09f, 0.18f), Equator = C(0.24f, 0.15f, 0.30f), Ground = C(0.06f, 0.04f, 0.08f),
            Fog = C(0.14f, 0.08f, 0.17f), FogDensity = 0.024f, Sun = C(1f, 0.8f, 0.95f), SunIntensity = 1.2f,
            SunEuler = new Vector3(50f, 150f, 0f), Bloom = 0.8f, Vignette = 0.28f, Saturation = 10f, PostExposure = 0.1f,
            Tint = C(1f, 0.95f, 1f)
        };

        public static readonly PhaseTheme Fase6 = new PhaseTheme
        {
            Name = "Fase6", Floor = C(0.78f, 0.45f, 0.30f), Wall = C(0.34f, 0.24f, 0.22f), Accent = C(1f, 0.6f, 0.3f),
            Sky = C(0.20f, 0.12f, 0.10f), Equator = C(0.32f, 0.20f, 0.16f), Ground = C(0.08f, 0.05f, 0.04f),
            Fog = C(0.20f, 0.11f, 0.08f), FogDensity = 0.02f, Sun = C(1f, 0.72f, 0.5f), SunIntensity = 1.3f,
            SunEuler = new Vector3(38f, 200f, 0f), Bloom = 0.9f, Vignette = 0.25f, Saturation = 18f, PostExposure = 0.15f,
            Tint = C(1f, 0.96f, 0.92f)
        };

        public static readonly PhaseTheme Fase7 = new PhaseTheme
        {
            Name = "Fase7", Floor = C(0.45f, 0.78f, 0.50f), Wall = C(0.35f, 0.45f, 0.65f), Accent = C(1f, 0.95f, 0.55f),
            Sky = C(0.35f, 0.50f, 0.75f), Equator = C(0.45f, 0.55f, 0.65f), Ground = C(0.15f, 0.20f, 0.15f),
            Fog = C(0.55f, 0.68f, 0.85f), FogDensity = 0.012f, Sun = C(1f, 0.96f, 0.85f), SunIntensity = 1.5f,
            SunEuler = new Vector3(58f, -20f, 0f), Bloom = 1.0f, Vignette = 0.18f, Saturation = 28f, PostExposure = 0.25f,
            Tint = Color.white
        };
    }
}
