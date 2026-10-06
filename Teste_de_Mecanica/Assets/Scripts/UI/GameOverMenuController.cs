using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverMenuController : MonoBehaviour
{
    public static GameOverMenuController Instance;

    [Header("Painel de Game Over")]
    [SerializeField] private CanvasGroup gameOverCanvas;

    [Header("Botões")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    [Header("Fade")]
    [SerializeField] private ScreenFader screenFader;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (gameOverCanvas == null)
        {
            gameOverCanvas = GetComponentInChildren<CanvasGroup>();
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(OnMenuClicked);
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.alpha = 0f;
            gameOverCanvas.interactable = false;
            gameOverCanvas.blocksRaycasts = false;
            gameOverCanvas.gameObject.SetActive(false);
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

    private ScreenFader GetScreenFader()
    {
        if (screenFader != null)
        {
            return screenFader;
        }

        if (ScreenFader.Instance != null)
        {
            return ScreenFader.Instance;
        }

        return FindAnyObjectByType<ScreenFader>();
    }

    public void ShowGameOver()
    {
        if (gameOverCanvas == null || isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        Time.timeScale = 0f;

        gameOverCanvas.gameObject.SetActive(true);
        gameOverCanvas.alpha = 0f;
        gameOverCanvas.interactable = true;
        gameOverCanvas.blocksRaycasts = true;

        StartCoroutine(ShowGameOverRoutine());
    }

    private IEnumerator ShowGameOverRoutine()
    {
        ScreenFader fader = GetScreenFader();

        if (fader != null)
        {
            yield return StartCoroutine(fader.FadeTo(1f));
        }
        else if (gameOverCanvas != null)
        {
            gameOverCanvas.alpha = 1f;
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.alpha = 1f;
        }

        isTransitioning = false;
    }

    public void HideGameOverAndRestart()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(HideGameOverRoutine(true));
    }

    public void HideGameOverAndMenu()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(HideGameOverRoutine(false));
    }

    private IEnumerator HideGameOverRoutine(bool restart)
    {
        isTransitioning = true;

        if (restartButton != null)
        {
            restartButton.interactable = false;
        }

        if (menuButton != null)
        {
            menuButton.interactable = false;
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.interactable = false;
            gameOverCanvas.blocksRaycasts = false;
        }

        ScreenFader fader = GetScreenFader();

        if (fader != null)
        {
            yield return StartCoroutine(fader.FadeTo(0f));
        }
        else if (gameOverCanvas != null)
        {
            gameOverCanvas.alpha = 0f;
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(false);
        }

        Time.timeScale = 1f;

        if (restart && GameManager.Instance != null)
        {
            GameManager.Instance.RestartFromCheckpoint();
        }
        else if (!restart && GameManager.Instance != null)
        {
            GameManager.Instance.VoltarAoMenu();
        }

        isTransitioning = false;
    }

    public void OnRestartClicked()
    {
        HideGameOverAndRestart();
    }

    public void OnMenuClicked()
    {
        HideGameOverAndMenu();
    }
}