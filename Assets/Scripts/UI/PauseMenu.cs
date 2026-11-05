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
        Text titleLabel, resumeLabel, restartLabel, menuLabel;
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
            font = Resources.Load<Font>("Fonts/PressStart2P");
            BuildUi();
            panel.SetActive(false);
        }

        void OnDestroy()
        {
            if (GamePause.IsPaused)
                GamePause.Resume();
        }

        void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
                Close();
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
            RefreshTexts();
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

        void BackToMenu()
        {
            Close();
            // cena de teste aberta direto, sem o menu no build
            if (GameFlow.MenuAvailable)
                GameFlow.ReturnToMenu();
            else
                GameFlow.Quit();
        }

        void RefreshTexts()
        {
            titleLabel.text = GameSettings.Text("PAUSADO", "PAUSED");
            resumeLabel.text = GameSettings.Text("CONTINUAR", "RESUME");
            restartLabel.text = GameSettings.Text("REINICIAR FASE", "RESTART LEVEL");
            menuLabel.text = GameSettings.Text("MENU PRINCIPAL", "MAIN MENU");
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

            GameObject title = CreateLabel("Titulo", panel.transform, "", 64);
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(800f, 120f);
            titleLabel = title.GetComponent<Text>();

            resumeButton = CreateButton("Continuar", Close, out resumeLabel);
            restartButton = CreateButton("Reiniciar fase", Restart, out restartLabel);
            CreateButton("Menu principal", BackToMenu, out menuLabel);
            RefreshTexts();
        }

        Button CreateButton(string text, UnityEngine.Events.UnityAction onClick, out Text labelText)
        {
            GameObject go = CreateRect(text, panel.transform);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 76f);
            go.AddComponent<Image>().color = Color.white;

            var button = go.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = ButtonHighlightColor;
            colors.selectedColor = ButtonHighlightColor;
            colors.pressedColor = ButtonHighlightColor;
            button.colors = colors;
            button.onClick.AddListener(onClick);

            GameObject label = CreateLabel("Texto", go.transform, text, 26);
            Stretch(label.GetComponent<RectTransform>());
            labelText = label.GetComponent<Text>();
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
