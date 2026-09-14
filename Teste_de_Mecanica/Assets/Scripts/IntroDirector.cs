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
        StartCoroutine(EsperarLoaderEIniciarIntro());
    }

    private IEnumerator EsperarLoaderEIniciarIntro()
    {
        if (levelLoader != null)
        {
            yield return StartCoroutine(levelLoader.WaitUntilTransparent());
        }

        if (introTextWriter != null)
        {
            introTextWriter.IniciarIntroducao();
        }
    }

    public void StartIntro()
    {
        StartCoroutine(EsperarLoaderEIniciarIntro());
    }

    public void LoadLevel1()
    {
        if (levelLoader != null)
        {
            levelLoader.Transition("Level1");
            return;
        }

        SceneManager.LoadScene("Level1");
    }
}