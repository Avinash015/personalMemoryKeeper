using UnityEngine;
using TMPro;

public class TapToSelectManager : MonoBehaviour
{
    public Camera arCamera;
    public TextMeshProUGUI debugText;

    // Make sure this matches your Unity layer (e.g., ARImage)
    public string targetLayerName = "ARImage";
    private int targetLayer;

    void Start()
    {
        targetLayer = LayerMask.NameToLayer(targetLayerName);

        if (targetLayer == -1)
        {
            debugText.text = "⚠️ Target Layer '" + targetLayerName + "' not found!";
        }
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            debugText.text = "📱 Touch Detected!\n";

            Ray ray = arCamera.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            int layerMask = 1 << targetLayer;
            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 2f);
            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObject = hit.collider.gameObject;
                debugText.text += "✅ Object Hit: " + clickedObject.name;

                UserImageMetadata metadata = clickedObject.GetComponent<UserImageMetadata>();
                if (metadata != null)
                {
                    debugText.text += "\n🕒 Metadata: " + metadata.placedTime;
                }
                else
                {
                    debugText.text += "\n⚠️ No Metadata Found!";
                }
            }
            else
            {
                debugText.text += "❌ Raycast Missed (Check Layer + Collider)";
            }
        }
    }
}
