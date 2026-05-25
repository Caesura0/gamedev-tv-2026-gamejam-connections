using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CutSceneVideoHandler : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string fileName = "cutscene.mp4"; // put this in Assets/StreamingAssets/
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject staticBackground;
    [SerializeField] private float prepareTimeout = 10f;

    [Header("Render Texture Setup (Editor / URP Compatibility)")]
    [Tooltip("Optional: Assign a UI RawImage here. If left null, one will be created dynamically at runtime.")]
    [SerializeField] private RawImage videoRenderImage;

    private Coroutine cutsceneCoroutine;
    private RenderTexture tempRenderTexture;
    private RawImage dynamicRawImage;
    private bool videoStarted;
    private bool videoFinished;

    // ─────────────────────────────────────────────────────────
    //  WebGL JavaScript plugin imports
    // ─────────────────────────────────────────────────────────
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void VideoOverlay_Create(string url, string gameObjectName, string callbackMethod);
    [DllImport("__Internal")] private static extern void VideoOverlay_Skip();
    [DllImport("__Internal")] private static extern int  VideoOverlay_IsFinished();
#endif

    // ─────────────────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────────────────
    private void Start()
    {
        if (staticBackground != null)
            staticBackground.SetActive(false);

        cutsceneCoroutine = StartCoroutine(PlayCutScene());
    }

    // ─────────────────────────────────────────────────────────
    //  Main coroutine — routes to WebGL or Editor path
    // ─────────────────────────────────────────────────────────
    private IEnumerator PlayCutScene()
    {
        if (skipButton != null)
            skipButton.gameObject.SetActive(true);

#if UNITY_WEBGL && !UNITY_EDITOR
        yield return StartCoroutine(PlayCutScene_WebGL());
#else
        yield return StartCoroutine(PlayCutScene_Editor());
#endif

        EndCutscene();
    }

    // ═════════════════════════════════════════════════════════
    //  PATH A — WebGL: native HTML5 <video> overlay
    // ═════════════════════════════════════════════════════════
    private IEnumerator PlayCutScene_WebGL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string url = Path.Combine(Application.streamingAssetsPath, fileName);
        Debug.Log($"[CutScene] WebGL: Playing video via HTML5 overlay: {url}");

        VideoOverlay_Create(url, gameObject.name, "OnWebGLVideoEnded");

        // Wait until the JS side reports the video has finished (or was skipped)
        while (VideoOverlay_IsFinished() == 0)
        {
            yield return null;
        }

        Debug.Log("[CutScene] WebGL: Video finished.");
#else
        yield break;
