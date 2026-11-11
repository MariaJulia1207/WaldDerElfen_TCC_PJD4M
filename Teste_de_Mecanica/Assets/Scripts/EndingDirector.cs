using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class EndingDirector : MonoBehaviour
{
    [Header("Referências Principais")]
    [SerializeField] private EndingTextWriter endingTextWriter;
    [SerializeField] private LevelLoader levelLoader;

    [Header("Áudio da Cutscene")]
    [Tooltip("Se ativado, inicia a música do final automaticamente após o Fade In.")]
    [SerializeField] private bool tocarMusicaNoInicio = true;
    [Tooltip("Nome exato da faixa cadastrada na MusicLibrary da cena Boot.")]
    [SerializeField] private string nomeMusicaFinal = "EndingTheme";

    [Header("Cutscenes Animadas (Timelines)")]
    [Tooltip("Arraste aqui as Timelines na ordem exata em que devem ser exibidas entre os parágrafos.")]
    [SerializeField] private PlayableDirector[] timelinesCutscenes;

    [Header("Créditos & Rolagem")]
    [SerializeField] private GameObject painelCreditos;
    [SerializeField] private CreditsScroller creditsScroller;

    [Header("Navegação")]
    [SerializeField] private string nomeCenaMenu = "Menu";

    private bool _escritaConcluida = false;

    private void Awake()
    {
        if (endingTextWriter == null)
        {
            // O argumento FindObjectsInactive.Include permite encontrar o componente mesmo se a UI estiver oculta
            endingTextWriter = FindAnyObjectByType<EndingTextWriter>(FindObjectsInactive.Include);
        }

        if (endingTextWriter != null)
        {
            endingTextWriter.aoFinalizarTodosParagrafos.AddListener(OnFinalizarTodosParagrafos);
        }
    }

    private void OnDestroy()
    {
        if (endingTextWriter != null)
        {
            endingTextWriter.aoFinalizarTodosParagrafos.RemoveListener(OnFinalizarTodosParagrafos);
        }
    }

    private void Start()
    {
        StartCoroutine(ExecutarSequenciaFinal());
    }

    private LevelLoader GetLevelLoader()
    {
        return levelLoader != null ? levelLoader : LevelLoader.Instance;
    }

    private IEnumerator ExecutarSequenciaFinal()
    {
        // 1. Aguarda o LevelLoader com timeout de segurança (máximo 3 segundos)
        LevelLoader loader = GetLevelLoader();
        if (loader != null)
        {
            float timeout = 3f;
            float timer = 0f;

            while (!loader.IsFullyTransparent && timer < timeout)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }

        // 2. Dispara a música da cutscene final
        if (tocarMusicaNoInicio && !string.IsNullOrEmpty(nomeMusicaFinal))
        {
            BackgroundMusic.Play(nomeMusicaFinal, loop: false, fadeDuration: 1.0f);
        }

        // 3. Inicia o fluxo de escrita de texto
        if (endingTextWriter != null)
        {
            endingTextWriter.IniciarEscritaFinal();
        }
        else
        {
            Debug.LogError("EndingDirector: EndingTextWriter não foi encontrado ou atribuído no Inspector!", this);
            yield break;
        }

        // 4. Aguarda a digitação dos textos e as Timelines intermediárias terminarem
        yield return new WaitUntil(() => _escritaConcluida);

        // 5. Ativa os Créditos e inicia a rolagem vertical
        if (painelCreditos != null)
        {
            painelCreditos.SetActive(true);

            if (creditsScroller != null)
            {
                creditsScroller.IniciarRolagem();
            }
            else
            {
                // Tenta encontrar o componente no próprio painel caso não esteja atribuído
                creditsScroller = painelCreditos.GetComponentInChildren<CreditsScroller>();
                if (creditsScroller != null)
                {
                    creditsScroller.IniciarRolagem();
                }
            }
        }

        // 6. Aguarda a música da cutscene terminar completamente
        yield return StartCoroutine(BackgroundMusic.WaitUntilFinished());

        // 7. Elimina os dados salvos do jogador
        DeletarSaveDoJogador();

        // 8. Faz o Fade Out para ecrã preto e carrega a cena do Menu Principal
        if (loader != null)
        {
            loader.Transition(nomeCenaMenu);
        }
        else
        {
            SceneManager.LoadScene(nomeCenaMenu);
        }
    }

    /// <summary>
    /// Método chamado pelo EndingTextWriter quando um parágrafo tem 'temCutsceneDepois = true'.
    /// </summary>
    public IEnumerator TocarCutsceneIntermediaria(int indice)
    {
        if (timelinesCutscenes == null || indice < 0 || indice >= timelinesCutscenes.Length)
        {
            Debug.LogWarning($"EndingDirector: Nenhum PlayableDirector configurado para o índice {indice}.", this);
            yield break;
        }

        PlayableDirector timeline = timelinesCutscenes[indice];

        if (timeline != null)
        {
            timeline.gameObject.SetActive(true);
            timeline.Play();

            // Aguarda o término da Timeline
            yield return new WaitUntil(() => timeline.state != PlayState.Playing);

            timeline.gameObject.SetActive(false);
        }
    }

    private void OnFinalizarTodosParagrafos()
    {
        _escritaConcluida = true;
    }

    private void DeletarSaveDoJogador()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("EndingDirector: Save do jogo eliminado com sucesso.");
    }
}