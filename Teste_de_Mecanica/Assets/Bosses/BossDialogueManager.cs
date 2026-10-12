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

    private BossBruxa bossBruxaAtual;
    private BossArvore bossArvoreAtual;

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


    public void StartBossDialogue(DialogueData dialogueData,BossBruxa boss)
    {
        bossBruxaAtual = boss;
        bossArvoreAtual = null;

        IniciarDialogo(dialogueData);
    }

    public void StartBossDialogue(DialogueData dialogueData,BossArvore boss)
    {
        bossArvoreAtual = boss;
        bossBruxaAtual = null;

        IniciarDialogo(dialogueData);
    }

    private void IniciarDialogo(DialogueData dialogueData)
    {
        if (dialogueData == null)
        {
            Debug.LogWarning(
                "BossDialogueManager: DialogueData não foi definido."
            );

            NotificarDialogoConcluido();
            return;
        }

        if (IsDialogueOpen)
        {
            Debug.LogWarning(
                "BossDialogueManager: já existe um diálogo aberto."
            );

            NotificarDialogoConcluido();
            return;
        }

        if (dialogueData.mainDialogue == null ||
            dialogueData.mainDialogue.Length == 0)
        {
            Debug.LogWarning(
                "BossDialogueManager: o diálogo não possui linhas."
            );

            NotificarDialogoConcluido();
            return;
        }

        if (dialoguePanel == null ||
            portraitImage == null ||
            speakerNameText == null ||
            dialogueText == null)
        {
            Debug.LogError(
                "BossDialogueManager: referências da UI não atribuídas."
            );

            NotificarDialogoConcluido();
            return;
        }

        currentDialogueData = dialogueData;
        currentLines = dialogueData.mainDialogue;
        currentLineIndex = 0;

        ignoreNextInput = true;

        ObserverManager.Notify("HideInteractButton");

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            PlayerController controller =
                player.GetComponent<PlayerController>();

            if (controller != null)
                controller.DisableControls();
        }

        dialoguePanel.SetActive(true);

        ShowLine();
    }

    private void NotificarDialogoConcluido()
    {
        BossBruxa bruxa = bossBruxaAtual;
        BossArvore arvore = bossArvoreAtual;

        bossBruxaAtual = null;
        bossArvoreAtual = null;

        if (bruxa != null)
            bruxa.ConcluirDialogoInicial();

        if (arvore != null)
            arvore.ConcluirDialogoInicial();
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
        SoundEffectManager.StopVoice();
        SoundEffectManager.ResetVoicePitch();

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

        NotificarDialogoConcluido();
    }
}