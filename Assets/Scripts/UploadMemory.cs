using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class UploadMemory : MonoBehaviour
{
    public Button selectImageButton; // Assign in Unity Editor
    public Button uploadButton; // Assign in Unity Editor
    public TextMeshProUGUI statusText; // Assign in Unity Editor

    private string filePath;
    private bool isSelectingImage = false;  // Prevent multiple selections
    private bool isUploading = false;  // Prevent multiple uploads
    

    void Start()
    {
        selectImageButton.onClick.AddListener(SelectImage);
        uploadButton.onClick.AddListener(UploadImage);
    }

    public void SelectImage()
    {
        if (isSelectingImage) return;  // Prevent multiple calls
        isSelectingImage = true;  // Set flag to true when selecting starts

        // Check and request permission
        if (NativeGallery.CheckPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image) != NativeGallery.Permission.Granted)
        {
            NativeGallery.RequestPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
            isSelectingImage = false;  // Reset flag if permission denied
            return;
        }

        // Open the gallery to pick an image
        StartCoroutine(OpenGallery());
    }

    IEnumerator OpenGallery()
    {
        bool selectionCompleted = false;

        NativeGallery.GetImageFromGallery((path) =>
        {
            selectionCompleted = true; // Mark selection as completed

            if (!string.IsNullOrEmpty(path))
            {
                filePath = path;  // Store the selected file path
                Debug.Log("Selected Image Path: " + filePath);
                statusText.text = "Selected: " + Path.GetFileName(filePath);
            }
        });

        // Wait until the user either selects or cancels
        yield return new WaitUntil(() => selectionCompleted);

        isSelectingImage = false;  // Reset flag once selection/cancel is completed
    }

    public void UploadImage()
    {
        if (isUploading) return; // Prevent multiple uploads

        if (string.IsNullOrEmpty(filePath))
        {
            statusText.text = "No file selected!";
            Debug.LogError("No file selected for upload!");
            return;
        }

        isUploading = true; // Set flag to indicate an upload is in progress
        StartCoroutine(UploadCoroutine());
    }

    IEnumerator UploadCoroutine()
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("Upload failed: filePath is null");
            statusText.text = "No file selected!";
            isUploading = false; // Reset flag on failure
            yield break;
        }

        // Replace with actual userId and qrCode
        string userId = "67b7793ac15248c18d25b37f"; // Example, replace with real userId
        string qrCode = "test"; // Replace with the scanned QR Code value

        byte[] imageData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddField("userId", userId);
        form.AddField("qrCode", qrCode);
        form.AddBinaryData("image", imageData, Path.GetFileName(filePath), "image/png");

        using (UnityWebRequest www = UnityWebRequest.Post("http://192.168.0.111:8000/api/memories/upload", form))
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

        isUploading = false; // Reset flag after upload completes
    }
}
