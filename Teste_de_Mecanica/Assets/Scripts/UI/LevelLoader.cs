using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Animator transitionAnim;
    [SerializeField] private float fadeDuration = 1f;

    private bool carregando;
    public bool IsFullyTransparent { get; private set; }

    private void Start()
    {
        IsFullyTransparent = false;

        if (transitionAnim != null)
        {
            transitionAnim.SetTrigger("End");
        }

        StartCoroutine(WaitForFadeIn());
    }

    public IEnumerator WaitUntilTransparent()
    {
        while (!IsFullyTransparent)
        {
            yield return null;
        }
    }

    public void Transition(string sceneName)
    {
        if (carregando)
            return;

        StartCoroutine(LoadScene(sceneName));
    }

    public void TransitionQuit()
    {
        if (carregando)
            return;

        StartCoroutine(QuitGame());
    }

    private IEnumerator WaitForFadeIn()
    {
        yield return new WaitForSeconds(fadeDuration);
        IsFullyTransparent = true;
    }

    private IEnumerator LoadScene(string sceneName)
    {
        carregando = true;

        if (transitionAnim != null)
        {
            transitionAnim.SetTrigger("Start");
        }

        yield return new WaitForSeconds(fadeDuration);

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator QuitGame()
    {
        carregando = true;

        if (transitionAnim != null)
        {
            transitionAnim.SetTrigger("Start");
        }

        yield return new WaitForSeconds(fadeDuration);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}