using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ImageDisplay : MonoBehaviour
{
    private Renderer imageRenderer;

    void Start()
    {
        imageRenderer = GetComponent<Renderer>();
    }

    public void SetImage(string imageUrl)
    {
        StartCoroutine(LoadImage(imageUrl));
    }

    IEnumerator LoadImage(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                imageRenderer.material.mainTexture = texture;
            }
            else
            {
                Debug.LogError("Failed to load image: " + request.error);
            }
        }
    }
}
