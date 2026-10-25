using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

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

    private int lastCloseFrame = -1;

    /// <summary>
    /// Retorna verdadeiro se o diálogo estiver aberto ou se acabou de ser fechado neste mesmo frame.
    /// </summary>
    public bool JustClosedThisFrame => lastCloseFrame == Time.frameCount;

    public bool IsDialogueOpen
    {
        get
        {
            return dialoguePanel != null && dialoguePanel.activeSelf;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        FindPlayerController();
    }

    private void FindPlayerController()
    {
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
            }
        }
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

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            NextLine();
        }
    }

    public void StartDialogue(DialogueData dialogueData, NPCInteractable npc, bool alreadyCompleted)
    {
        if (dialogueData == null || npc == null)
            return;

        if (IsDialogueOpen)
            return;

        currentDialogueData = dialogueData;
        currentNPC = npc;

        currentDialogueIsMain = !(dialogueData.playOnlyOnce && alreadyCompleted);

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

        FindPlayerController();
        if (playerController != null)
            playerController.DisableControls();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowLine();
    }

    private void ShowLine()
    {
        if (currentLines == null || currentLineIndex < 0 || currentLineIndex >= currentLines.Length)
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

        if (portraitImage != null) portraitImage.sprite = line.portrait;
        if (speakerNameText != null) speakerNameText.text = line.speakerName;
        fullText = line.dialogueText ?? "";

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        if (dialogueText != null) dialogueText.text = "";

        DialogueLine line = currentLines[currentLineIndex];

        for (int i = 0; i < fullText.Length; i++)
        {
            char letter = fullText[i];

            if (dialogueText != null) dialogueText.text += letter;

            if (!char.IsWhiteSpace(letter) && !char.IsPunctuation(letter))
            {
                PlayVoiceSound(line);
            }

            // Não espera no último caractere para liberar a entrada imediatamente
            if (i < fullText.Length - 1)
            {
                yield return new WaitForSeconds(letterDelay);
            }
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void PlayVoiceSound(DialogueLine line)
    {
        if (line == null || currentDialogueData == null)
            return;

        if (string.IsNullOrWhiteSpace(currentDialogueData.voiceSoundName))
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

            if (dialogueText != null) dialogueText.text = fullText;
            isTyping = false;
            return;
        }

        currentLineIndex++;

        if (currentLines == null || currentLineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    private void EndDialogue()
    {
        // Marca o frame exato em que o diálogo fechou
        lastCloseFrame = Time.frameCount;

        SoundEffectManager.StopVoice();
        SoundEffectManager.ResetVoicePitch();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;

        NPCInteractable npc = currentNPC;
        DialogueData data = currentDialogueData;
        bool terminouDialogoPrincipal = currentDialogueIsMain;

        if (terminouDialogoPrincipal && data != null && data.playOnlyOnce && npc != null)
        {
            npc.CompleteDialogue();
        }

        if (terminouDialogoPrincipal && data != null && npc != null)
        {
            ExecutarAcoesPosDialogo(data, npc);
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        FindPlayerController();
        if (playerController != null)
            playerController.EnableControls();

        currentNPC = null;
        currentDialogueData = null;
        currentLines = null;
        currentDialogueIsMain = false;

        if (InteractOM.Instance != null && InteractOM.Instance.CurrentNPC != null)
        {
            ObserverManager.Notify("ShowInteractButton");
        }
    }

    private void ExecutarAcoesPosDialogo(DialogueData data, NPCInteractable npc)
    {
        if (data == null || npc == null) return;

        if (data.moverNpcAposDialogo)
        {
            npc.transform.position = new Vector3(
                data.posicaoDestinoNpc.x,
                data.posicaoDestinoNpc.y,
                npc.transform.position.z);
        }

        if (data.criarItemAposDialogo && data.prefabItemColetavel != null)
        {
            Vector2 posicaoNpc = npc.transform.position;
            Vector2 posicaoBase = posicaoNpc;

            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                posicaoBase = (posicaoNpc + (Vector2)player.transform.position) * 0.5f;
            }

            Vector2 posicaoItem = posicaoBase + data.deslocamentoItem;

            Instantiate(
                data.prefabItemColetavel,
                posicaoItem,
                Quaternion.identity);
        }
    }
}