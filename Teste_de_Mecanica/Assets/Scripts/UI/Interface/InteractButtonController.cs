using UnityEngine;

public class InteractButtonController : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonVisual;

    private void OnEnable()
    {
        ObserverManager.Subscribe("ShowInteractButton", ShowButton);
        ObserverManager.Subscribe("HideInteractButton", HideButton);
    }

    private void OnDisable()
    {
        ObserverManager.Unsubscribe("ShowInteractButton", ShowButton);
        ObserverManager.Unsubscribe("HideInteractButton", HideButton);
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

    private void ShowButton()
    {
        Debug.Log("[InteractButtonController] Evento ShowInteractButton recebido.");
        if (buttonVisual != null)
        {
            buttonVisual.SetActive(true);
        }
    }

    private void HideButton()
    {
        Debug.Log("[InteractButtonController] Evento HideInteractButton recebido.");
        if (buttonVisual != null)
        {
            buttonVisual.SetActive(false);
        }
    }

    public void OnInteractPressed()
    {
        Debug.Log("[InteractButtonController] Botão de UI pressionado pelo jogador.");
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