using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BossDialogueManager : MonoBehaviour
{
    public static BossDialogueManager Instance;

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

    private BossBruxa bossAtual;

    private DialogueData currentDialogueData;


    // =========================================================
    // PROPRIEDADE
    // =========================================================

    public bool IsDialogueOpen
    {
        get
        {
            return dialoguePanel != null &&
                   dialoguePanel.activeSelf;
        }
    }


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        Instance = this;
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

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


    // =========================================================
    // INICIAR DIÁLOGO
    // =========================================================

    public void StartBossDialogue(
        DialogueData dialogueData,
        BossBruxa boss)
    {
        if (dialogueData == null)
        {
            Debug.LogWarning(
                "BossDialogueManager: DialogueData não foi definido.");

            return;
        }

        if (dialogueData.mainDialogue == null ||
            dialogueData.mainDialogue.Length == 0)
        {
            Debug.LogWarning(
                "BossDialogueManager: o diálogo não possui linhas.");

            return;
        }

        if (dialoguePanel == null)
        {
            Debug.LogError(
                "BossDialogueManager: Dialogue Panel não foi atribuído no Inspector.");

            return;
        }

        if (portraitImage == null ||
            speakerNameText == null ||
            dialogueText == null)
        {
            Debug.LogError(
                "BossDialogueManager: uma ou mais referências da UI não foram atribuídas.");

            return;
        }


        // ---------------------------------------------------------
        // GUARDA AS REFERÊNCIAS DO DIÁLOGO
        // ---------------------------------------------------------

        bossAtual = boss;

        currentDialogueData = dialogueData;

        currentLines = dialogueData.mainDialogue;

        currentLineIndex = 0;


        // ---------------------------------------------------------
        // CONTROLE DE INPUT
        // ---------------------------------------------------------

        ignoreNextInput = true;


        // ---------------------------------------------------------
        // ESCONDE BOTÃO DE INTERAÇÃO
        // ---------------------------------------------------------

        ObserverManager.Notify("HideInteractButton");


        // ---------------------------------------------------------
        // DESABILITA CONTROLES DO PLAYER
        // ---------------------------------------------------------

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            PlayerController playerController =
                player.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.DisableControls();
            }
        }


        // ---------------------------------------------------------
        // ABRE PAINEL
        // ---------------------------------------------------------

        dialoguePanel.SetActive(true);

        ShowLine();
    }


    // =========================================================
    // MOSTRAR LINHA
    // =========================================================

    private void ShowLine()
    {
        if (currentLines == null ||
            currentLines.Length == 0)
        {
            EndDialogue();
            return;
        }

        if (currentLineIndex < 0 ||
            currentLineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }


        DialogueLine line =
            currentLines[currentLineIndex];


        if (line == null)
        {
            currentLineIndex++;

            if (currentLineIndex >= currentLines.Length)
            {
                EndDialogue();
                return;
            }

            ShowLine();
            return;
        }


        // ---------------------------------------------------------
        // UI
        // ---------------------------------------------------------

        portraitImage.sprite =
            line.portrait;

        speakerNameText.text =
            line.speakerName;

        fullText =
            line.dialogueText;


        // ---------------------------------------------------------
        // CORROTINA ANTERIOR
        // ---------------------------------------------------------

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }


        // ---------------------------------------------------------
        // TYPEWRITER
        // ---------------------------------------------------------

        typingCoroutine =
            StartCoroutine(TypeText());
    }


    // =========================================================
    // TYPEWRITER
    // =========================================================

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

        typingCoroutine = null;
    }


    // =========================================================
    // VOZ
    // =========================================================

    private void PlayVoiceSound(DialogueLine line)
    {
        if (line == null)
            return;

        if (currentDialogueData == null)
            return;

        if (string.IsNullOrEmpty(
            currentDialogueData.voiceSoundName))
        {
            return;
        }


        float pitch = Random.Range(
            line.voicePitchMin,
            line.voicePitchMax);


        SoundEffectManager.PlayWithPitch(
            currentDialogueData.voiceSoundName,
            pitch);
    }


    // =========================================================
    // PRÓXIMA LINHA
    // =========================================================

    public void NextLine()
    {
        if (!IsDialogueOpen)
            return;


        // ---------------------------------------------------------
        // SE AINDA ESTÁ DIGITANDO
        // ---------------------------------------------------------

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


        // ---------------------------------------------------------
        // PRÓXIMA LINHA
        // ---------------------------------------------------------

        currentLineIndex++;


        if (currentLines == null ||
            currentLineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }


        ShowLine();
    }


    // =========================================================
    // FINALIZAR DIÁLOGO
    // =========================================================

    private void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;


        // ---------------------------------------------------------
        // FECHA PAINEL
        // ---------------------------------------------------------

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }


        // ---------------------------------------------------------
        // DEVOLVE CONTROLE AO PLAYER
        // ---------------------------------------------------------

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            PlayerController playerController =
                player.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.EnableControls();
            }
        }


        // ---------------------------------------------------------
        // GUARDA O BOSS
        // ---------------------------------------------------------

        BossBruxa boss =
            bossAtual;

        bossAtual = null;


        // ---------------------------------------------------------
        // LIMPA DADOS DO DIÁLOGO
        // ---------------------------------------------------------

        currentLines = null;

        currentDialogueData = null;

        currentLineIndex = 0;

        fullText = "";


        // ---------------------------------------------------------
        // AVISA O BOSS
        // ---------------------------------------------------------

        if (boss != null)
        {
            boss.ConcluirDialogoInicial();
        }
    }
}