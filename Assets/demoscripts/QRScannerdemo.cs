using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;
using TMPro;
using System.Collections;

public class QRScanner : MonoBehaviour
{
    public ARCameraManager arCameraManager;
    public ARPlaneManager verticalPlaneManager;
    public TextMeshProUGUI qrResultText;
    public GameObject doneButton;
    public GameObject storeButton;
    public GameObject retrieveButton;

    private IBarcodeReader reader = new BarcodeReader();
    private bool isProcessing = false;
    private bool planeDetected = false;
    private string scannedQRCode = "";
    
    private enum AppState { Scanning, AfterScan, StoreMode, RetrieveMode }
    private AppState currentState = AppState.Scanning;
    public GameObject backButton; // assign in the inspector


    void Start()
    {
        verticalPlaneManager.enabled = false;
        doneButton.SetActive(false);
        storeButton.SetActive(false);
        retrieveButton.SetActive(false);
        backButton.SetActive(false);

    }

    void OnEnable() => arCameraManager.frameReceived += OnCameraFrameReceived;
    void OnDisable() => arCameraManager.frameReceived -= OnCameraFrameReceived;

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (isProcessing) return;
        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) return;

        StartCoroutine(ProcessQR(image));
    }

    private IEnumerator ProcessQR(XRCpuImage image)
    {
        isProcessing = true;
        currentState = AppState.AfterScan;
        backButton.SetActive(true);

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGBA32
        };

        Texture2D texture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
        image.Convert(conversionParams, texture.GetRawTextureData<byte>());
        image.Dispose();
        texture.Apply();

        var result = reader.Decode(texture.GetPixels32(), texture.width, texture.height);
        Destroy(texture);

        if (result != null)
        {
            scannedQRCode = result.Text;
            qrResultText.text = "QR Scanned: " + scannedQRCode;

            arCameraManager.frameReceived -= OnCameraFrameReceived;

            verticalPlaneManager.enabled = true;
            StartCoroutine(WaitForVerticalPlane());

            FindObjectOfType<ImagePlacerdemo>().OnQRScanned(scannedQRCode);
        }

        isProcessing = false;
        yield return null;
    }

    private IEnumerator WaitForVerticalPlane()
    {
        qrResultText.text = "Move to detect a vertical plane...";

        while (!planeDetected)
        {
            foreach (var plane in verticalPlaneManager.trackables)
            {
                if (plane.alignment == PlaneAlignment.Vertical)
                {
                    planeDetected = true;
                    doneButton.SetActive(true);
                    qrResultText.text = "Plane detected! Click 'Done'";
                    break;
                }
            }
            yield return null;
        }
    }

    public void OnDoneButtonClicked()
    {
        doneButton.SetActive(false);
        storeButton.SetActive(true);
        retrieveButton.SetActive(true);
        qrResultText.text = "Choose to Store or Retrieve Memory";
        // ✅ Stop plane detection
        verticalPlaneManager.enabled = false;
        
        backButton.SetActive(true); // 👈 SHOW IT NOW
        currentState = AppState.AfterScan;

        
    }

    public void OnStoreButtonClicked()
    {
        storeButton.SetActive(false);
        retrieveButton.SetActive(false);
        verticalPlaneManager.enabled = true;
        qrResultText.text = "QR Scanned: " + scannedQRCode;

        var placer = FindObjectOfType<ImagePlacerdemo>();
        currentState = AppState.StoreMode;
        backButton.SetActive(true);

        placer.PrepareToUploadImage(); // New method

    }
    public void OnBackButtonClicked()
    {
        scannedQRCode = "";
        planeDetected = false;

        // Hide everything
        doneButton.SetActive(false);
        storeButton.SetActive(false);
        retrieveButton.SetActive(false);
        backButton.SetActive(false);

        qrResultText.text = "Scanning QR...";

        // Re-enable QR scanning
        arCameraManager.frameReceived += OnCameraFrameReceived;

        // Disable plane detection
        verticalPlaneManager.enabled = false;

        // Reactivate planes if any were hidden
        foreach (var plane in verticalPlaneManager.trackables)
        {
            plane.gameObject.SetActive(true);
        }

        // 🧹 Clear any placed memory images
        FindObjectOfType<ImagePlacerdemo>()?.ClearPlacedImages();

        currentState = AppState.Scanning;
    }




    public void OnRetrieveButtonClicked()
    {
        storeButton.SetActive(false);
        retrieveButton.SetActive(false);
        verticalPlaneManager.enabled = true;
        qrResultText.text = "QR Scanned: " + scannedQRCode;
        currentState = AppState.RetrieveMode;
        backButton.SetActive(true);

        // ✅ Stop plane detection
        //verticalPlaneManager.enabled = false;

        // ✅ Hide all existing planes
        foreach (var plane in verticalPlaneManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }

        FindObjectOfType<ImagePlacerdemo>().HandleImagePlacementOrRetrieval();
    }
}