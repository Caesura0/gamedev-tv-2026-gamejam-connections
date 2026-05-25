var VideoOverlayPlugin = {

    // ── state ──────────────────────────────────────────────
    $videoState: {
        element: null,
        finished: false,
        callbackObj: null,
        callbackMethod: null
    },

    // ── Create & play ─────────────────────────────────────
    VideoOverlay_Create: function (urlPtr, gameObjectNamePtr, callbackMethodPtr) {

        var url            = UTF8ToString(urlPtr);
        var gameObjectName = UTF8ToString(gameObjectNamePtr);
        var callbackMethod = UTF8ToString(callbackMethodPtr);

        // Clean up any previous instance
        if (videoState.element) {
            videoState.element.remove();
            videoState.element = null;
        }

        videoState.finished       = false;
        videoState.callbackObj    = gameObjectName;
        videoState.callbackMethod = callbackMethod;

        // Grab the Unity canvas so we can position our overlay on top of it
        var canvas = document.querySelector("#unity-canvas") || document.querySelector("canvas");

        var video  = document.createElement("video");
        video.src  = url;
        video.autoplay = true;
        video.muted    = true;          // required for browser autoplay policy
        video.playsInline = true;
        video.setAttribute("playsinline", "");

        // Style: full-screen overlay directly on top of the canvas
        video.style.position   = "absolute";
        video.style.top        = "0";
        video.style.left       = "0";
        video.style.width      = "100%";
        video.style.height     = "100%";
        video.style.objectFit  = "cover";
        video.style.zIndex     = "1000";         // above the Unity canvas
        video.style.background = "black";
        video.style.pointerEvents = "none";      // let clicks through to Unity (skip button)

        // When the video ends naturally
        video.addEventListener("ended", function () {
            videoState.finished = true;
            video.remove();
            videoState.element = null;

            // Notify Unity via SendMessage
            if (videoState.callbackObj && videoState.callbackMethod) {
                SendMessage(videoState.callbackObj, videoState.callbackMethod);
            }
        });

        // Handle errors gracefully
        video.addEventListener("error", function (e) {
            console.error("[VideoOverlay] Playback error:", e);
            videoState.finished = true;
            video.remove();
            videoState.element = null;

            if (videoState.callbackObj && videoState.callbackMethod) {
                SendMessage(videoState.callbackObj, videoState.callbackMethod);
            }
        });

        // Insert into the page
        var container = canvas ? canvas.parentElement : document.body;
        if (container.style.position === "" || container.style.position === "static") {
            container.style.position = "relative";
        }
        container.appendChild(video);
        videoState.element = video;

        // Start playback (returns a promise)
        var playPromise = video.play();
        if (playPromise !== undefined) {
            playPromise.catch(function (err) {
                console.warn("[VideoOverlay] Autoplay blocked:", err);
                // Even if autoplay is blocked, the video element stays up.
                // The user will see a black overlay and can still skip.
            });
        }
    },

    // ── Skip / destroy ────────────────────────────────────
    VideoOverlay_Skip: function () {
        if (videoState.element) {
            videoState.element.pause();
            videoState.element.remove();
            videoState.element = null;
        }
        videoState.finished = true;
    },

    // ── Poll: has the video ended? ────────────────────────
    VideoOverlay_IsFinished: function () {
        return videoState.finished ? 1 : 0;
    }
};

autoAddDeps(VideoOverlayPlugin, '$videoState');
mergeInto(LibraryManager.library, VideoOverlayPlugin);
