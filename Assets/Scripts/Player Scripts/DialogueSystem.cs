using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using System;

public class DialogueSystem : MonoBehaviour
{
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    [TextArea(3, 10)]

    public string dialogMessage = "testing the dialog";
    public float typingSpeed = 0.05f;
    private bool isPlayerInRange = false;
    private bool isDialogActive = false;

    private Coroutine typingCoroutine;
    public InputManager interactAction;
    public static DialogueSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple instances of DialogueSystem detected. Destroying duplicate.", this);
            Destroy(gameObject);
        }
    }



   


    void Start()
    {
        InputManager.Instance.OnInteractPressed += OnInteract;
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Something is wrong", this);
        }

        StartDialog();
    }

    private void OnInteract()
    {
        CloseDialog();
    }


    private void StartDialog()
    {
        SceneChangeData.Instance.isInDiagloue = true;
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
        SceneChangeData.Instance.isInDiagloue = false;
        isDialogActive = false;
        dialogPanel.SetActive(false);
        dialogText.text = "";

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Player"))
    //    {
    //        isPlayerInRange = true;
    //        if (isPlayerInRange)
    //        {
    //            if (!isDialogActive)
    //            {
    //                StartDialog();
    //            }
    //            else
    //            {
    //                CloseDialog();
    //            }
    //        }
    //        //Debug.Log("Player is in the area!");
    //    }
    //}
    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Player"))
    //    {
    //        isPlayerInRange = false;
    //        //Debug.Log("Player is out the area!");
    //        if (isDialogActive)
    //        {
    //            CloseDialog();
    //        }
    //    }
    //}
}
