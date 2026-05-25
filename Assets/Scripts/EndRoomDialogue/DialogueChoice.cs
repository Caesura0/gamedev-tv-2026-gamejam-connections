using UnityEngine;

[CreateAssetMenu(fileName = "DialogueChoice", menuName = "Dialogue/Dialogue Choice")]
public class DialogueChoice : ScriptableObject
{
    [Header("Option A")]
    public string goodEnding = "Option A";

    [Header("Option B")]
    public string badEnding = "Option B";

    [Header("Option B")]
    public string badderEnding = "Option B";
}