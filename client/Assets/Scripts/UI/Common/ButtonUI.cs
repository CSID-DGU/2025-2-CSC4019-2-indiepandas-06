using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Element References")]
    [SerializeField] private Image backImage;
    [SerializeField] private TMP_Text labelText;

    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite pressedSprite;

    [Header("Text Offset")]
    [SerializeField] private Vector2 textHoverAmount;
    [SerializeField] private Vector2 textPressedAmount;

    private Vector2 originalPos;
    private bool isHovering = false;

    void Awake() {
        if (labelText != null) {
            originalPos = labelText.rectTransform.anchoredPosition;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        backImage.sprite = hoverSprite;
        labelText.rectTransform.anchoredPosition = originalPos + textHoverAmount;
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        backImage.sprite = normalSprite;
        labelText.rectTransform.anchoredPosition = originalPos;
        isHovering = false;
    }

    public void OnPointerDown(PointerEventData eventData) {
        backImage.sprite = pressedSprite;
        labelText.rectTransform.anchoredPosition = originalPos + textPressedAmount;
    }

    public void OnPointerUp(PointerEventData eventData) {
        backImage.sprite = hoverSprite;
        labelText.rectTransform.anchoredPosition = originalPos + (isHovering ? textHoverAmount : Vector2.zero);
    }
}