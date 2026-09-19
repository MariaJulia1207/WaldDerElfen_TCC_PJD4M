using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;

    public Sprite portrait;

    [TextArea(2, 5)]
    public string dialogueText;

    [Header("Voz")]
    [Range(0.1f, 3f)]
    public float voicePitchMin = 0.9f;

    [Range(0.1f, 3f)]
    public float voicePitchMax = 1.1f;
}