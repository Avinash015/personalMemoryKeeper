using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.IO; // For File and Path


public class ImagePlacerdemo : MonoBehaviour
{
    public GameObject imagePrefab;
    public TextMeshProUGUI debugText;
    public ARRaycastManager raycastManager;
    public ARPlaneManager verticalPlaneManager;
    public GameObject uploadButton;
    public GameObject createPrefabButton;

    private Texture2D selectedImageTexture;
    private string selectedImagePath; // ✅ Declared to store selected image path
    private GameObject customImagePrefab;

    private bool canPlaceImage = false;
    
    private string scannedQRCode = "";
    private string filePath;
    private List<GameObject> placedImages = new List<GameObject>();
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        debugText.text = "Scan QR to start.";
    }

    void Update()
    {
        if (canPlaceImage && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (raycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                PlaceImage(hitPose.position, hitPose.rotation);
            }
        }
    }
    private List<GameObject> placedObjects = new List<GameObject>();

    public void RegisterPlacedObject(GameObject obj)
    {
        placedObjects.Add(obj);
    }

    public void ClearPlacedImages()
    {
        foreach (var obj in placedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        placedObjects.Clear();
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void UploadImage()
    {
        debugText.text = "Uploading image...";

    #if UNITY_ANDROID
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path);
                if (texture != null)
                {
                    selectedImageTexture = texture;
                    selectedImagePath = path; // ✅ Store image path
                    filePath = path; 
                    debugText.text = "Step 2: Image loaded. Click to create prefab.";
                    createPrefabButton.SetActive(true);
                    uploadButton.SetActive(false);

                    // ✅ Start uploading to server
                    //StartCoroutine(UploadImageToServer(path));
                }
                else
                {
                    debugText.text = "Failed to load image.";
                }
            }
            else
            {
                debugText.text = "Image selection canceled.";
            }
        }, "Select an image to use in AR");

        if (permission != NativeGallery.Permission.Granted)
        {
            debugText.text = "Permission denied!";
        }
    #endif
    }
    private IEnumerator UploadImageToServer(string filePath)
    {
        string userId = "67b7793ac15248c18d25b37f"; // Replace with actual user ID
        string qrCode = scannedQRCode;

        byte[] imageData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddField("userId", userId);
        form.AddField("qrCode", qrCode);
        form.AddBinaryData("image", imageData, Path.GetFileName(filePath), "image/png");

        using (UnityWebRequest www = UnityWebRequest.Post("http://192.168.29.142:8000/api/memories/upload", form))
        {
            debugText.text = "Uploading image to server...";

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Upload failed: " + www.error);
                debugText.text = "Upload failed: " + www.error;
            }
            else
            {
                Debug.Log("Upload successful!");
                debugText.text = "Image uploaded to server successfully!";
            }
        }
    }



    
    public void CreateImagePrefab()
    {
        debugText.text = "Creating prefab from image...";

        customImagePrefab = new GameObject("UserImage");
        var renderer = customImagePrefab.AddComponent<SpriteRenderer>();
        var sprite = Sprite.Create(selectedImageTexture, new Rect(0, 0, selectedImageTexture.width, selectedImageTexture.height), new Vector2(0.5f, 0.5f));
        renderer.sprite = sprite;

        // ✅ Dynamic scaling based on image size
        float scaleFactor = 0.5f; // You can tweak this for bigger/smaller default size
        float maxDimension = Mathf.Max(selectedImageTexture.width, selectedImageTexture.height);
        customImagePrefab.transform.localScale = Vector3.one * scaleFactor / maxDimension;
        

        debugText.text = "Step 3: Tap on a vertical plane to place image.";

        imagePrefab = customImagePrefab;
        canPlaceImage = true;

        createPrefabButton.SetActive(false);
    }


    public void PrepareToUploadImage()
    {
        debugText.text = "Step 1: Click Upload to select image.";
        uploadButton.SetActive(true);
        createPrefabButton.SetActive(false);
        canPlaceImage = false;
    }

    public void OnQRScanned(string qrCode)
    {
        scannedQRCode = qrCode;
        debugText.text = "Detecting vertical plane...";
    }

    public void HandleImagePlacementOrRetrieval()
    {
        int count = PlayerPrefs.GetInt(scannedQRCode + "_ImageCount", 0);

        if (count > 0)
        {
            debugText.text = $"{count} image(s) retrieved from {scannedQRCode} location.";
            StartCoroutine(WaitForVerticalPlaneAndRetrieve());
        }
        else
        {
            debugText.text = $"No images found for {scannedQRCode} location.";
        }
    }

    private IEnumerator WaitForVerticalPlaneAndRetrieve()
    {
        while (true)
        {
            foreach (var plane in verticalPlaneManager.trackables)
            {
                if (plane.alignment == PlaneAlignment.Vertical)
                {
                    PlaceAllStoredImages(plane);
                    yield break;
                }
            }
            yield return null;
        }
    }

    private void PlaceAllStoredImages(ARPlane plane)
    {
        int count = PlayerPrefs.GetInt(scannedQRCode + "_ImageCount", 0);

        for (int i = 0; i < count; i++)
        {
            float x = PlayerPrefs.GetFloat(scannedQRCode + "_Image" + i + "_X");
            float y = PlayerPrefs.GetFloat(scannedQRCode + "_Image" + i + "_Y");
            float z = PlayerPrefs.GetFloat(scannedQRCode + "_Image" + i + "_Z");

            float rotX = PlayerPrefs.GetFloat(scannedQRCode + "_Image" + i + "_RotX");
            float rotY = PlayerPrefs.GetFloat(scannedQRCode + "_Image" + i + "_RotY");
            float rotZ = PlayerPrefs.GetFloat(scannedQRCode + "_Image" + i + "_RotZ");

            string imagePath = PlayerPrefs.GetString(scannedQRCode + "_Image" + i + "_Path");

            Vector3 position = new Vector3(x, y, z);
            Quaternion rotation = Quaternion.Euler(rotX, rotY, rotZ);

            if (System.IO.File.Exists(imagePath))
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(imagePath, 512);
                if (texture != null)
                {
                    
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    GameObject newImage = new GameObject("RestoredImage_" + i);
                    string timestamp = PlayerPrefs.GetString(scannedQRCode + "_Image" + i + "_Timestamp", "Unknown time");

                    // Optional: show timestamp on screen
                    GameObject timestampGO = new GameObject("TimestampText_" + i);
                    timestampGO.transform.position = position + new Vector3(0, 0.15f, 0); // slightly above image (image height is ~0.05f)

                    TextMeshPro textMesh = timestampGO.AddComponent<TextMeshPro>();
                    textMesh.text = "Uploaded at: " + timestamp;
                    textMesh.fontSize = 8; // 🔼 Increased font size
                    textMesh.color = Color.black; // ⬛ Changed to black
                    textMesh.alignment = TextAlignmentOptions.Center;
                    timestampGO.transform.LookAt(Camera.main.transform);
                    timestampGO.transform.Rotate(0, 180f, 0); // face the camera
                    timestampGO.transform.localScale = Vector3.one * 0.01f;


                    newImage.transform.position = position;

                    // Create and assign sprite renderer
                    SpriteRenderer renderer = newImage.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;

                    // Face camera directly
                    Vector3 cameraDirection = Camera.main.transform.position - position;
                    cameraDirection.y = 0; // Ignore vertical tilt
                    newImage.transform.rotation = Quaternion.LookRotation(cameraDirection.normalized);

                    // ✅ FIX 1: Flip image horizontally by scaling X negatively
                    newImage.transform.localScale = new Vector3(-0.05f, 0.05f, 0.01f);

                }
                else
                {
                    Debug.LogWarning("Texture is null for path: " + imagePath);
                }
            }
            else
            {
                Debug.LogWarning("Image file not found at path: " + imagePath);
            }
        }
    }


    public void EnableImagePlacement()
    {
        canPlaceImage = true;
        debugText.text = "Tap on detected plane to place an image.";
    }

    private void PlaceImage(Vector3 position, Quaternion rotation)
    {
        if (imagePrefab != null)
        {
            GameObject placed = Instantiate(imagePrefab, position, rotation);

            // ✅ Make image face camera horizontally (like a poster)
            Vector3 cameraDirection = Camera.main.transform.position - position;
            cameraDirection.y = 0; // Prevent vertical tilt
            placed.transform.rotation = Quaternion.LookRotation(cameraDirection.normalized);

            // ✅ Set consistent scale (same as retrieved version)
            placed.transform.localScale = new Vector3(-0.05f, 0.05f, 0.01f); // Flip horizontally if needed

            // ✅ Optional collider for interaction
            // if (placed.GetComponent<Collider>() == null)
            // {
            //     BoxCollider box = placed.AddComponent<BoxCollider>();
            //     box.size = new Vector3(1f, 1f, 0.01f);
            //     box.center = Vector3.zero;
            // }

            RegisterPlacedObject(placed);
            placedImages.Add(placed);

            // ✅ Save position, rotation, and selected image path
            SaveImage(position, rotation, selectedImagePath);

            debugText.text = "Image placed! Tap again to add more.";
            canPlaceImage = false;
        }
    }

    

    private void SaveImage(Vector3 position, Quaternion rotation, string imagePath)
    {
        int count = PlayerPrefs.GetInt(scannedQRCode + "_ImageCount", 0);

        PlayerPrefs.SetFloat(scannedQRCode + "_Image" + count + "_X", position.x);
        PlayerPrefs.SetFloat(scannedQRCode + "_Image" + count + "_Y", position.y);
        PlayerPrefs.SetFloat(scannedQRCode + "_Image" + count + "_Z", position.z);

        PlayerPrefs.SetFloat(scannedQRCode + "_Image" + count + "_RotX", rotation.eulerAngles.x);
        PlayerPrefs.SetFloat(scannedQRCode + "_Image" + count + "_RotY", rotation.eulerAngles.y);
        PlayerPrefs.SetFloat(scannedQRCode + "_Image" + count + "_RotZ", rotation.eulerAngles.z);

        PlayerPrefs.SetString(scannedQRCode + "_Image" + count + "_Path", imagePath); // ✅ Save path
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        PlayerPrefs.SetString(scannedQRCode + "_Image" + count + "_Timestamp", timestamp); // 🆕 Store timestamp


        PlayerPrefs.SetInt(scannedQRCode + "_ImageCount", count + 1);
        PlayerPrefs.Save();
    }
}
