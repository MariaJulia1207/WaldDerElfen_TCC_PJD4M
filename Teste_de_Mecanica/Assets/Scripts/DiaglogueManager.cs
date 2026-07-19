using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;

    [SerializeField] private Image portraitImage;

    [SerializeField] private TMP_Text speakerNameText;

    [SerializeField] private TMP_Text dialogueText;

    [Header("Typewriter")]
    [SerializeField] private float letterDelay = 0.03f;

    private DialogueLine[] currentLines;

    private int currentLineIndex;

    private Coroutine typingCoroutine;

    private bool isTyping;

    private string fullText;

    private NPCInteractable currentNPC;

    private DialogueData currentDialogueData;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(
        DialogueData dialogueData,
        NPCInteractable npc,
        bool alreadyCompleted)
    {
        currentDialogueData = dialogueData;
        currentNPC = npc;

        if (dialogueData.playOnlyOnce &&
            alreadyCompleted)
        {
            currentLines =
                dialogueData.summaryDialogue;
        }
        else
        {
            currentLines =
                dialogueData.mainDialogue;
        }

        currentLineIndex = 0;

        dialoguePanel.SetActive(true);

        ShowLine();
    }

    private void ShowLine()
    {
        DialogueLine line =
            currentLines[currentLineIndex];

        portraitImage.sprite =
            line.portrait;

        speakerNameText.text =
            line.speakerName;

        fullText =
            line.dialogueText;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine =
            StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isTyping = true;

        dialogueText.text = "";

        foreach (char letter in fullText)
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(
                letterDelay);
        }

        isTyping = false;
    }

    public void NextLine()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);

            dialogueText.text = fullText;

            isTyping = false;

            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    private void EndDialogue()
    {
        if (
            currentDialogueData.playOnlyOnce &&
            currentNPC != null
        )
        {
            currentNPC.CompleteDialogue();
        }

        dialoguePanel.SetActive(false);
    }
}