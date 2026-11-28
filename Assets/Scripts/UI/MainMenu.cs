using System.Collections.Generic;
using Dreamscape.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Dreamscape.UI
{
    /// <summary>
    /// Menu inicial, montado por código: introdução, menu, opções, controles e créditos. As artes ficam em
    /// Resources/Menu (Background e Logo) e a fonte em Resources/Fonts. Sem Logo, o título é escrito com a fonte.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MainMenu : MonoBehaviour
    {
        static readonly Color TitleColor = new Color(1f, 0.84f, 0.29f);
        static readonly Color ArrowColor = new Color(0.65f, 1f, 0.95f);
        static readonly Color OutlineColor = new Color(0.10f, 0.05f, 0.22f);
        static readonly Vector2 ReferenceSize = new Vector2(1920f, 1080f);

        // enquadramento dos créditos: aproxima do estudante no pufe, como no storyboard
        const float CreditsZoom = 1.75f;
        static readonly Vector2 CreditsFocus = new Vector2(0.84f, 0.18f);

        readonly List<GameObject> screens = new List<GameObject>();
        RectTransform background;
        Font font;
        Sprite logo;
        GameObject currentScreen;
        GameObject intro, main, options, controls, credits, confirm;
        Text blinkLabel;
        System.Action back;

        void Awake()
        {
            font = Resources.Load<Font>("Fonts/PressStart2P");
            logo = Resources.Load<Sprite>("Menu/Logo");
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            EnsureEventSystem();
            BuildCanvas();
            BuildAll();
            Show(intro, null);
        }

        void Update()
        {
            if (currentScreen == intro)
            {
                blinkLabel.enabled = Mathf.Repeat(Time.unscaledTime, 1.2f) < 0.8f;
                if (AnyInputPressed())
                    OpenMain();
                return;
            }

            bool cancel = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                          (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
            if (cancel)
                back?.Invoke();

            // clicar fora não pode deixar o menu sem seleção: teclado e controle parariam de funcionar
            EventSystem events = EventSystem.current;
            if (events != null && events.currentSelectedGameObject == null && firstSelected != null)
                events.SetSelectedGameObject(firstSelected);
        }

        static bool AnyInputPressed()
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                return true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                return true;
            Gamepad pad = Gamepad.current;
            return pad != null && (pad.startButton.wasPressedThisFrame || pad.buttonSouth.wasPressedThisFrame);
        }

        void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            EventSystem.current = go.GetComponent<EventSystem>();
        }

        // ------------------------------------------------------------ navegação

        GameObject firstSelected;

        void Show(GameObject screen, System.Action onBack, GameObject select = null)
        {
            foreach (GameObject s in screens)
                s.SetActive(s == screen);
            currentScreen = screen;
            back = onBack;
            firstSelected = select;
            bool closeUp = screen == credits;
            background.localScale = Vector3.one * (closeUp ? CreditsZoom : 1f);
            background.pivot = closeUp ? CreditsFocus : Vector2.one * 0.5f;
            background.anchoredPosition = Vector2.zero;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(select);
        }

        void OpenMain()
        {
            bool hasSave = GameFlow.HasSave;
            continueButton.interactable = hasSave;
            newGameButton.interactable = GameFlow.CanStartNewGame;
            continueButton.GetComponent<MenuItem>().Refresh(false);
            newGameButton.GetComponent<MenuItem>().Refresh(false);
            Button first = hasSave ? continueButton : newGameButton.interactable ? newGameButton : optionsButton;
            Show(main, null, first.gameObject);
            RelinkVertical(mainButtons);
        }

        void OpenOptions()
        {
            RefreshOptions();
            Show(options, OpenMain, volumeRow.gameObject);
        }

        void OpenControls() => Show(controls, OpenOptions, controlsBack.gameObject);

        void OpenCredits() => Show(credits, OpenMain, creditsBack.gameObject);

        void OnNewGame()
        {
            if (GameFlow.HasSave)
                Show(confirm, OpenMain, confirmNo.gameObject);
            else
                GameFlow.NewGame();
        }

        // ------------------------------------------------------------ opções

        void ChangeVolume(float delta)
        {
            GameSettings.Volume = Mathf.Round((GameSettings.Volume + delta) * 10f) / 10f;
            RefreshOptions();
        }

        void ToggleLanguage()
        {
            GameSettings.English = !GameSettings.English;
            RebuildTexts();
            RefreshOptions();
        }

        void RefreshOptions()
        {
            volumeRow.Label.text = $"{GameSettings.Text("VOLUME", "VOLUME")}  < {Mathf.RoundToInt(GameSettings.Volume * 100f)}% >";
            languageRow.Label.text = $"{GameSettings.Text("IDIOMA", "LANGUAGE")}  < {(GameSettings.English ? "EN" : "PT-BR")} >";
        }

        // ------------------------------------------------------------ construção

        readonly List<Button> mainButtons = new List<Button>();
        readonly List<(Text label, string pt, string en)> translated = new List<(Text, string, string)>();
        Button continueButton, newGameButton, optionsButton, controlsBack, creditsBack, confirmNo;
        MenuItem volumeRow, languageRow;
        Transform canvasRoot;

        void BuildCanvas()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceSize;
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            canvasRoot = transform;

            GameObject bg = NewRect("Fundo", canvasRoot);
            background = bg.GetComponent<RectTransform>();
            var image = bg.AddComponent<Image>();
            image.sprite = Resources.Load<Sprite>("Menu/Background");
            image.color = image.sprite != null ? Color.white : new Color(0.13f, 0.09f, 0.27f);
            image.raycastTarget = false;
            var fitter = bg.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = image.sprite != null ? image.sprite.rect.width / image.sprite.rect.height : 16f / 9f;
        }

        void BuildAll()
        {
            BuildIntro();
            BuildMain();
            BuildOptions();
            BuildControls();
            BuildCredits();
            BuildConfirm();
            RebuildTexts();
        }

        void RebuildTexts()
        {
            foreach ((Text label, string pt, string en) in translated)
                label.text = GameSettings.Text(pt, en);
        }

        void BuildIntro()
        {
            intro = NewScreen("Introducao");
            AddLogo(intro.transform);
            Text pt = AddText(intro.transform, "PRESSIONE QUALQUER TECLA", 38, TitleColor, new Vector2(0.27f, 0.47f));
            blinkLabel = pt;
            Size(pt, 1040f, 70f);
            Text en = AddText(intro.transform, "PRESS ANY KEY TO START", 24, Color.white, new Vector2(0.27f, 0.41f));
            Size(en, 800f, 50f);
        }

        void BuildMain()
        {
            main = NewScreen("Menu");
            AddLogo(main.transform);
            Vector2 start = new Vector2(0.255f, 0.56f);
            continueButton = AddItem(main.transform, "CONTINUAR", "CONTINUE", Stack(start, 0), GameFlow.Continue);
            newGameButton = AddItem(main.transform, "NOVO JOGO", "NEW GAME", Stack(start, 1), OnNewGame);
            optionsButton = AddItem(main.transform, "OPÇÕES", "OPTIONS", Stack(start, 2), OpenOptions);
            Button creditsButton = AddItem(main.transform, "CRÉDITOS", "CREDITS", Stack(start, 3), OpenCredits);
            Button quit = AddItem(main.transform, "SAIR", "QUIT", Stack(start, 4), GameFlow.Quit);
            mainButtons.AddRange(new[] { continueButton, newGameButton, optionsButton, creditsButton, quit });
        }

        void BuildOptions()
        {
            options = NewScreen("Opcoes");
            AddTitle(options.transform, "OPÇÕES", "OPTIONS");
            Vector2 start = new Vector2(0.25f, 0.68f);
            Button volume = AddItem(options.transform, "", "", Stack(start, 0), () => ChangeVolume(GameSettings.Volume >= 1f ? -1f : 0.1f), true);
            volumeRow = volume.GetComponent<MenuItem>();
            volume.gameObject.AddComponent<LeftRightInput>().Changed = direction => ChangeVolume(direction * 0.1f);
            Button controlsButton = AddItem(options.transform, "CONTROLES", "CONTROLS", Stack(start, 1), OpenControls, true);
            Button language = AddItem(options.transform, "", "", Stack(start, 2), ToggleLanguage, true);
            languageRow = language.GetComponent<MenuItem>();
            language.gameObject.AddComponent<LeftRightInput>().Changed = _ => ToggleLanguage();
            Button backButton = AddItem(options.transform, "VOLTAR", "BACK", Stack(start, 3.4f), OpenMain, true);
            LinkVertical(new List<Button> { volume, controlsButton, language, backButton });
        }

        void BuildControls()
        {
            controls = NewScreen("Controles");
            AddTitle(controls.transform, "CONTROLES", "CONTROLS");
            (string pt, string en, string keysPt, string keysEn)[] rows =
            {
                ("ANDAR", "MOVE", "WASD", "WASD"),
                ("CORRER", "RUN", "SHIFT", "SHIFT"),
                ("PULAR", "JUMP", "ESPAÇO", "SPACE"),
                ("AGACHAR", "CROUCH", "C", "C"),
                ("CÂMERA", "CAMERA", "MOUSE", "MOUSE"),
                ("PAUSAR", "PAUSE", "ESC", "ESC"),
            };
            for (int i = 0; i < rows.Length; i++)
            {
                float y = 0.70f - i * 0.075f;
                Text action = AddTranslated(controls.transform, rows[i].pt, rows[i].en, 30, Color.white, new Vector2(0.07f, y));
                action.alignment = TextAnchor.MiddleLeft;
                Size(action, 420f, 60f, new Vector2(0f, 0.5f));
                Text key = AddTranslated(controls.transform, rows[i].keysPt, rows[i].keysEn, 30, TitleColor, new Vector2(0.30f, y));
                key.alignment = TextAnchor.MiddleLeft;
                Size(key, 640f, 60f, new Vector2(0f, 0.5f));
            }
            controlsBack = AddItem(controls.transform, "VOLTAR", "BACK", new Vector2(0.17f, 0.14f), OpenOptions);
            LinkVertical(new List<Button> { controlsBack });
        }

        void BuildCredits()
        {
            credits = NewScreen("Creditos");
            // o fundo aproximado é detalhado demais atrás dos nomes
            credits.AddComponent<Image>().color = new Color(0.05f, 0.03f, 0.13f, 0.45f);
            AddTitle(credits.transform, "CRÉDITOS", "CREDITS");
            Text heading = AddTranslated(credits.transform, "INTEGRANTES:", "TEAM:", 34, Color.white, new Vector2(0.07f, 0.68f));
            heading.alignment = TextAnchor.MiddleLeft;
            Size(heading, 700f, 60f, new Vector2(0f, 0.5f));
            string[] names = { "João Victor Feijó", "Nelson Prado", "João Paulo Del Vecchio", "Lucas P. Magalhães" };
            for (int i = 0; i < names.Length; i++)
            {
                Text name = AddText(credits.transform, names[i], 28, Color.white, new Vector2(0.07f, 0.60f - i * 0.07f));
                name.alignment = TextAnchor.MiddleLeft;
                Size(name, 780f, 50f, new Vector2(0f, 0.5f));
            }
            creditsBack = AddItem(credits.transform, "VOLTAR", "BACK", new Vector2(0.17f, 0.14f), OpenMain);
            LinkVertical(new List<Button> { creditsBack });
        }

        void BuildConfirm()
        {
            confirm = NewScreen("Confirmar");
            var dim = confirm.AddComponent<Image>();
            dim.color = new Color(0.05f, 0.03f, 0.13f, 0.72f);
            Text question = AddTranslated(confirm.transform, "APAGAR O JOGO SALVO?", "DELETE SAVED GAME?", 40, TitleColor, new Vector2(0.5f, 0.62f));
            Size(question, 1400f, 80f);
            Button yes = AddItem(confirm.transform, "SIM", "YES", new Vector2(0.5f, 0.48f), GameFlow.NewGame);
            confirmNo = AddItem(confirm.transform, "NÃO", "NO", new Vector2(0.5f, 0.39f), OpenMain);
            LinkVertical(new List<Button> { yes, confirmNo });
        }

        static Vector2 Stack(Vector2 start, float index) => start - new Vector2(0f, index * 0.082f);

        GameObject NewScreen(string screenName)
        {
            GameObject go = NewRect(screenName, canvasRoot);
            Stretch(go.GetComponent<RectTransform>());
            screens.Add(go);
            return go;
        }

        void AddLogo(Transform parent)
        {
            if (logo != null)
            {
                GameObject go = NewRect("Logo", parent);
                var image = go.AddComponent<Image>();
                image.sprite = logo;
                image.preserveAspect = true;
                image.raycastTarget = false;
                Place(go.GetComponent<RectTransform>(), new Vector2(0.27f, 0.765f));
                go.GetComponent<RectTransform>().sizeDelta = new Vector2(860f, 340f);
                return;
            }
            Text title = AddText(parent, "DREAMSCAPE", 76, TitleColor, new Vector2(0.27f, 0.765f));
            Size(title, 900f, 130f);
        }

        void AddTitle(Transform parent, string pt, string en)
        {
            Text title = AddTranslated(parent, pt, en, 56, TitleColor, new Vector2(0.07f, 0.86f));
            title.alignment = TextAnchor.MiddleLeft;
            Size(title, 1000f, 100f, new Vector2(0f, 0.5f));
        }

        Button AddItem(Transform parent, string pt, string en, Vector2 anchor, UnityEngine.Events.UnityAction onClick, bool alignLeft = false)
        {
            GameObject go = NewRect(string.IsNullOrEmpty(pt) ? "Linha" : pt, parent);
            RectTransform rect = go.GetComponent<RectTransform>();
            Place(rect, anchor);
            rect.sizeDelta = new Vector2(780f, 76f);
            // gráfico invisível: é o que recebe o clique em toda a linha, não só nas letras
            var hit = go.AddComponent<Image>();
            hit.color = Color.clear;

            Text label = string.IsNullOrEmpty(pt)
                ? AddText(go.transform, "", 44, Color.white, new Vector2(0.5f, 0.5f))
                : AddTranslated(go.transform, pt, en, 44, Color.white, new Vector2(0.5f, 0.5f));
            Stretch(label.rectTransform);
            if (alignLeft)
            {
                label.alignment = TextAnchor.MiddleLeft;
                label.rectTransform.offsetMin = new Vector2(70f, 0f);
            }

            GameObject arrow = NewRect("Seta", go.transform);
            RectTransform arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.anchorMin = arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
            arrowRect.sizeDelta = new Vector2(70f, 70f);
            float width = string.IsNullOrEmpty(pt) ? 400f : Mathf.Max(pt.Length, en.Length) * 22f + 44f;
            arrowRect.anchoredPosition = new Vector2(-width, 0f);
            if (alignLeft)
            {
                arrowRect.anchorMin = arrowRect.anchorMax = new Vector2(0f, 0.5f);
                arrowRect.anchoredPosition = new Vector2(30f, 0f);
            }
            var arrowText = arrow.AddComponent<Text>();
            arrowText.font = font;
            arrowText.text = ">";
            arrowText.fontSize = 44;
            arrowText.color = ArrowColor;
            arrowText.alignment = TextAnchor.MiddleCenter;
            arrowText.raycastTarget = false;
            arrowText.horizontalOverflow = HorizontalWrapMode.Overflow;
            AddOutline(arrow);
            arrow.SetActive(false);

            var button = go.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(onClick);
            var item = go.AddComponent<MenuItem>();
            item.Label = label;
            item.Arrow = arrow;
            return button;
        }

        Text AddTranslated(Transform parent, string pt, string en, int size, Color color, Vector2 anchor)
        {
            Text text = AddText(parent, pt, size, color, anchor);
            translated.Add((text, pt, en));
            return text;
        }

        Text AddText(Transform parent, string content, int size, Color color, Vector2 anchor)
        {
            GameObject go = NewRect("Texto", parent);
            Place(go.GetComponent<RectTransform>(), anchor);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;
            AddOutline(go);
            return text;
        }

        static void AddOutline(GameObject go)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = OutlineColor;
            outline.effectDistance = new Vector2(4f, -4f);
        }

        static void LinkVertical(List<Button> buttons) => RelinkVertical(buttons);

        // botão desabilitado sai da navegação: Continuar sem save não pode prender a seta
        static void RelinkVertical(List<Button> buttons)
        {
            List<Button> active = buttons.FindAll(b => b.interactable);
            for (int i = 0; i < active.Count; i++)
            {
                active[i].navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = active[(i + active.Count - 1) % active.Count],
                    selectOnDown = active[(i + 1) % active.Count],
                };
            }
        }

        static GameObject NewRect(string objectName, Transform parent)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void Place(RectTransform rect, Vector2 anchor)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = Vector2.zero;
        }

        static void Size(Text text, float width, float height, Vector2? pivot = null)
        {
            text.rectTransform.sizeDelta = new Vector2(width, height);
            text.rectTransform.pivot = pivot ?? Vector2.one * 0.5f;
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
