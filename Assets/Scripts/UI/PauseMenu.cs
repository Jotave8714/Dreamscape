using Dreamscape.Characters;
using Dreamscape.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Dreamscape.UI
{
    /// <summary>
    /// Menu de pause: Esc ou Start abre e fecha. Criado por código e mantido entre cenas, então funciona em
    /// qualquer cena com jogador sem precisar editar o arquivo da cena.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PauseMenu : MonoBehaviour
    {
        static readonly Color OverlayColor = new Color(0.03f, 0.03f, 0.08f, 0.82f);
        static readonly Color ButtonColor = new Color(0.16f, 0.16f, 0.26f, 1f);
        static readonly Color ButtonHighlightColor = new Color(0.30f, 0.30f, 0.50f, 1f);
        static readonly Color TextColor = new Color(0.93f, 0.93f, 0.97f, 1f);

        GameObject panel;
        Button resumeButton;
        Button restartButton;
        Font font;
        PlayerInputController pausedPlayer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var go = new GameObject(nameof(PauseMenu));
            DontDestroyOnLoad(go);
            go.AddComponent<PauseMenu>();
        }

        void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildUi();
            panel.SetActive(false);
        }

        void OnDestroy()
        {
            if (GamePause.IsPaused)
                GamePause.Resume();
        }

        void Update()
        {
            bool toggle = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                          (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
            if (!toggle)
                return;

            if (GamePause.IsPaused)
                Close();
            else
                Open();
        }

        void Open()
        {
            // cena sem jogador (ex.: um menu principal futuro) não tem o que pausar
            pausedPlayer = FindAnyObjectByType<PlayerInputController>();
            if (pausedPlayer == null)
                return;

            GamePause.Pause();
            // sem isso, Espaço no menu pediria um pulo com o tempo parado e ele sairia ao despausar
            pausedPlayer.SetInputEnabled(false);

            EnsureEventSystem();
            restartButton.interactable = SceneManager.GetActiveScene().buildIndex >= 0;
            panel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }

        void Close()
        {
            panel.SetActive(false);
            if (pausedPlayer != null)
                pausedPlayer.SetInputEnabled(true);
            pausedPlayer = null;
            GamePause.Resume();
        }

        void Restart()
        {
            Close();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.transform.SetParent(transform);
            EventSystem.current = go.GetComponent<EventSystem>();
        }

        // ------------------------------------------------------------ construção da UI

        void BuildUi()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            panel = CreateRect("Painel", transform);
            Stretch(panel.GetComponent<RectTransform>());
            panel.AddComponent<Image>().color = OverlayColor;
            var layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 24f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            GameObject title = CreateLabel("Titulo", panel.transform, "PAUSADO", 72);
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 120f);

            resumeButton = CreateButton("Continuar", Close);
            restartButton = CreateButton("Reiniciar fase", Restart);
            CreateButton("Sair", Quit);
        }

        Button CreateButton(string text, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = CreateRect(text, panel.transform);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(420f, 76f);
            go.AddComponent<Image>().color = Color.white;

            var button = go.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = ButtonHighlightColor;
            colors.selectedColor = ButtonHighlightColor;
            colors.pressedColor = ButtonHighlightColor;
            button.colors = colors;
            button.onClick.AddListener(onClick);

            GameObject label = CreateLabel("Texto", go.transform, text, 36);
            Stretch(label.GetComponent<RectTransform>());
            return button;
        }

        GameObject CreateLabel(string objectName, Transform parent, string text, int size)
        {
            GameObject go = CreateRect(objectName, parent);
            var label = go.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = TextColor;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            return go;
        }

        static GameObject CreateRect(string objectName, Transform parent)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
