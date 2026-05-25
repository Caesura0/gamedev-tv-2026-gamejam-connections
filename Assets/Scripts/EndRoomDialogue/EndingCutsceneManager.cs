using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndingCutsceneManager : MonoBehaviour
{
    [Header("Content")]
    public Sprite[] cutsceneImages;

    [Header("Outcome Slides")]
    [Tooltip("Shown after the player picks Run.")]
    public Sprite runSlide;
    [Tooltip("Shown after the player picks Freeze.")]
    public Sprite freezeSlide;
    [Tooltip("Shown after the player picks Approach.")]
    public Sprite approachSlide;

    [Header("UI References")]
    [Tooltip("The two Image components used to cross-fade. Stack them on top of each other, both stretched fullscreen.")]
    public Image imageA;
    public Image imageB;

    [Header("Settings")]
    public float fadeDuration = 0.6f;
    [Tooltip("Seconds after a fade starts before interact is accepted again.")]
    public float inputLockDuration = 0.2f;

    [Header("Choice")]
    [Tooltip("If true, skips the choice box and outcome slides entirely and loads nextScene after the last image.")]
    public bool skipChoice = false;
    public SceneEnum nextScene;
    public EndingChoiceBox endingChoiceBox;

    private int currentIndex = 0;
    private bool isFading = false;
    private bool inputLocked = false;
    private bool choiceShown = false;

    // After the outcome slide is shown, one interact loads the scene
    private bool waitingForFinalInteract = false;
    private SceneEnum pendingScene;

    private bool aIsTop = true;

    private Image Top => aIsTop ? imageA : imageB;
    private Image Bottom => aIsTop ? imageB : imageA;

    private void Start()
    {
        imageA.sprite = cutsceneImages[0];
        imageA.color = Color.white;
        imageB.color = new Color(1f, 1f, 1f, 0f);
        aIsTop = true;
        currentIndex = 0;

        if (cutsceneImages.Length == 1)
        {
            if (skipChoice) { waitingForFinalInteract = true; pendingScene = nextScene; }
            else ShowChoiceBox();
        }

        if (InputManager.Instance != null)
            InputManager.Instance.OnInteractPressed += OnInteract;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnInteractPressed -= OnInteract;
    }

    private void OnInteract()
    {
        if (isFading || inputLocked) return;

        if (waitingForFinalInteract)
        {
            Loader.Load(pendingScene);
            return;
        }

        if (choiceShown) return;

        int nextIndex = currentIndex + 1;
        StartCoroutine(CrossFadeTo(nextIndex));
    }

    private IEnumerator CrossFadeTo(int index)
    {
        isFading = true;
        inputLocked = true;

        bool isLastImage = index >= cutsceneImages.Length - 1;
        int clampedIndex = Mathf.Min(index, cutsceneImages.Length - 1);

        Bottom.sprite = cutsceneImages[clampedIndex];
        Bottom.color = new Color(1f, 1f, 1f, 0f);

        aIsTop = !aIsTop;

        Image fadingOut = aIsTop ? imageB : imageA;
        Image fadingIn = aIsTop ? imageA : imageB;

        fadingIn.transform.SetAsLastSibling();

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            fadingIn.color = new Color(1f, 1f, 1f, t);
            fadingOut.color = new Color(1f, 1f, 1f, 1f - t);

            yield return null;
        }

        fadingIn.color = Color.white;
        fadingOut.color = new Color(1f, 1f, 1f, 0f);

        currentIndex = clampedIndex;
        isFading = false;

        if (isLastImage)
        {
            if (skipChoice)
            {
                waitingForFinalInteract = true;
                pendingScene = nextScene;
            }
            else
            {
                ShowChoiceBox();
            }
            yield break;
        }

        yield return new WaitForSeconds(inputLockDuration);
        inputLocked = false;
    }

    private IEnumerator CrossFadeToOutcome(Sprite outcomeSprite)
    {
        isFading = true;
        inputLocked = true;

        Bottom.sprite = outcomeSprite;
        Bottom.color = new Color(1f, 1f, 1f, 0f);

        aIsTop = !aIsTop;

        Image fadingOut = aIsTop ? imageB : imageA;
        Image fadingIn = aIsTop ? imageA : imageB;

        fadingIn.transform.SetAsLastSibling();

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            fadingIn.color = new Color(1f, 1f, 1f, t);
            fadingOut.color = new Color(1f, 1f, 1f, 1f - t);

            yield return null;
        }

        fadingIn.color = Color.white;
        fadingOut.color = new Color(1f, 1f, 1f, 0f);

        isFading = false;

        yield return new WaitForSeconds(inputLockDuration);
        inputLocked = false;

        waitingForFinalInteract = true;
    }

    private void ShowChoiceBox()
    {
        choiceShown = true;
        inputLocked = true;

        if (InputManager.Instance != null)
            InputManager.Instance.OnInteractPressed -= OnInteract;

        endingChoiceBox.Show(OnChoiceMade);

        // Re-subscribe so we can catch the final interact after the outcome slide
        if (InputManager.Instance != null)
            InputManager.Instance.OnInteractPressed += OnInteract;
    }

    private void OnChoiceMade(int index)
    {
        inputLocked = false;

        Sprite outcomeSprite;
        switch (index)
        {
            case 0:
                outcomeSprite = runSlide;
                pendingScene = SceneEnum.GoodEndingCutScene;
                break;
            case 1:
                outcomeSprite = freezeSlide;
                pendingScene = SceneEnum.BadEndingCutScene;
                break;
            default:
                outcomeSprite = approachSlide;
                pendingScene = SceneEnum.BadEndingCutScene;
                break;
        }

        StartCoroutine(CrossFadeToOutcome(outcomeSprite));
    }
}