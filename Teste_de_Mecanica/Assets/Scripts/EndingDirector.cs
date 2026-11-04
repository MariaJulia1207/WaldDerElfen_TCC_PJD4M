using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class EndingDirector : MonoBehaviour
{
    [Header("Referências Principais")]
    [SerializeField] private EndingTextWriter endingTextWriter;
    [SerializeField] private LevelLoader levelLoader;

    [Header("Áudio da Cutscene")]
    [Tooltip("Nome exato da faixa cadastrada no MusicLibrary da cena Boot.")]
    [SerializeField] private string nomeMusicaFinal = "EndingTheme";

    [Header("Cutscenes Animadas (Timelines)")]
    [Tooltip("Timelines na ordem exata dos parágrafos onde 'temCutsceneDepois' está marcado.")]
    [SerializeField] private PlayableDirector[] timelinesCutscenes;

    [Header("Créditos & Menu")]
    [SerializeField] private GameObject painelCreditos;
    [SerializeField] private string nomeCenaMenu = "MainMenu";

    private bool _escritaConcluida = false;

    private void Awake()
    {
        if (endingTextWriter == null)
        {
            endingTextWriter = FindAnyObjectByType<EndingTextWriter>();
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
        // 1. Aguarda o Fade In inicial da cena via LevelLoader
        LevelLoader loader = GetLevelLoader();
        if (loader != null)
        {
            yield return loader.WaitUntilTransparent();
        }

        // 2. Dispara a música tema da cutscene (com loop = false)
        if (!string.IsNullOrEmpty(nomeMusicaFinal))
        {
            BackgroundMusic.Play(nomeMusicaFinal, loop: false, fadeDuration: 1.0f);
        }

        // 3. Inicia a digitação dos textos
        if (endingTextWriter != null)
        {
            endingTextWriter.IniciarEscritaFinal();
        }
        else
        {
            Debug.LogError("EndingDirector: EndingTextWriter não configurado!");
            yield break;
        }

        // 4. Aguarda a escrita de todos os parágrafos e cutscenes intermediárias
        yield return new WaitUntil(() => _escritaConcluida);

        // 5. Exibe a rolagem dos créditos
        if (painelCreditos != null)
        {
            painelCreditos.SetActive(true);
        }

        // 6. Aguarda a música terminar completamente
        yield return StartCoroutine(BackgroundMusic.WaitUntilFinished());

        // 7. Deleta o save do jogador
        DeletarSaveDoJogador();

        // 8. Tela fica preta (via LevelLoader) e retorna ao Menu Principal
        if (loader != null)
        {
            loader.Transition(nomeCenaMenu);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nomeCenaMenu);
        }
    }

    public IEnumerator TocarCutsceneIntermediaria(int indice)
    {
        if (timelinesCutscenes == null || indice >= timelinesCutscenes.Length)
        {
            Debug.LogWarning($"EndingDirector: Timeline no índice {indice} não foi atribuída no array!");
            yield break;
        }

        PlayableDirector timeline = timelinesCutscenes[indice];

        if (timeline != null)
        {
            timeline.gameObject.SetActive(true);
            timeline.Play();

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
        // Insira o método do seu sistema de Save/Load
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("EndingDirector: Save deletado com sucesso.");
    }
}