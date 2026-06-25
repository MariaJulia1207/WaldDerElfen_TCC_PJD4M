using UnityEngine;

public class InteractButtonController : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonVisual;

    private void OnEnable()
    {
        ObserverManager.Subscribe(
            "ShowInteractButton",
            ShowButton);

        ObserverManager.Subscribe(
            "HideInteractButton",
            HideButton);
    }

    private void OnDisable()
    {
        ObserverManager.Unsubscribe(
            "ShowInteractButton",
            ShowButton);

        ObserverManager.Unsubscribe(
            "HideInteractButton",
            HideButton);
    }

    private void Start()
    {
        buttonVisual.SetActive(false);
    }

    private void ShowButton()
    {
        buttonVisual.SetActive(true);
    }

    private void HideButton()
    {
        buttonVisual.SetActive(false);
    }

    public void OnInteractPressed()
    {
        InteractOM.Instance.Interact();
    }
}