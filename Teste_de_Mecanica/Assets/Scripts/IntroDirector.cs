using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroDirector : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private IntroTextWriter introTextWriter;
    [SerializeField] private LevelLoader levelLoader;

    private void Start()
    {
        if (introTextWriter == null)
        {
            introTextWriter = FindAnyObjectByType<IntroTextWriter>();
        }

        StartCoroutine(EsperarLoaderEIniciarIntro());
    }

    private LevelLoader GetLevelLoader()
    {
        return levelLoader != null ? levelLoader : LevelLoader.Instance;
    }

    private IEnumerator EsperarLoaderEIniciarIntro()
    {
        LevelLoader loader = GetLevelLoader();

        if (loader != null)
        {
            // Timeout de segurança (3s) para impedir que o jogo trave se o LevelLoader não responder
            float timeout = 3f;
            float timer = 0f;

            while (!loader.IsFullyTransparent && timer < timeout)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }

        if (introTextWriter != null)
        {
            introTextWriter.IniciarIntroducao();
        }
        else
        {
            Debug.LogError("IntroDirector: Nenhum IntroTextWriter encontrado na cena!");
        }
    }

    public void StartIntro()
    {
        StartCoroutine(EsperarLoaderEIniciarIntro());
    }

    public void LoadLevel1()
    {
        LevelLoader loader = GetLevelLoader();

        if (loader != null)
        {
            loader.Transition("Level1");
            return;
        }

        SceneManager.LoadScene("Level1");
    }
}