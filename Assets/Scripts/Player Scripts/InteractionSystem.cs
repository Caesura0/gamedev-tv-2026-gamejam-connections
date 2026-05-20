using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class InteractionSystem : MonoBehaviour
{
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    [TextArea(3, 10)]

    public string dialogMessage = "testing the dialog";
    public float typingSpeed = 0.05f;
    private bool isPlayerInRange = false;
    private bool isDialogActive = false;

    private Coroutine typingCoroutine;
    public InputActionReference interactAction;

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
    }


    void Start()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Something is wrong", this);
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (isPlayerInRange)
        {
            if (!isDialogActive)
            {
                StartDialog();
            }
            else
            {
                CloseDialog();
            }
        }
    }
    private void StartDialog()
    {
        isDialogActive = true;
        dialogPanel.SetActive(true);
        dialogText.text = "";
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeDialog(dialogMessage));
    }
    private IEnumerator TypeDialog(string message)
    {
        foreach (char letter in message.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    private void CloseDialog()
    {
        isDialogActive = false;
        dialogPanel.SetActive(false);
        dialogText.text = "";

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            //Debug.Log("Player is in the area!");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            //Debug.Log("Player is out the area!");
            if (isDialogActive)
            {
                CloseDialog();
            }
        }
    }
}
