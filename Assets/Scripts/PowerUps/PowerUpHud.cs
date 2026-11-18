using System.Collections.Generic;
using Dreamscape.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Dreamscape.PowerUps
{
    /// <summary>
    /// Mostra no canto da tela os power-ups ativos e o tempo restante, e desenha o efeito de vento da super velocidade:
    /// linhas saindo do centro da tela e um leve aumento do campo de visão, proporcionais à velocidade.
    /// </summary>
    public sealed class PowerUpHud : MonoBehaviour
    {
        const int MaxRows = 4;
        const int WindLineCount = 44;
        const float WarningSeconds = 5f;
        const float MaxFovKick = 9f;
        const float FullWindSpeed = 7.5f; // corrida com o power-up
        const float NoWindSpeed = 2f;

        static readonly Color PanelColor = new Color(0.05f, 0.03f, 0.13f, 0.75f);
        static readonly Color TrackColor = new Color(1f, 1f, 1f, 0.15f);
        static readonly Color WarningColor = new Color(1f, 0.35f, 0.3f);

        sealed class Row
        {
            public GameObject Root;
            public Text Name, Seconds;
            public Image Fill;
            public RectTransform FillRect;
        }

        sealed class WindLine
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Direction;
            public float Progress, Speed, Length;
        }

        readonly List<Row> rows = new List<Row>();
        readonly List<WindLine> windLines = new List<WindLine>();
        ActivePowerUps powerUps;
        CharacterMotor motor;
        Font font;
        RectTransform windRoot;
        Camera boostedCamera;
        float baseFov;
        float windIntensity;

        public float WindIntensity => windIntensity;

        public void Bind(ActivePowerUps source, CharacterMotor characterMotor)
        {
            powerUps = source;
            motor = characterMotor;
            font = Resources.Load<Font>("Fonts/PressStart2P");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Build();
        }

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // abaixo do menu de pause
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            windRoot = NewRect("Vento", transform);
            windRoot.anchorMin = windRoot.anchorMax = new Vector2(0.5f, 0.5f);
            for (int i = 0; i < WindLineCount; i++)
                windLines.Add(NewWindLine(i / (float)WindLineCount));

            for (int i = 0; i < MaxRows; i++)
                rows.Add(NewRow(i));
        }

        Row NewRow(int index)
        {
            RectTransform root = NewRect("Efeito", transform);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0f, 1f);
            root.anchoredPosition = new Vector2(28f, -28f - index * 86f);
            root.sizeDelta = new Vector2(500f, 76f);
            NewImage(root, PanelColor);

            Text name = NewText(root, 18, TextAnchor.UpperLeft);
            Text seconds = NewText(root, 22, TextAnchor.UpperRight);
            foreach (Text text in new[] { name, seconds })
            {
                RectTransform rect = text.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(14f, 0f);
                rect.offsetMax = new Vector2(-14f, -14f);
            }

            RectTransform track = NewRect("Barra", root);
            track.anchorMin = new Vector2(0f, 0f);
            track.anchorMax = new Vector2(1f, 0f);
            track.pivot = new Vector2(0.5f, 0f);
            track.offsetMin = new Vector2(14f, 12f);
            track.offsetMax = new Vector2(-14f, 26f);
            NewImage(track, TrackColor);

            RectTransform fill = NewRect("Preenchimento", track);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            Image fillImage = NewImage(fill, Color.white);

            root.gameObject.SetActive(false);
            return new Row { Root = root.gameObject, Name = name, Seconds = seconds, Fill = fillImage, FillRect = fill };
        }

        WindLine NewWindLine(float startProgress)
        {
            RectTransform rect = NewRect("Linha", windRoot);
            var line = new WindLine { Rect = rect, Image = NewImage(rect, Color.white), Progress = startProgress };
            line.Image.enabled = false;
            Respawn(line);
            line.Progress = startProgress;
            return line;
        }

        static void Respawn(WindLine line)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            line.Direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            line.Rect.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
            line.Progress = 0f;
            line.Speed = Random.Range(1.6f, 2.8f);
            line.Length = Random.Range(120f, 320f);
        }

        void Update()
        {
            UpdateRows();
            UpdateWind(Time.deltaTime);
        }

        void UpdateRows()
        {
            IReadOnlyList<ActivePowerUps.Effect> effects = powerUps.Effects;
            for (int i = 0; i < rows.Count; i++)
            {
                Row row = rows[i];
                bool active = i < effects.Count;
                row.Root.SetActive(active);
                if (!active)
                    continue;

                ActivePowerUps.Effect effect = effects[i];
                bool warning = effect.Remaining <= WarningSeconds;
                // pisca nos últimos segundos para avisar que vai acabar
                bool blinkOff = warning && Mathf.Repeat(effect.Remaining, 0.5f) < 0.2f;
                Color color = warning ? WarningColor : effect.Pickup.DisplayColor;
                row.Name.text = effect.Pickup.DisplayName;
                row.Name.color = effect.Pickup.DisplayColor;
                row.Seconds.text = $"{Mathf.CeilToInt(effect.Remaining)}s";
                row.Seconds.color = color;
                row.Fill.color = color;
                row.Fill.enabled = !blinkOff;
                row.FillRect.anchorMax = new Vector2(Mathf.Clamp01(effect.Progress), 1f);
            }
        }

        void UpdateWind(float dt)
        {
            if (dt <= 0f)
                return;

            float target = motor.SpeedMultiplier > 1f
                ? Mathf.InverseLerp(NoWindSpeed, FullWindSpeed, motor.HorizontalSpeed)
                : 0f;
            windIntensity = Mathf.MoveTowards(windIntensity, target, dt * 3f);
            UpdateFov();

            RectTransform canvasRect = (RectTransform)transform;
            float inner = canvasRect.rect.height * 0.32f;
            float outer = canvasRect.rect.width * 0.62f;
            foreach (WindLine line in windLines)
            {
                bool visible = windIntensity > 0.01f;
                line.Image.enabled = visible;
                if (!visible)
                    continue;

                line.Progress += dt * line.Speed * (0.5f + windIntensity);
                if (line.Progress >= 1f)
                    Respawn(line);

                float eased = line.Progress * line.Progress;
                line.Rect.anchoredPosition = line.Direction * Mathf.Lerp(inner, outer, eased);
                line.Rect.sizeDelta = new Vector2(line.Length * (0.4f + windIntensity), 3f + 4f * eased);
                line.Image.color = new Color(1f, 1f, 1f, Mathf.Sin(line.Progress * Mathf.PI) * 0.5f * windIntensity);
            }
        }

        void UpdateFov()
        {
            Camera cam = Camera.main;
            if (cam != boostedCamera)
            {
                boostedCamera = cam;
                if (cam != null)
                    baseFov = cam.fieldOfView;
            }
            if (cam != null && !cam.orthographic)
                cam.fieldOfView = baseFov + MaxFovKick * windIntensity;
        }

        void OnDestroy()
        {
            if (boostedCamera != null)
                boostedCamera.fieldOfView = baseFov;
        }

        Text NewText(Transform parent, int size, TextAnchor alignment)
        {
            RectTransform rect = NewRect("Texto", parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        static Image NewImage(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static RectTransform NewRect(string objectName, Transform parent)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
    }
}
