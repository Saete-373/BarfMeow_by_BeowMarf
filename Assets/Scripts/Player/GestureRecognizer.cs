using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NativeWebSocket;

public class GestureRecognizer : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private bool useFrontCamera = true;
    [SerializeField] private int targetWidth = 640;
    [SerializeField] private int targetHeight = 480;
    [SerializeField] private int targetFrameRate = 30;

    [Header("Capture Settings")]
    [SerializeField] private int captureWidth = 640;
    [SerializeField] private int captureHeight = 480;

    [Header("Display Settings")]
    public GameplayManager gpManager;
    [SerializeField] private RawImage cameraDisplay;
    [SerializeField] private TMP_Text gestureText;
    [SerializeField] private TMP_Text fpsText;

    [Header("Server Settings")]
    [SerializeField] private string serverUrl = "ws://localhost:8000/ws"; // WebSocket endpoint
    [SerializeField] private float captureInterval = 0.1f;

    private WebCamTexture webCamTexture;
    private Texture2D captureTexture;

    private float timeSinceLastCapture = 0f;
    private WebSocket websocket;
    private bool isRequesting = false;

    [Serializable]
    private class GestureResponse
    {
        public string gesture;
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

    private void Awake()
    {
        gpManager = GetComponent<PlayerController>().gameplayManager;
        cameraDisplay = gpManager.cameraDisplay;
        gestureText = gpManager.gestureText;
        fpsText = gpManager.fpsText;
    }

    async void Start()
    {
        StartCamera();
        await ConnectWebSocket();
    }

    void StartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("No camera detected");
            return;
        }

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

        if (string.IsNullOrEmpty(deviceName))
            deviceName = devices[0].name;

        webCamTexture = new WebCamTexture(deviceName, targetWidth, targetHeight, targetFrameRate);
        webCamTexture.Play();

        StartCoroutine(WaitForWebcamInitialization());
    }

    private IEnumerator WaitForWebcamInitialization()
    {
        while (webCamTexture.width <= 16 || webCamTexture.height <= 16)
            yield return null;

        if (cameraDisplay != null)
        {
            cameraDisplay.texture = webCamTexture;
            cameraDisplay.rectTransform.localScale = new Vector3(-0.1125f, 0.1125f, 0.1125f);
        }

        captureTexture = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
    }

    private Color32[] CropWebcamTo4x3(Color32[] pixels, int webcamWidth, int webcamHeight)
    {
        float webcamAspect = (float)webcamWidth / webcamHeight;
        float targetAspect = (float)captureWidth / captureHeight;

        int cropX = 0, cropY = 0, cropWidth = webcamWidth, cropHeight = webcamHeight;

        if (webcamAspect > targetAspect)
        {
            cropWidth = Mathf.RoundToInt(webcamHeight * targetAspect);
            cropX = (webcamWidth - cropWidth) / 2;
        }
        else if (webcamAspect < targetAspect)
        {
            cropHeight = Mathf.RoundToInt(webcamWidth / targetAspect);
            cropY = (webcamHeight - cropHeight) / 2;
        }

        Color32[] croppedPixels = new Color32[captureWidth * captureHeight];
        for (int y = 0; y < captureHeight; y++)
        {
            for (int x = 0; x < captureWidth; x++)
            {
                int sourceX = cropX + (x * cropWidth) / captureWidth;
                int sourceY = cropY + (y * cropHeight) / captureHeight;
                sourceX = Mathf.Clamp(sourceX, 0, webcamWidth - 1);
                sourceY = Mathf.Clamp(sourceY, 0, webcamHeight - 1);
                int sourceIndex = sourceY * webcamWidth + sourceX;
                int targetIndex = y * captureWidth + x;
                croppedPixels[targetIndex] = pixels[sourceIndex];
            }
        }
        return croppedPixels;
    }

    private void CropAndResizeWebcam()
    {
        // สร้าง RenderTexture ขนาด target
        RenderTexture rt = RenderTexture.GetTemporary(captureWidth, captureHeight, 0);

        // ใช้ Graphics.Blit() crop & resize webcamTexture ลง RenderTexture
        Graphics.Blit(webCamTexture, rt);

        // อ่าน pixels กลับมาเป็น Texture2D
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        captureTexture.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        captureTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
    }

    async System.Threading.Tasks.Task ConnectWebSocket()
    {
        websocket = new WebSocket(serverUrl);

        // websocket.OnOpen += () => Debug.Log("WebSocket connected");
        // websocket.OnError += (e) => Debug.LogError("WebSocket error: " + e);
        // websocket.OnClose += (e) => Debug.Log("WebSocket closed: " + e);
        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            try
            {
                GestureResponse response = JsonUtility.FromJson<GestureResponse>(message);
                currentGesture = response.gesture;
                currentFps = response.fps;

                if (currentGesture == "no") currentGesture = "stop";

                if (currentGesture == "transition") return;

                if (gpManager != null)
                {
                    gpManager.gestureText.text = currentGesture;
                    gpManager.fpsText.text = Mathf.Round(currentFps).ToString();
                }

                // Debug.Log($"Gesture: {currentGesture}");
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to parse WS message: " + ex.Message);
            }
        };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif

        if (webCamTexture == null || !webCamTexture.isPlaying) return;

        timeSinceLastCapture += Time.deltaTime;
        if (timeSinceLastCapture >= captureInterval && !isRequesting)
        {
            timeSinceLastCapture = 0f;
            StartCoroutine(CaptureAndSendWebSocket());
        }
    }

    IEnumerator CaptureAndSendWebSocket()
    {
        if (isRequesting) yield break;
        isRequesting = true;

        if (webCamTexture.width <= 16 || webCamTexture.height <= 16)
        {
            isRequesting = false;
            yield break;
        }

        // Color32[] webcamPixels = webCamTexture.GetPixels32();
        // Color32[] croppedPixels = CropWebcamTo4x3(webcamPixels, webCamTexture.width, webCamTexture.height);
        // captureTexture.SetPixels32(croppedPixels);
        // captureTexture.Apply();
        CropAndResizeWebcam();

        byte[] pngData = captureTexture.EncodeToPNG();

        if (websocket.State == WebSocketState.Open)
        {
            websocket.Send(pngData);
        }

        isRequesting = false;
        yield return null;

    }

    private void OnDestroy()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
        if (captureTexture != null)
        {
            DestroyImmediate(captureTexture);
        }
        if (websocket != null)
        {
            websocket.Close();
        }
    }

    public string GetCurrentGesture()
    {
        return currentGesture;
    }
}
