using UnityEngine;
using UnityEngine.EventSystems;

public class ResizableBox : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public RectTransform panel;  // Assign your Panel in the Inspector
    public RectTransform resizeHandle;  // Assign the Resize Handle in Inspector

    private Vector2 originalSize;
    private Vector2 originalMousePosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        originalSize = panel.sizeDelta;
        originalMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 difference = eventData.position - originalMousePosition;
        panel.sizeDelta = new Vector2(originalSize.x + difference.x, originalSize.y + difference.y);
    }
}
