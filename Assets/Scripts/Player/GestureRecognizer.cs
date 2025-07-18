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
    [SerializeField] private int targetWidth = 72;
    [SerializeField] private int targetHeight = 54;
    [SerializeField] private int targetFrameRate = 30;

    [Header("Display Settings")]
    [SerializeField] private RawImage cameraDisplay;
    [SerializeField] private TMP_Text gestureText;
    [SerializeField] private TMP_Text fpsText;

    [Header("Server Settings")]
    [SerializeField] private string serverUrl = "http://localhost:5000/predict";
    [SerializeField] private float captureInterval = 0.05f;

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

        // Create webcam texture
        webCamTexture = new WebCamTexture(deviceName, targetWidth, targetHeight, targetFrameRate);
        webCamTexture.Play();

        // Set up display
        if (cameraDisplay != null)
        {
            cameraDisplay.texture = webCamTexture;
        }

        // Create texture for capturing frames
        captureTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
    }

    void Update()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying)
            return;

        // In the Update method:
        if (cameraDisplay != null)
        {
            cameraDisplay.rectTransform.localEulerAngles = new Vector3(0, 180, 0);

            // Use actual webcam dimensions, not target dimensions
            cameraDisplay.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
        }

        // Update gesture text
        if (gestureText != null)
        {

            gestureText.text = currentGesture;
        }

        if (fpsText != null)
        {
            fpsText.text = currentFps.ToString("F0");
        }

        // Capture frames at regular intervals
        timeSinceLastCapture += Time.deltaTime;
        if (timeSinceLastCapture >= captureInterval)
        {
            timeSinceLastCapture = 0f;
            StartCoroutine(CaptureAndRecognize());
        }
    }

    IEnumerator CaptureAndRecognize()
    {
        // Don't try to capture if texture isn't ready
        if (webCamTexture.width <= 16 || webCamTexture.height <= 16)
        {
            yield break;
        }

        // Check if we need to resize the capture texture
        if (captureTexture.width != webCamTexture.width || captureTexture.height != webCamTexture.height)
        {
            // Recreate the capture texture with the correct dimensions
            captureTexture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
            // Debug.Log($"Resized capture texture to match webcam: {webCamTexture.width}x{webCamTexture.height}");
        }

        // Capture frame from webcam
        Color32[] pixels = webCamTexture.GetPixels32();
        captureTexture.SetPixels32(pixels);
        captureTexture.Apply();

        // Convert texture to jpg
        byte[] jpgData = captureTexture.EncodeToJPG(50);
        string base64Image = Convert.ToBase64String(jpgData);

        // Create request data
        GestureRequest requestData = new GestureRequest { image = base64Image };
        string jsonData = JsonUtility.ToJson(requestData);

        // Send to server
        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            try
            {
                if (request.result == UnityWebRequest.Result.Success)
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
                    }
                    // else
                    // {
                    // Debug.LogError($"Server error: {response.error}");
                    // }
                }
                else
                {
                    Debug.LogError($"Request error: {request.error}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error capturing or sending image: {e.Message}");
            }
        }

    }

    // void OnGestureDetected(string gesture)
    // {
    //     // Debug.Log($"Detected gesture: {gesture}");

    //     // You can trigger game events based on detected gestures here
    //     // For example:
    //     switch (gesture)
    //     {
    //         case "stop":
    //             // Handle stop gesture
    //             break;

    //         case "back":
    //             // Handle back gesture
    //             break;

    //             // Add more gestures as needed
    //     }
    // }

    void OnDestroy()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }

    // Public method to get current gesture
    public string GetCurrentGesture()
    {
        return currentGesture;
    }
}