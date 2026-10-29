using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SavePositionManager : MonoBehaviour
{
    public void SavePosition(GameObject imageObject)
    {
        if (imageObject == null)
        {
            Debug.LogWarning("No image object provided for saving position!");
            return;
        }

        Vector3 pos = imageObject.transform.position;
        string memoryId = imageObject.name; // Store memory _id as object name

        Debug.Log($"Saving position for {memoryId}: {pos}");
        StartCoroutine(SendPositionToServer(memoryId, pos));
    }

    IEnumerator SendPositionToServer(string id, Vector3 position)
    {
        string url = $"http://192.168.0.111:8000/api/memories/save-position/{id}";
        
        WWWForm form = new WWWForm();
        form.AddField("x", position.x.ToString());
        form.AddField("y", position.y.ToString());
        form.AddField("z", position.z.ToString());

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Position saved successfully!");
            }
            else
            {
                Debug.LogError("❌ Failed to save position: " + request.error);
            }
        }
    }
}
