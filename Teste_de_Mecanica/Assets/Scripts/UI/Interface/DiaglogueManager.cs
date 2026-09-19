using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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

    private bool ignoreNextInput;

    private string fullText;

    private NPCInteractable currentNPC;

    private DialogueData currentDialogueData;

    public bool IsDialogueOpen
    {
        get
        {
            return dialoguePanel != null &&
                   dialoguePanel.activeSelf;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!IsDialogueOpen)
            return;

        if (ignoreNextInput)
        {
            ignoreNextInput = false;
            return;
        }

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            NextLine();
        }
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

        // Esconde o botão enquanto o diálogo estiver aberto
        ObserverManager.Notify("HideInteractButton");

        dialoguePanel.SetActive(true);

        // Impede que o mesmo E usado para iniciar o diálogo também avance a primeira linha.
        ignoreNextInput = true;

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

        DialogueLine line =
            currentLines[currentLineIndex];

        foreach (char letter in fullText)
        {
            dialogueText.text += letter;

            if (!char.IsWhiteSpace(letter) &&
                !char.IsPunctuation(letter))
            {
                PlayVoiceSound(line);
            }

            yield return new WaitForSeconds(letterDelay);
        }

        isTyping = false;
    }

    private void PlayVoiceSound(DialogueLine line)
    {
        if (currentDialogueData == null)
            return;

        if (string.IsNullOrEmpty(
            currentDialogueData.voiceSoundName))
            return;

        float pitch = Random.Range(
            line.voicePitchMin,
            line.voicePitchMax);

        SoundEffectManager.PlayWithPitch(
            currentDialogueData.voiceSoundName,
            pitch);
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

        // Se o Player ainda estiver dentro do trigger,
        // o botão volta a aparecer.
        if (InteractOM.Instance != null)
        {
            ObserverManager.Notify("ShowInteractButton");
        }
    }
}