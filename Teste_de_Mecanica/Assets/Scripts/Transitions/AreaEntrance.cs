using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaEntrance : MonoBehaviour
{
    [SerializeField] private string transitionName;

    private void Start()
    {
        if (SceneManagement.SceneTransitionName == transitionName)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                player.transform.position = transform.position;

                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }

            // Consome a transição para não reutilizá-la
            SceneManagement.ConsumeTransitionName();
        }

        // Garante que, se necessário, o LevelLoader da cena GUI seja detectado
        // (não usado diretamente aqui, mas evita erros em outros fluxos que esperam o loader)
        _ = FindGuiLevelLoader();
    }

    private LevelLoader FindGuiLevelLoader()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
                continue;

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
}