using UnityEngine;

[CreateAssetMenu(
    fileName = "New Dialogue",
    menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Executar apenas uma vez")]
    public bool playOnlyOnce;

    [Header("Diálogo Principal")]
    public DialogueLine[] mainDialogue;

    [Header("Resumo após terminar")]
    public DialogueLine[] summaryDialogue;
}