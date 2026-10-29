using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 newPos = Camera.main.ScreenToWorldPoint(eventData.position);
        newPos.z = transform.position.z;
        transform.position = newPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        FindObjectOfType<SavePositionManager>().SavePosition(gameObject);
    }
}
