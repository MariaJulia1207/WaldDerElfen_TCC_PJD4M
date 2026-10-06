using UnityEngine;
using UnityEngine.UI;

public class GameOverMenuController : MonoBehaviour
{
    [Header("Botões")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private void Awake()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuClicked);
        }
    }

    private void OnDestroy()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(OnMenuClicked);
        }
    }

    public void OnRestartClicked()
    {
        GameOverSequenceController sequenceController = FindAnyObjectByType<GameOverSequenceController>();

        if (sequenceController != null)
        {
            sequenceController.PlayHideAndRestart();
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartFromCheckpoint();
        }
    }

    public void OnMenuClicked()
    {
        GameOverSequenceController sequenceController = FindAnyObjectByType<GameOverSequenceController>();

        if (sequenceController != null)
        {
            sequenceController.PlayHideAndGoToMenu();
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.VoltarAoMenu();
        }
    }
}
