using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CutSceneHandler : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string fileName = "cutscene.mp4"; // put this in Assets/StreamingAssets/
    [SerializeField] private Button skipButton;
    [SerializeField] private GameObject staticBackground;

    private void Start()
    {
        staticBackground.SetActive(false); 
        videoPlayer.errorReceived += OnVideoError;
        StartCoroutine(PlayCutScene());
        //Time.timeScale = 0f; // pause the game while the cutscene plays
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnCutsceneEnded;
        
    }

    private string BuildStreamingAssetsUrl(string file)
    {
        var path = Path.Combine(Application.streamingAssetsPath, file);
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL already needs a regular HTTP(S) URL; Unity provides it here.
        return path; // e.g., https://<host>/Build/StreamingAssets/cutscene.mp4
#else
        // Editor/Windows: convert local path to file:// URL
        Debug.Log(videoPlayer.url);
        return new Uri(path).AbsoluteUri;
#endif

    }

    private IEnumerator PlayCutScene()
    {
        skipButton.gameObject.SetActive(true);
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = BuildStreamingAssetsUrl(fileName);
        Debug.Log(videoPlayer.url);
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // optional: keep muted

        videoPlayer.Prepare();
        Debug.Log("Preparing...");
        while (!videoPlayer.isPrepared) yield return null;
        Debug.Log("Prepared!");
        videoPlayer.Play();
        yield return new WaitUntil(() => !videoPlayer.isPlaying);

        SkipVideo();
        //AudioManager.Instance.PlayGameplayMusic();
        Time.timeScale = 1f;
    }

    private void OnCutsceneEnded(VideoPlayer vp)
    {
        vp.loopPointReached -= OnCutsceneEnded; // clean up
        vp.Stop();
        staticBackground.SetActive(true); // show static background after cutscene
        //AudioManager.Instance.PlayGameplayMusic();
        Time.timeScale = 1f;
        // optionally hide the video overlay:
        // vp.targetCameraAlpha = 0f;
    }

    public void SkipVideo()
    {
        skipButton.gameObject.SetActive(false);
        videoPlayer.Stop();
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"Video Error: {message}");
    }
}
