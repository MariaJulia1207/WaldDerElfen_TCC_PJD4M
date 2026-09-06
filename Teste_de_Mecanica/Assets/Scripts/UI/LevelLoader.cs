using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Animator transitionAnim;

    private bool carregando;

    public void Transition(string sceneName)
    {
        if (carregando)
            return;

        StartCoroutine(LoadScene(sceneName));
    }

    private IEnumerator LoadScene(string sceneName)
    {
        carregando = true;

        // Fade Out
        transitionAnim.SetTrigger("Start");

        // Tempo da animação de Fade Out
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(sceneName);
    }
}