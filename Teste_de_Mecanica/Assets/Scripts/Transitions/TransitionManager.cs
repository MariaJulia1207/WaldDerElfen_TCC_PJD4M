using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("Fade")]
    [SerializeField] private Animator transitionAnim;

    [SerializeField] private float tempoFade = 1f;

    private bool carregandoCena;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Transition(string sceneName, string spawnPointName)
    {
        if (carregandoCena)
            return;

        StartCoroutine(CarregarCena(sceneName, spawnPointName));
    }

    private IEnumerator CarregarCena(string sceneName, string spawnPointName)
    {
        carregandoCena = true;

        // FADE OUT
        transitionAnim.SetTrigger("Start");

        yield return new WaitForSeconds(tempoFade);

        // Carrega a cena
        SceneManager.LoadScene(sceneName);

        // Espera a cena terminar de carregar
        yield return null;

        // Procura o ponto onde o jogador deve aparecer
        SpawnPoint spawnPoint = EncontrarSpawnPoint(spawnPointName);

        if (spawnPoint != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                player.transform.position = spawnPoint.transform.position;
            }
        }

        // FADE IN
        transitionAnim.SetTrigger("End");

        yield return new WaitForSeconds(tempoFade);

        carregandoCena = false;
    }

    private SpawnPoint EncontrarSpawnPoint(string nome)
    {
        SpawnPoint[] pontos = FindObjectsByType<SpawnPoint>(
            FindObjectsSortMode.None
        );

        foreach (SpawnPoint ponto in pontos)
        {
            if (ponto.nomePonto == nome)
            {
                return ponto;
            }
        }

        Debug.LogWarning(
            "Spawn Point não encontrado: " + nome
        );

        return null;
    }
}