using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaExit : MonoBehaviour
{
    [Header("Cena")]
    [SerializeField] private string sceneToLoad;

    [Header("Transição")]
    [SerializeField] private string sceneTransitionName;

    [Header("Level Loader")]
    [SerializeField] private LevelLoader levelLoader;

    private bool ativado;

    private void Start()
    {
        // Preferir o LevelLoader presente na cena GUI (se carregada).
        levelLoader = FindGuiLevelLoader() ?? levelLoader ?? FindObjectOfType<LevelLoader>();
    }

    private LevelLoader FindGuiLevelLoader()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
                continue;

            // Procura por cenas que tenham 'GUI' no nome (ajustável conforme projeto)
            if (scene.name != null && scene.name.ToLower().Contains("gui"))
            {
                GameObject[] roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    LevelLoader ll = root.GetComponentInChildren<LevelLoader>();
                    if (ll != null)
                        return ll;
                }
            }
        }

        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ativado)
            return;

        if (other.GetComponent<PlayerController>() != null)
        {
            ativado = true;

            // Guarda qual entrada deve ser usada na próxima cena
            SceneManagement.SetTransitionName(sceneTransitionName);

            // Faz o Fade Out e carrega a cena
            if (levelLoader != null)
            {
                levelLoader.Transition(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("AreaExit: Nenhum LevelLoader encontrado para realizar a transição da cena: " + sceneToLoad);
            }
        }
    }
}