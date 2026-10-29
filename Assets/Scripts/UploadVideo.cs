using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class UploadVideo : MonoBehaviour
{
    public Button selectVideoButton; // Assign in Unity Editor
    public Button uploadButton; // Assign in Unity Editor
    public TextMeshProUGUI statusText; // Assign in Unity Editor
    private string filePath;

    void Start()
    {
        selectVideoButton.onClick.AddListener(SelectVideo);
        uploadButton.onClick.AddListener(UploadVideoFile);
    }

    public void SelectVideo()
    {
        NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery((path) =>
        {
            if (!string.IsNullOrEmpty(path))
            {
                filePath = path;
                statusText.text = "Selected: " + Path.GetFileName(filePath);
                Debug.Log("Selected Video Path: " + filePath);
            }
        }, "Select a video");
    }

    public void UploadVideoFile()
    {
        if (string.IsNullOrEmpty(filePath))
        {
            statusText.text = "No file selected!";
            Debug.LogError("No file selected for upload!");
            return;
        }
        StartCoroutine(UploadCoroutine());
    }

    IEnumerator UploadCoroutine()
    {
        byte[] videoData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddField("userId", "67b7793ac15248c18d25b37f"); // Example user ID
        form.AddField("qrCode", "test"); // Example QR Code
        form.AddBinaryData("video", videoData, Path.GetFileName(filePath), "video/mp4");

    using (UnityWebRequest www = UnityWebRequest.Post("http://192.168.0.111:8000/api/videos/upload", form))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Upload Failed: " + www.error);
                Debug.LogError("Server Response: " + www.downloadHandler.text);
                statusText.text = "Upload Failed: " + www.error;
            }
            else
            {
                Debug.Log("Upload Success! Server Response: " + www.downloadHandler.text);
                statusText.text = "Upload Success!";
            }
        }
    }
}
