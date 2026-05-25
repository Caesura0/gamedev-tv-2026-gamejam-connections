using UnityEngine;

[CreateAssetMenu(fileName = "DialogueLine", menuName = "Dialogue/Dialogue Line")]
public class DialogueLine : ScriptableObject
{
    [TextArea(3, 8)]
    public string text;

    [Tooltip("Characters per second for the typewriter effect.")]
    public float typingSpeed = 40f;
}