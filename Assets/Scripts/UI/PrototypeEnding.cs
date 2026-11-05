using Dreamscape.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Dreamscape.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeEnding : MonoBehaviour
    {
        Font font;

        void Awake()
        {
            GamePause.Resume();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            font = Resources.Load<Font>("Fonts/PressStart2P");

            if (EventSystem.current == null)
            {
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform, false);
                EventSystem.current = events.GetComponent<EventSystem>();
            }

            gameObject.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            gameObject.AddComponent<GraphicRaycaster>();

            RectTransform panel = Rect("Encerramento", transform, new Vector2(1920f, 1080f));
            panel.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.05f, 0.18f);
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 32f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Label(panel, GameSettings.Text("PROTÓTIPO CONCLUÍDO", "PROTOTYPE COMPLETE"), 48, new Vector2(1600f, 100f));
            Label(panel, GameSettings.Text("Obrigado por jogar Dreamscape!", "Thanks for playing Dreamscape!"), 26, new Vector2(1600f, 70f));
            Button again = Button(panel, "NewGame", GameSettings.Text("JOGAR NOVAMENTE", "PLAY AGAIN"), GameFlow.NewGame);
            Button(panel, "MainMenu", GameSettings.Text("MENU PRINCIPAL", "MAIN MENU"), GameFlow.ReturnToMenu);
            EventSystem.current.SetSelectedGameObject(again.gameObject);
        }

        Button Button(Transform parent, string name, string text, UnityEngine.Events.UnityAction action)
        {
            RectTransform rect = Rect(name, parent, new Vector2(800f, 90f));
            rect.gameObject.AddComponent<Image>().color = new Color(0.2f, 0.15f, 0.35f);
            Button button = rect.gameObject.AddComponent<Button>();
            button.onClick.AddListener(action);
            Label(rect, text, 28, rect.sizeDelta);
            return button;
        }

        void Label(Transform parent, string text, int size, Vector2 dimensions)
        {
            Text label = Rect("Texto", parent, dimensions).gameObject.AddComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.text = text;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
        }

        static RectTransform Rect(string name, Transform parent, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * 0.5f;
            rect.sizeDelta = size;
            return rect;
        }
    }
}
