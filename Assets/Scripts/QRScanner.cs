using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;
using ZXing;
using System;
using System.Collections;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContinuousQRScanner : MonoBehaviour
{
    public ARCameraManager arCameraManager;
    public TextMeshProUGUI headingText; // "Scan Location QR"
    public TextMeshProUGUI resultText;  // Display scanned location
    public Button confirmButton;        // Confirm button

    private IBarcodeReader barcodeReader;
    private bool isProcessing = false;
    public static string lastScannedLocation = null; // Static: persists only while app is running

    void Awake()
    {
        barcodeReader = new BarcodeReader();
        headingText.text = "Scan Location QR"; // Set default heading
        confirmButton.onClick.AddListener(ConfirmLocation); // Add button listener
    }

    void OnEnable()
    {
        arCameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        arCameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (isProcessing) return;
        isProcessing = true;
        TryProcessCameraFrame();
    }

    private void TryProcessCameraFrame()
    {
        XRCpuImage image;
        if (!arCameraManager.TryAcquireLatestCpuImage(out image))
        {
            isProcessing = false;
            return;
        }

        StartCoroutine(ProcessCameraImage(image));
    }

    private IEnumerator ProcessCameraImage(XRCpuImage image)
    {
        XRCpuImage.ConversionParams conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.None
        };

        Texture2D texture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
        NativeArray<byte> rawTextureData = texture.GetRawTextureData<byte>();
        image.Convert(conversionParams, rawTextureData);
        image.Dispose();
        texture.Apply();

        yield return null;

        var result = barcodeReader.Decode(texture.GetPixels32(), texture.width, texture.height);
        Destroy(texture);

        if (result != null)
        {
            lastScannedLocation = result.Text; // Store scanned location
            resultText.text = "Location: " + lastScannedLocation;
        }

        isProcessing = false;
    }

    public void ConfirmLocation()
    {
        if (string.IsNullOrEmpty(lastScannedLocation))
        {
            resultText.text = "No location selected!"; // If no QR scanned
        }
        else
        {
            SceneManager.LoadScene("uploadRetrieve"); // Move to uploadRetrieve scene
        }
    }
}
