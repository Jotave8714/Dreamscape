using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dreamscape.UI
{
    /// <summary>Opção do menu: fica amarela com a seta quando selecionada, e o mouse seleciona ao passar por cima.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class MenuItem : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
    {
        public static readonly Color Normal = Color.white;
        public static readonly Color Selected = new Color(1f, 0.84f, 0.29f);
        public static readonly Color Disabled = new Color(0.55f, 0.53f, 0.65f);

        public Text Label;
        public GameObject Arrow;

        public void Refresh(bool selected)
        {
            bool interactable = GetComponent<Button>().interactable;
            Label.color = !interactable ? Disabled : selected ? Selected : Normal;
            Arrow.SetActive(selected && interactable);
        }

        public void OnSelect(BaseEventData _) => Refresh(true);

        public void OnDeselect(BaseEventData _) => Refresh(false);

        public void OnPointerEnter(PointerEventData _)
        {
            if (GetComponent<Button>().interactable && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
