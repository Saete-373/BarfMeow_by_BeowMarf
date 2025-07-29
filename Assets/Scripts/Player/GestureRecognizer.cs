using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
public class GestureRecognizer : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private bool useFrontCamera = true;
    [SerializeField] private int targetWidth; // 640
    [SerializeField] private int targetHeight; // 480
    [SerializeField] private int targetFrameRate; // 30 FPS
    [SerializeField] private bool maintainAspectRatio = true;

    [Header("Capture Settings")]
    [SerializeField] private int captureWidth; // 640
    [SerializeField] private int captureHeight; // 480

    [Header("Display Settings")]
    [SerializeField] private RawImage cameraDisplay;
    [SerializeField] private TMP_Text gestureText;
    [SerializeField] private TMP_Text fpsText;

    [Header("Server Settings")]
    [SerializeField] private string serverUrl = "http://localhost:5000/predict";
    [SerializeField] private float captureInterval;


    private WebCamTexture webCamTexture;
    private Texture2D captureTexture;

    private float timeSinceLastCapture = 0f;

    [Serializable]
    private class GestureResponse
    {
        public string gesture;
        public float confidence;
        public string error;
        public float fps;
    }

    [Serializable]
    private class GestureRequest
    {
        public string image;
    }

    [Header("Public Variables")]
    public string currentGesture = "none";
    public float currentFps = 0.0f;

    void Start()
    {
        StartCamera();
    }

    void StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("No camera detected");
            return;
        }

        // Choose camera (front or back)
        string deviceName = "";
        foreach (WebCamDevice device in devices)
        {
            if (useFrontCamera && device.isFrontFacing)
            {
                deviceName = device.name;
                break;
            }
            else if (!useFrontCamera && !device.isFrontFacing)
            {
                deviceName = device.name;
                break;
            }
        }

        // If no matching camera found, use the first one
        if (string.IsNullOrEmpty(deviceName) && devices.Length > 0)
        {
            deviceName = devices[0].name;
        }

        // Create webcam texture - let it use its native resolution first
        webCamTexture = new WebCamTexture(deviceName);
        webCamTexture.Play();

        // Wait for the webcam to initialize
        StartCoroutine(WaitForWebcamInitialization());
    }

    private IEnumerator WaitForWebcamInitialization()
    {
        // Wait for webcam to be ready
        while (webCamTexture.width <= 16 || webCamTexture.height <= 16)
        {
            yield return null;
        }
        // 1920x1080
        // Debug.Log($"Webcam initialized: {webCamTexture.width}x{webCamTexture.height}");

        // Create new webcam texture with adjusted resolution
        webCamTexture.Stop();
        webCamTexture = new WebCamTexture(webCamTexture.deviceName, targetWidth, targetHeight, targetFrameRate);
        webCamTexture.Play();

        // Wait for it to initialize again
        while (webCamTexture.width <= 16 || webCamTexture.height <= 16)
        {
            yield return null;
        }

        // Set up display
        if (cameraDisplay != null)
        {
            cameraDisplay.texture = webCamTexture;
            cameraDisplay.rectTransform.localScale = new Vector3(-0.1125f, 0.1125f, 0.1125f);

        }

        // Create texture for capturing frames with fixed 4:3 aspect ratio
        captureTexture = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);

        Debug.Log($"Webcam initialized: {webCamTexture.width}x{webCamTexture.height}");
        Debug.Log($"Capture texture fixed at: {captureWidth}x{captureHeight}");
    }

    private Color32[] CropWebcamTo4x3(Color32[] pixels, int webcamWidth, int webcamHeight)
    {
        float webcamAspect = (float)webcamWidth / webcamHeight;
        float targetAspect = (float)captureWidth / captureHeight; // 4:3

        int cropX = 0, cropY = 0;
        int cropWidth = webcamWidth;
        int cropHeight = webcamHeight;

        if (webcamAspect > targetAspect)
        {
            // Webcam is wider than 4:3, crop width
            cropWidth = Mathf.RoundToInt(webcamHeight * targetAspect);
            cropX = (webcamWidth - cropWidth) / 2;
        }
        else if (webcamAspect < targetAspect)
        {
            // Webcam is taller than 4:3, crop height
            cropHeight = Mathf.RoundToInt(webcamWidth / targetAspect);
            cropY = (webcamHeight - cropHeight) / 2;
        }

        // Create array for cropped pixels
        Color32[] croppedPixels = new Color32[captureWidth * captureHeight];

        // Sample from cropped area and resize to capture dimensions
        for (int y = 0; y < captureHeight; y++)
        {
            for (int x = 0; x < captureWidth; x++)
            {
                // Map from capture coordinates to cropped webcam coordinates
                int sourceX = cropX + (x * cropWidth) / captureWidth;
                int sourceY = cropY + (y * cropHeight) / captureHeight;

                // Clamp to ensure we don't go out of bounds
                sourceX = Mathf.Clamp(sourceX, 0, webcamWidth - 1);
                sourceY = Mathf.Clamp(sourceY, 0, webcamHeight - 1);

                // Unity textures are bottom-left origin, so we need to flip Y
                int sourceIndex = sourceY * webcamWidth + sourceX;
                int targetIndex = y * captureWidth + x;

                croppedPixels[targetIndex] = pixels[sourceIndex];
            }
        }

        return croppedPixels;
    }

    private bool isRequesting = false;

    void Update()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying) return;

        timeSinceLastCapture += Time.deltaTime;
        if (timeSinceLastCapture >= captureInterval && !isRequesting)
        {
            timeSinceLastCapture = 0f;
            StartCoroutine(CaptureAndRecognize());
        }
    }

    IEnumerator CaptureAndRecognize()
    {
        if (isRequesting) yield break;
        isRequesting = true;

        // Don't try to capture if texture isn't ready
        if (webCamTexture.width <= 16 || webCamTexture.height <= 16)
        {
            yield break;
        }

        // Capture texture is always fixed at 1280x960, no need to resize
        if (captureTexture == null)
        {
            captureTexture = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
        }

        string jsonData = "";

        // WebGL-specific fix: ensure texture is readable
        try
        {
            // Capture frame from webcam
            Color32[] webcamPixels = webCamTexture.GetPixels32();

            // Crop and resize to 4:3 aspect ratio (1280x960)
            Color32[] croppedPixels = CropWebcamTo4x3(webcamPixels, webCamTexture.width, webCamTexture.height);

            // Apply cropped pixels to capture texture
            captureTexture.SetPixels32(croppedPixels);
            captureTexture.Apply();

            // Convert texture to jpg with higher quality for WebGL
            byte[] jpgData = captureTexture.EncodeToJPG(50);
            string base64Image = Convert.ToBase64String(jpgData);

            // Create request data
            GestureRequest requestData = new GestureRequest { image = base64Image };
            jsonData = JsonUtility.ToJson(requestData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error capturing image: {e.Message}");
            yield break;
        }

        // Send to server (outside try block to avoid yield in try-catch)
        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // WebGL-specific timeout settings
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    GestureResponse response = JsonUtility.FromJson<GestureResponse>(request.downloadHandler.text);

                    if (response.error == null)
                    {
                        // Update gesture
                        currentGesture = response.gesture;
                        currentFps = response.fps;
                        // OnGestureDetected(currentGesture);

                        if (currentGesture == "stop")
                        {
                            currentGesture = "grab";
                        }
                        else if (currentGesture == "no")
                        {
                            currentGesture = "stop";
                        }

                        gestureText.text = currentGesture;
                        fpsText.text = Mathf.Round(currentFps).ToString();
                    }
                    // else
                    // {
                    // Debug.LogError($"Server error: {response.error}");
                    // }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error processing server response: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Request error: {request.error}");
            }
        }
        isRequesting = false;
    }

    void OnDestroy()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }

        if (captureTexture != null)
        {
            DestroyImmediate(captureTexture);
        }
    }

    // Public method to get current gesture
    public string GetCurrentGesture()
    {
        return currentGesture;
    }
}