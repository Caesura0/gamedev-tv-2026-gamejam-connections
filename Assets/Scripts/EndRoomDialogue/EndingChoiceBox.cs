using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndingChoiceBox : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup choicePanel;
    public Button[] optionButtons = new Button[3];
    public TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[3];

    [Header("Labels")]
    public string[] optionLabels = { "Run", "Freeze", "Approach" };

    [Header("Style")]
    public Color highlightColor = new Color(1f, 0.92f, 0.4f);
    public Color normalColor = Color.white;

    [Header("Fade")]
    public float fadeInDuration = 0.5f;

    private int selected = 0;
    private bool active = false;
    private bool navHeld = false;
    private Action<int> callback;

    private void Awake()
    {
        // Start fully hidden
        if (choicePanel != null)
        {
            choicePanel.alpha = 0f;
            choicePanel.interactable = false;
            choicePanel.blocksRaycasts = false;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int captured = i;
            optionButtons[i].onClick.AddListener(() => Confirm(captured));
            AddHoverTrigger(optionButtons[i], captured);

            if (optionTexts[i] != null)
                optionTexts[i].textWrappingMode = TextWrappingModes.Normal;
        }
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnInteractPressed += OnInteractPressed;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnInteractPressed -= OnInteractPressed;
    }

    private void Update()
    {
        if (!active || InputManager.Instance == null) return;

        float vertical = InputManager.Instance.Movement.y;

        if (vertical > 0.5f && !navHeld)
        {
            navHeld = true;
            SetSelected((selected - 1 + optionButtons.Length) % optionButtons.Length);
        }
        else if (vertical < -0.5f && !navHeld)
        {
            navHeld = true;
            SetSelected((selected + 1) % optionButtons.Length);
        }
        else if (Mathf.Abs(vertical) < 0.2f)
        {
            navHeld = false;
        }
    }

    public void Show(Action<int> onChosen)
    {
        callback = onChosen;
        selected = 0;
        navHeld = false;

        for (int i = 0; i < optionTexts.Length; i++)
            if (optionTexts[i] != null)
                optionTexts[i].text = optionLabels[i];

        RefreshHighlight();
        StartCoroutine(FadeIn());
    }

    public void Hide()
    {
        active = false;
        if (choicePanel != null)
        {
            choicePanel.alpha = 0f;
            choicePanel.interactable = false;
            choicePanel.blocksRaycasts = false;
        }
    }

    private IEnumerator FadeIn()
    {
        choicePanel.interactable = false;
        choicePanel.blocksRaycasts = false;
        choicePanel.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            choicePanel.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        choicePanel.alpha = 1f;
        choicePanel.interactable = true;
        choicePanel.blocksRaycasts = true;
        active = true; // only accept input once fully visible
    }

    private void OnInteractPressed()
    {
        if (!active) return;
        Confirm(selected);
    }

    private void SetSelected(int index)
    {
        selected = index;
        RefreshHighlight();
    }

    private void Confirm(int index)
    {
        if (!active) return;
        Hide();
        callback?.Invoke(index);
    }

    private void RefreshHighlight()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool sel = i == selected;
            if (optionTexts[i] != null)
                optionTexts[i].color = sel ? highlightColor : normalColor;
            optionButtons[i].transform.localScale = sel ? Vector3.one * 1.06f : Vector3.one;
        }
    }

    private void AddHoverTrigger(Button btn, int index)
    {
        var trigger = btn.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>()
                   ?? btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var entry = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
        };
        entry.callback.AddListener(_ => SetSelected(index));
        trigger.triggers.Add(entry);
    }
}