#endif
    }

    /// <summary>
    /// Called from JavaScript via SendMessage when the HTML5 video ends.
    /// </summary>
    public void OnWebGLVideoEnded()
    {
        Debug.Log("[CutScene] WebGL: OnWebGLVideoEnded callback received.");
        // The coroutine poll loop will pick up IsFinished() == 1 on the next frame.
    }

    // ═════════════════════════════════════════════════════════
    //  PATH B — Editor / Standalone: Unity VideoPlayer
    //           (with all the WMF workarounds)
    // ═════════════════════════════════════════════════════════
    private IEnumerator PlayCutScene_Editor()
    {
        // Wait briefly to let the scene fully load and avoid initialization spikes
        yield return new WaitForSecondsRealtime(0.2f);

        videoStarted = false;
        videoFinished = false;

        // Configure VideoPlayer defensively
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.isLooping = false;
        videoPlayer.waitForFirstFrame = false;
        videoPlayer.playOnAwake = false;

        // ---------- URP / Render Texture setup ----------
        RawImage activeRawImage = videoRenderImage;
        if (activeRawImage == null)
        {
#if UNITY_2023_1_OR_NEWER
            Canvas canvas = FindAnyObjectByType<Canvas>();
#else
            Canvas canvas = FindObjectOfType<Canvas>();
#endif
            if (canvas != null)
            {
                GameObject go = new GameObject("TempVideoRenderImage");
                go.transform.SetParent(canvas.transform, false);
                go.transform.SetAsFirstSibling();

                activeRawImage = go.AddComponent<RawImage>();
                dynamicRawImage = activeRawImage;

                RectTransform rect = activeRawImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
            }
        }

        if (activeRawImage != null)
        {
            tempRenderTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.ARGB32);
            tempRenderTexture.Create();

            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = tempRenderTexture;

            activeRawImage.texture = tempRenderTexture;
            activeRawImage.gameObject.SetActive(true);
        }
        else
        {
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
        }
        // ------------------------------------------------

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Path.GetFullPath(
            Path.Combine(Application.streamingAssetsPath, fileName));
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        videoPlayer.started += OnVideoStarted;
        videoPlayer.loopPointReached += OnVideoFinished;

        Debug.Log($"[CutScene] Editor: Preparing video: {videoPlayer.url}");
        videoPlayer.Prepare();

        // Wait for prepare (with timeout)
        float elapsed = 0f;
        while (!videoPlayer.isPrepared)
        {
            elapsed += Time.unscaledDeltaTime;
            if (elapsed > prepareTimeout)
            {
                Debug.LogError("[CutScene] Video prepare timed out.");
                yield break;
            }
            yield return null;
        }

        Debug.Log("[CutScene] Video prepared. Playing...");

        // Explicitly disable audio tracks to prevent WMF audio-sync hangs
        for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
        {
            videoPlayer.EnableAudioTrack(i, false);
        }

        videoPlayer.Play();

        // Wait for the started event
        elapsed = 0f;
        while (!videoStarted)
        {
            elapsed += Time.unscaledDeltaTime;
            if (elapsed > 5f)
            {
                Debug.LogError("[CutScene] Video failed to start.");
                yield break;
            }
            yield return null;
        }

        // Self-healing: if stuck at frame -1, pause/play to kickstart decoder
        elapsed = 0f;
        while (videoPlayer.frame == -1 && elapsed < 2.0f)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (videoPlayer.frame == -1)
        {
            Debug.LogWarning("[CutScene] Video decoder stalled at frame -1. Kickstarting...");
            videoPlayer.Pause();
            yield return new WaitForSecondsRealtime(0.1f);
            videoPlayer.Play();

            elapsed = 0f;
            while (videoPlayer.frame == -1 && elapsed < 1.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        Debug.Log($"[CutScene] Video playing. Frame: {videoPlayer.frame}, Length: {videoPlayer.frameCount}");

        // Wait for video to finish
        yield return new WaitUntil(() => videoFinished);

        Debug.Log("[CutScene] Video playback finished.");
    }

    // ─────────────────────────────────────────────────────────
    //  VideoPlayer event handlers (Editor path only)
    // ─────────────────────────────────────────────────────────
    private void OnVideoStarted(VideoPlayer vp)
    {
        videoStarted = true;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        videoFinished = true;
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"[CutScene] Video Error: {message}");
        videoFinished = true; // let the coroutine exit
    }

    // ─────────────────────────────────────────────────────────
    //  Shared: cleanup
    // ─────────────────────────────────────────────────────────
    private void EndCutscene()
    {
        cutsceneCoroutine = null;

        // Editor cleanup
        if (videoPlayer != null)
        {
            videoPlayer.started -= OnVideoStarted;
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.Stop();
            videoPlayer.targetTexture = null;
        }

        if (tempRenderTexture != null)
        {
            tempRenderTexture.Release();
            Destroy(tempRenderTexture);
            tempRenderTexture = null;
        }

        if (dynamicRawImage != null)
        {
            Destroy(dynamicRawImage.gameObject);
            dynamicRawImage = null;
        }
        else if (videoRenderImage != null)
        {
            videoRenderImage.gameObject.SetActive(false);
        }

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        if (staticBackground != null)
            staticBackground.SetActive(true);

        Time.timeScale = 1f;
    }

    // ─────────────────────────────────────────────────────────
    //  Shared: skip button
    // ─────────────────────────────────────────────────────────
    public void SkipVideo()
    {
        if (cutsceneCoroutine != null)
        {
            StopCoroutine(cutsceneCoroutine);
            cutsceneCoroutine = null;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        VideoOverlay_Skip();
#endif

        EndCutscene();
    }

    // ─────────────────────────────────────────────────────────
    //  Cleanup on destroy
    // ─────────────────────────────────────────────────────────
    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.started -= OnVideoStarted;
            videoPlayer.loopPointReached -= OnVideoFinished;
        }

        if (tempRenderTexture != null)
        {
            tempRenderTexture.Release();
            Destroy(tempRenderTexture);
        }

        if (dynamicRawImage != null)
        {
            Destroy(dynamicRawImage.gameObject);
        }
    }
}