using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ViewMemories : MonoBehaviour
{
    public GameObject memoryItemPrefab; // Assign this in the Inspector
    public Transform memoryScrollViewContent; // Assign Content of Scroll View in Inspector
    public string fetchMemoriesURL = "http://192.168.0.111:8000/api/memories/view"; // Change this to your server URL

    void Start()
    {
        StartCoroutine(FetchMemories());
    }

    IEnumerator FetchMemories()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(fetchMemoriesURL))
        {
            request.timeout = 10;  // Set timeout to 10 seconds
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to fetch memories: " + request.error);
            }
            else
            {
                Debug.Log("Fetched Memories: " + request.downloadHandler.text);
                DisplayMemories(request.downloadHandler.text);
            }
        }
    }

    void DisplayMemories(string json)
    {
        Debug.Log("Server Response: " + json);

        MemoryData[] memories = JsonHelper.FromJson<MemoryData>(json);
        if (memories == null || memories.Length == 0)
        {
            Debug.LogError("MemoryData is NULL or Empty! Check API response format.");
            return;
        }

        foreach (var memory in memories)
        {
            GameObject memoryItem = Instantiate(memoryItemPrefab, memoryScrollViewContent);
            Image memoryImage = memoryItem.transform.Find("MemoryImage").GetComponent<Image>();

            StartCoroutine(LoadImage(memory.imageURL, memoryImage));
        }
    }




    IEnumerator LoadImage(string url, Image imageComponent)
    {
        Debug.Log($"Loading image from: {url}");

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load image: {request.error} | URL: {url}");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                Debug.Log($"Successfully loaded image: {url}");
            }
        }
    }


    [System.Serializable]
    public class MemoryData
    {
        public string _id;
        public string imageURL;
    }
}
