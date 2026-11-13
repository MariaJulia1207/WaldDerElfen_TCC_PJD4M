using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractButtonController : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonVisual;

    private void OnEnable()
    {
        ObserverManager.Subscribe("ShowInteractButton", ShowButton);
        ObserverManager.Subscribe("HideInteractButton", HideButton);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        ObserverManager.Unsubscribe("ShowInteractButton", ShowButton);
        ObserverManager.Unsubscribe("HideInteractButton", HideButton);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (buttonVisual != null)
        {
            buttonVisual.SetActive(false);
        }
        else
        {
            Debug.LogError($"[InteractButtonController] buttonVisual NÃO atribuído no Inspector de {gameObject.name}");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Garante que o botão nunca fique visível ao trocar de cena (ex: indo para a CutsceneFinal)
        HideButton();
    }

    private void ShowButton()
    {
        if (buttonVisual != null)
        {
            buttonVisual.SetActive(true);
        }
    }

    private void HideButton()
    {
        if (buttonVisual != null)
        {
            buttonVisual.SetActive(false);
        }
    }

    /// <summary>
    /// Método público para ser chamado via UnityEvent no fim de diálogos ou Signal Track na Timeline.
    /// </summary>
    public void ForceHideButton()
    {
        HideButton();
    }

    public void OnInteractPressed()
    {
        if (InteractOM.Instance != null)
        {
            InteractOM.Instance.Interact();
        }
        else
        {
            Debug.LogError("[InteractButtonController] InteractOM.Instance é NULO!");
        }
    }
}