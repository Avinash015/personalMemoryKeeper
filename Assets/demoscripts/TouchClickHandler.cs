using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TouchClickHandler : MonoBehaviour
{
    public Camera arCamera;
    public TextMeshProUGUI debugText;

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.GetTouch(0).position;

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                foreach (var result in results)
                {
                    GameObject clicked = result.gameObject;
                    debugText.text = "🖱️ Clicked: " + clicked.name;

                    UserImageMetadata metadata = clicked.GetComponent<UserImageMetadata>();
                    if (metadata != null)
                    {
                        debugText.text += "\n🕒 Placed At: " + metadata.placedTime;
                    }
                    else
                    {
                        debugText.text += "\n⚠️ No Metadata Found";
                    }

                    break;
                }
            }
            else
            {
                debugText.text = "❌ No UI Raycast hit!";
            }
        }
    }
}
