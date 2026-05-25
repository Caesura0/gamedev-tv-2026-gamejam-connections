/// EndingDialogueUI.cs
/// ─────────────────────────────────────────────────────────────────────────
/// OPTIONAL convenience script — skip this if you're wiring up the Canvas
/// manually in the Editor (which is the recommended approach for a cutscene).
///
/// If you'd rather let code build the UI, attach this MonoBehaviour to an
/// empty GameObject. It creates the Canvas + Panel + Text at runtime and
/// then hands references to EndingDialogueManager.
///
/// Usage:
///   1. Add EndingDialogueUI to a GameObject.
///   2. Add EndingDialogueManager to the same (or another) GameObject.
///   3. Assign your DialogueLine[] to EndingDialogueManager.dialogueLines.
///   4. Leave dialoguePanel / dialogueText empty — this script fills them.
/// ─────────────────────────────────────────────────────────────────────────

using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class EndingDialogueUI : MonoBehaviour
{
    [Header("Panel Style")]
    public Color panelColor = new Color(0f, 0f, 0f, 0.75f);
    public Color textColor = Color.white;
    public int fontSize = 28;
    [Range(0f, 1f)] public float panelWidthFraction = 0.8f;
    [Range(0f, 1f)] public float panelHeightFraction = 0.25f;

    private void Awake()
    {
        var manager = GetComponent<EndingCutsceneManager>();


        var canvasGO = new GameObject("DialogueCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();


        var panelGO = new GameObject("DialoguePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);

        var panelImage = panelGO.AddComponent<Image>();
        panelImage.color = panelColor;

        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f - panelWidthFraction / 2f, 0.04f);
        panelRect.anchorMax = new Vector2(0.5f + panelWidthFraction / 2f, 0.04f + panelHeightFraction);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var cg = panelGO.AddComponent<CanvasGroup>();


        var textGO = new GameObject("DialogueText");
        textGO.transform.SetParent(panelGO.transform, false);

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.color = textColor;
        tmp.fontSize = fontSize;
        tmp.margin = new Vector4(20, 16, 20, 16);
        tmp.textWrappingMode = TextWrappingModes.Normal;

        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;


        //manager.dialoguePanel = cg;
        //manager.dialogueText = tmp;
    }
}