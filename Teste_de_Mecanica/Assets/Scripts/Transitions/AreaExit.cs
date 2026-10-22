using UnityEngine;

public class AreaExit : MonoBehaviour
{
    [Header("Cena")]
    [SerializeField] private string sceneToLoad;

    [Header("Transição")]
    [SerializeField] private string sceneTransitionName;

    private bool ativado;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ativado)
            return;

        if (other.GetComponent<PlayerController>() != null)
        {
            ativado = true;

            // Guarda a entrada da próxima cena
            SceneManagement.SetTransitionName(sceneTransitionName);

            // Executa a transição diretamente via Singleton
            if (LevelLoader.Instance != null)
            {
                LevelLoader.Instance.Transition(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("AreaExit: Nenhum LevelLoader.Instance foi encontrado na cena.");
            }
        }
    }
}