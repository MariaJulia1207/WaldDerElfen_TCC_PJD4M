using UnityEngine;

public class AreaExit : MonoBehaviour
{
    [Header("Cena")]
    [SerializeField] private string sceneToLoad;

    [Header("Transição")]
    [SerializeField] private string sceneTransitionName;

    [Header("Level Loader")]
    [SerializeField] private LevelLoader levelLoader;

    private bool ativado;

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
            levelLoader.Transition(sceneToLoad);
        }
    }
}