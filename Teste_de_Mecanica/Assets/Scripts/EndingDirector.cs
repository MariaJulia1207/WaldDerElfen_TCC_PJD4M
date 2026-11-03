using System.Collections;
using UnityEngine;
using UnityEngine.Playables; // Necessário para controlar as Timelines

public class EndingDirector : MonoBehaviour
{
    [Header("Referências de Sistema")]
    [SerializeField] private EndingTextWriter textWriter; // Ou um TextWriter genérico
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private AudioSource musicalCutscene;

    [Header("Cutscenes Animadas (Timeline)")]
    [SerializeField] private PlayableDirector timelinePersonagens;

    [Header("Créditos & Navegação")]
    [SerializeField] private GameObject painelCreditos;
    [SerializeField] private string nomeCenaMenu = "MainMenu";

    private void Start()
    {
        StartCoroutine(ExecutarSequenciaFinal());
    }

    private IEnumerator ExecutarSequenciaFinal()
    {
        // 1. Aguarda a transição de entrada da cena terminar
        yield return StartCoroutine(EsperarLoader());

        // 2. Executa a digitação dos textos de encerramento
        if (textWriter != null)
        {
            textWriter.IniciarEscritaFinal();
            // Espera a corrotina do texto finalizar se você expor o estado dele
        }

        // 3. Reproduz a Timeline com os personagens
        if (timelinePersonagens != null)
        {
            timelinePersonagens.Play();
            // Aguarda a Timeline terminar de rodar
            yield return new WaitUntil(() => timelinePersonagens.state != PlayState.Playing);
        }

        // 4. Ativa a rolagem de créditos
        if (painelCreditos != null)
        {
            painelCreditos.SetActive(true);
        }

        // 5. Espera a música principal terminar
        if (musicalCutscene != null)
        {
            yield return new WaitUntil(() => !musicalCutscene.isPlaying);
        }

        // 6. Deleta os dados do Save antes de sair
        DeletarSaveDoJogador();

        // 7. Retorna ao menu com o LevelLoader (com fade out automático)
        LevelLoader loader = GetLevelLoader();
        if (loader != null)
        {
            loader.Transition(nomeCenaMenu);
        }
    }

    private void DeletarSaveDoJogador()
    {
        // Substitua pelo seu método real de Save (ex: SaveSystem.DeleteSaveData())
        PlayerPrefs.DeleteAll(); 
        Debug.Log("Dados de save deletados com sucesso.");
    }

    private LevelLoader GetLevelLoader()
    {
        return levelLoader != null ? levelLoader : LevelLoader.Instance;
    }

    private IEnumerator EsperarLoader()
    {
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
    }

    public string TocarCutsceneIntermediaria(int indiceCutscene)
    {
        // Aqui você pode implementar a lógica para tocar uma cutscene intermediária
        // com base no índice fornecido. Por exemplo, você pode ter um array de
        // PlayableDirectors ou clipes de áudio que correspondam a cada índice.

        // Exemplo simples:
        switch (indiceCutscene)
        {
            case 0:
                // Tocar primeira cutscene
                if (timelinePersonagens != null)
                {
                    timelinePersonagens.Play();
                    return "Cutscene 0 iniciada.";
                }
                break;
            case 1:
                // Tocar segunda cutscene
                if (musicalCutscene != null)
                {
                    musicalCutscene.Play();
                    return "Cutscene 1 iniciada.";
                }
                break;
            default:
                return "Índice de cutscene inválido.";
        }

        return "Nenhuma cutscene foi iniciada.";
    }
}
