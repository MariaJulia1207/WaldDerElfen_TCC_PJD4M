using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;

    private DialogueData currentDialogue;
    private int currentLine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueData dialogue)
    {
        currentDialogue = dialogue;
        currentLine = 0;

        dialoguePanel.SetActive(true);

        npcNameText.text = dialogue.npcName;

        ShowLine();
    }

    private void ShowLine()
    {
        dialogueText.text =
            currentDialogue.dialogueLines[currentLine];
    }

    public void NextLine()
    {
        currentLine++;

        if (currentLine >= currentDialogue.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        ObserverManager.Notify("DialogueFinished");
    }
}