using UnityEditor;
using UnityEngine;

namespace Dreamscape.EditorTools
{
    /// <summary>Epílogo da biblioteca: siga as margens azuis, não a página que promete um atalho.</summary>
    public sealed class Fase18Builder : IPhaseBuilder
    {
        public const string Name = "Fase18_JardimDasEntrelinhas";
        public int Order => 18;
        public string SceneName => Name;
        public PhaseTheme Theme => Garden;

        static readonly Color Blue = new Color(0.32f, 0.86f, 1f);
        static readonly PhaseTheme Garden = new PhaseTheme
        {
            Name = "Fase18", Floor = new Color(0.36f, 0.63f, 0.53f),
            Wall = new Color(0.19f, 0.29f, 0.31f), Accent = Blue,
            Sky = new Color(0.26f, 0.42f, 0.52f), Equator = new Color(0.30f, 0.37f, 0.38f),
            Ground = new Color(0.09f, 0.16f, 0.17f), Fog = new Color(0.31f, 0.49f, 0.54f),
            FogDensity = 0.007f, Sun = new Color(1f, 0.86f, 0.64f), SunIntensity = 1.25f,
            SunEuler = new Vector3(32f, -28f, 0f), Bloom = 0.45f,
            Vignette = 0.18f, Saturation = 12f, PostExposure = 0.1f
        };

        [MenuItem("Dreamscape/Fases/Gerar Fase 18 - Jardim das Entrelinhas")]
        public static void Generate() => PhaseBuildMenu.BuildPhase(Name);

        public static void Preview() => PhasePreview.CaptureScene(Name);

        public void Build(PhaseKit kit)
        {
            kit.OpenBookPlatform("E18 Entrada", Vector3.zero, new Vector2(10f, 8f));
            kit.Spawn(new Vector3(0f, 0f, -1f));
            kit.Carpet(new Vector3(0f, 0f, 0f), new Vector2(3f, 5f));
            kit.ShelfArch("Arco do jardim", new Vector3(0f, 0f, -3.3f), 5f, 4f);
            kit.Sign("FASE 18 | JARDIM DAS ENTRELINHAS", new Vector3(0f, 4.3f, 4f), 0f, 0.11f);
            kit.Sign("NEM TODA LINHA RETA LEVA AO FIM", new Vector3(0f, 3.4f, 4f), 0f, 0.065f);

            // O falso atalho cede antes de se cruzarem seus 10 m. O desvio é chão contínuo e azul.
            kit.CrumblingPlatform("E18 Atalho de papel", new Vector3(0f, 0f, 9f), new Vector2(3f, 10f), 0.7f);
            kit.Sign("ATALHO", new Vector3(0f, 1.8f, 6.2f), 0f, 0.09f);
            kit.BookPlatform("E18 Margem segura", new Vector3(-5f, 0f, 10f), new Vector2(3f, 16f));
            kit.Carpet(new Vector3(-5f, 0f, 10f), new Vector2(2f, 15f));
            kit.Sign("SIGA AS MARGENS AZUIS", new Vector3(-4.8f, 2.8f, 3f), 0f, 0.06f);
            foreach (float z in new[] { 4f, 10f, 16f })
                kit.Pole(new Vector3(-7.2f, 0f, z), Blue);

            kit.OpenBookPlatform("E18 Clareira do pulo", new Vector3(0f, 0f, 19f), new Vector2(13f, 6f));
            kit.Pickup(PickupKind.DoubleJump, new Vector3(0f, 0f, 19f));
            kit.Sign("ESPACO + ESPACO: SUBA OS LIVROS", new Vector3(0f, 3.5f, 22f), 0f, 0.065f);
            kit.Sign("O PODER DURA 30 S | SE ACABAR, ELE VOLTA", new Vector3(0f, 2.9f, 22f), 0f, 0.04f);
            kit.BookPlatform("E18 Degrau 1", new Vector3(0f, 1.7f, 26f), new Vector2(5f, 4f));
            kit.BookPlatform("E18 Degrau 2", new Vector3(0f, 3.4f, 32f), new Vector2(5f, 4f));
            kit.OpenBookPlatform("E18 Mirante", new Vector3(0f, 3.4f, 39f), new Vector2(8f, 6f));
            foreach (Vector3 point in new[] { new Vector3(0f, 1.7f, 26f), new Vector3(0f, 3.4f, 32f), new Vector3(0f, 3.4f, 39f) })
            {
                kit.Carpet(point, new Vector2(2.6f, 2.5f));
                kit.ReadingLamp(point + new Vector3(-2f, 0f, 0.9f), 45f, Blue);
            }

            // Uma última curva em piso estável: sem novos poderes nem outra pegadinha depois da subida.
            kit.BookPlatform("E18 Passarela final", new Vector3(0f, 3.4f, 46f), new Vector2(3f, 8f));
            kit.OpenBookPlatform("E18 Ultima pagina", new Vector3(0f, 3.4f, 54f), new Vector2(10f, 8f));
            kit.Carpet(new Vector3(0f, 3.4f, 47f), new Vector2(2f, 6f));
            kit.Carpet(new Vector3(0f, 3.4f, 53f), new Vector2(3f, 5f));
            kit.Goal(new Vector3(0f, 3.4f, 56f), 180f);
            kit.Sign("A ULTIMA PAGINA E SUA", new Vector3(0f, 7.2f, 56.5f), 0f, 0.09f);
            kit.ReadingDesk("Mesa do epilogo", new Vector3(3.2f, 3.4f, 54f), 90f);
            kit.ReadingLamp(new Vector3(3.2f, 4.2f, 54f), 90f);
            kit.Pole(new Vector3(-3.8f, 3.4f, 52f), Blue);
            kit.Pole(new Vector3(3.8f, 3.4f, 52f), Blue);

            // As estantes e páginas flutuantes ficam fora da área de movimento.
            kit.LibraryBackdrop(new Vector2(-8f, -4f), new Vector2(8f, 58f), -5f, 18f, 8f);
            for (int i = 0; i < 5; i++)
            {
                kit.FloatingBooks($"Jardim oeste {i}", new Vector3(-10f, 4f + i, 8f + i * 10f), 1.4f, 4);
                kit.FloatingPages($"Folhas leste {i}", new Vector3(10f, 5f + i, 7f + i * 10f), 1.4f, 6);
            }
            kit.DreamClock("Sol entre as paginas", new Vector3(0f, 15f, 65f), 4f, 0f, 1f);
            kit.DreamMotes("Polen de letras", new Vector3(0f, 13f, 28f), new Vector3(24f, 4f, 66f), Blue, 180);
            kit.FallZone(-7f);
        }
    }
}
