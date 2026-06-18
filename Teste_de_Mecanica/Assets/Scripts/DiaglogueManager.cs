using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialoguePanel;

    public Image portraitImage;

    public TMP_Text characterNameText;

    public TMP_Text dialogueText;

    private DialogueData currentDialogue;

    private int currentLine;

    private bool dialogueActive;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    public bool IsDialogueActive()
    {
        return dialogueActive;
    }

    public void StartDialogue(DialogueData dialogue, bool jaConversou)
    {
        dialoguePanel.SetActive(true);

        dialogueActive = true;

        if (jaConversou)
        {
            portraitImage.sprite =
                dialogue.linhas[0].portrait;

            characterNameText.text =
                dialogue.linhas[0].characterName;

            dialogueText.text =
                dialogue.resumoDialogo;

            currentDialogue = null;

            return;
        }

        currentDialogue = dialogue;
        currentLine = 0;

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        DialogueLine line =
            currentDialogue.linhas[currentLine];

        portraitImage.sprite =
            line.portrait;

        characterNameText.text =
            line.characterName;

        dialogueText.text =
            line.text;
    }

    public void NextLine()
    {
        if (!dialogueActive)
            return;

        if (currentDialogue == null)
        {
            EndDialogue();
            return;
        }

        currentLine++;

        if (currentLine >= currentDialogue.linhas.Count)
        {
            ObserverManager.Instance.RegisterDialogue(
                currentDialogue.npcID);

            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void EndDialogue()
    {
        dialogueActive = false;

        dialoguePanel.SetActive(false);

        ObserverManager.Instance.EndInteraction();
    }
}