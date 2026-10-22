using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }

    [SerializeField] private Animator transitionAnim;
    [SerializeField] private float fadeDuration = 1f;

    private bool carregando;
    public bool IsFullyTransparent { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayFadeIn();
    }

    public void PlayFadeIn()
    {
        IsFullyTransparent = false;
        StopAllCoroutines();
        StartCoroutine(ExecuteFadeIn());
    }

    private IEnumerator ExecuteFadeIn()
    {
        yield return null; // Espera 1 frame para estabilização da nova cena

        if (transitionAnim != null)
        {
            transitionAnim.ResetTrigger("Start");
            transitionAnim.SetTrigger("End");
        }

        yield return new WaitForSecondsRealtime(fadeDuration);
        IsFullyTransparent = true;
    }

    // Método que estava faltando para o IntroDirector
    public IEnumerator WaitUntilTransparent()
    {
        while (!IsFullyTransparent)
        {
            yield return null;
        }
    }

    public void Transition(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || carregando)
            return;

        string activeScene = SceneManager.GetActiveScene().name;
        if (string.Equals(activeScene, sceneName, System.StringComparison.Ordinal))
        {
            return;
        }

        StartCoroutine(LoadScene(sceneName));
    }

    public void TransitionQuit()
    {
        if (carregando)
            return;

        StartCoroutine(QuitGame());
    }

    private IEnumerator LoadScene(string sceneName)
    {
        carregando = true;

        if (transitionAnim != null)
        {
            transitionAnim.ResetTrigger("End");
            transitionAnim.SetTrigger("Start");
        }

        yield return new WaitForSecondsRealtime(fadeDuration);

        if (!string.Equals(SceneManager.GetActiveScene().name, sceneName, System.StringComparison.Ordinal))
        {
            SceneManager.LoadScene(sceneName);
        }

        carregando = false;
    }

    private IEnumerator QuitGame()
    {
        carregando = true;

        if (transitionAnim != null)
        {
            transitionAnim.ResetTrigger("End");
            transitionAnim.SetTrigger("Start");
        }

        yield return new WaitForSecondsRealtime(fadeDuration);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}