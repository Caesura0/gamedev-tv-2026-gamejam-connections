
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{

    [SerializeField, TextArea]
    private string dialogueInput;

    private void Start()
    {
        DialogueSystem.Instance.StartDialog(dialogueInput);
    }

}
