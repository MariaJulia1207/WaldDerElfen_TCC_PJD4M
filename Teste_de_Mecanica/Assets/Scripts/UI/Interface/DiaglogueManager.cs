
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
    private bool currentDialogueIsMain;

    private string fullText;

    private NPCInteractable currentNPC;
    private DialogueData currentDialogueData;
    private PlayerController playerController;

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
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            playerController =
                player.GetComponent<PlayerController>();
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

    public void StartDialogue(DialogueData dialogueData,NPCInteractable npc,bool alreadyCompleted)
    {
        if (dialogueData == null || npc == null)
            return;

        if (IsDialogueOpen)
            return;

        currentDialogueData = dialogueData;
        currentNPC = npc;

        currentDialogueIsMain =
            !(dialogueData.playOnlyOnce && alreadyCompleted);

        currentLines = currentDialogueIsMain
            ? dialogueData.mainDialogue
            : dialogueData.summaryDialogue;

        if (currentLines == null || currentLines.Length == 0)
        {
            currentNPC = null;
            currentDialogueData = null;
            return;
        }

        currentLineIndex = 0;
        isTyping = false;
        ignoreNextInput = true;

        ObserverManager.Notify("HideInteractButton");

        if (playerController != null)
            playerController.DisableControls();

        dialoguePanel.SetActive(true);

        ShowLine();
    }

    private void ShowLine()
    {
        if (currentLines == null ||
            currentLineIndex < 0 ||
            currentLineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentLines[currentLineIndex];

        if (line == null)
        {
            currentLineIndex++;
            ShowLine();
            return;
        }

        portraitImage.sprite = line.portrait;
        speakerNameText.text = line.speakerName;
        fullText = line.dialogueText ?? "";

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        dialogueText.text = "";

        DialogueLine line = currentLines[currentLineIndex];

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
        typingCoroutine = null;
    }

    private void PlayVoiceSound(DialogueLine line)
    {
        if (line == null || currentDialogueData == null)
            return;

        if (string.IsNullOrWhiteSpace(
            currentDialogueData.voiceSoundName))
            return;

        SoundEffectManager.PlayVoiceForSpeaker(
            currentDialogueData.voiceSoundName,
            line.speakerName,
            line.voicePitchMin,
            line.voicePitchMax);
    }

    public void NextLine()
    {
        if (!IsDialogueOpen)
            return;

        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            dialogueText.text = fullText;
            isTyping = false;
            return;
        }

        currentLineIndex++;

        if (currentLines == null ||
            currentLineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    private void EndDialogue()
    {
        SoundEffectManager.ResetVoicePitch();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;

        // Guarda as referências antes de limpar o estado.
        NPCInteractable npc = currentNPC;
        DialogueData data = currentDialogueData;
        bool terminouDialogoPrincipal = currentDialogueIsMain;

        // Registra a conclusão do diálogo de uso único.
        if (terminouDialogoPrincipal &&
            data != null &&
            data.playOnlyOnce &&
            npc != null)
        {
            npc.CompleteDialogue();
        }

        // As recompensas e o deslocamento só ocorrem
        // depois do diálogo principal, nunca no resumo.
        if (terminouDialogoPrincipal &&
            data != null &&
            npc != null)
        {
            ExecutarAcoesPosDialogo(data, npc);
        }

        dialoguePanel.SetActive(false);

        if (playerController != null)
            playerController.EnableControls();

        currentNPC = null;
        currentDialogueData = null;
        currentLines = null;
        currentDialogueIsMain = false;

        ObserverManager.Notify("ShowInteractButton");
    }

    private void ExecutarAcoesPosDialogo(DialogueData data,NPCInteractable npc)
    {
        // Move o NPC para a posição configurada.
        if (data.moverNpcAposDialogo)
        {
            npc.transform.position =
                new Vector3(
                    data.posicaoDestinoNpc.x,
                    data.posicaoDestinoNpc.y,
                    npc.transform.position.z);
        }

        // Cria o item entre o NPC e o jogador.
        if (data.criarItemAposDialogo &&
            data.prefabItemColetavel != null)
        {
            Vector2 posicaoNpc = npc.transform.position;
            Vector2 posicaoBase = posicaoNpc;

            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                posicaoBase = (
                    posicaoNpc +
                    (Vector2)player.transform.position
                ) * 0.5f;
            }

            Vector2 posicaoItem =
                posicaoBase + data.deslocamentoItem;

            Instantiate(
                data.prefabItemColetavel,
                posicaoItem,
                Quaternion.identity);
        }
    }
}