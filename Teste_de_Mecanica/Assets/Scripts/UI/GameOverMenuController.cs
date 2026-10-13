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
    [SerializeField] private int tempoFade = 1;

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

        GameObject taggedFader = GameObject.FindGameObjectWithTag("ScreenFader");
        if (taggedFader != null)
        {
            ScreenFader foundFader = taggedFader.GetComponent<ScreenFader>();
            if (foundFader != null)
            {
                return foundFader;
            }
        }

        ScreenFader objectInScene = FindAnyObjectByType<ScreenFader>(FindObjectsInactive.Include);
        if (objectInScene != null)
        {
            return objectInScene;
        }

        GameObject faderObject = new GameObject("ScreenFader", typeof(Canvas), typeof(CanvasGroup), typeof(ScreenFader));
        faderObject.tag = "ScreenFader";

        Canvas canvas = faderObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasGroup canvasGroupRef = faderObject.GetComponent<CanvasGroup>();
        canvasGroupRef.alpha = 0f;
        canvasGroupRef.blocksRaycasts = false;
        canvasGroupRef.interactable = false;

        DontDestroyOnLoad(faderObject);

        return faderObject.GetComponent<ScreenFader>();
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup targetCanvasGroup, float targetAlpha, float duration)
    {
        if (targetCanvasGroup == null)
        {
            yield break;
        }

        float startAlpha = targetCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            targetCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        targetCanvasGroup.alpha = targetAlpha;
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
        gameOverCanvas.interactable = false;
        gameOverCanvas.blocksRaycasts = false;

        if (restartButton != null)
        {
            restartButton.interactable = false;
        }

        if (menuButton != null)
        {
            menuButton.interactable = false;
        }

        StartCoroutine(ShowGameOverRoutine());
    }

    private IEnumerator ShowGameOverRoutine()
    {
        ScreenFader fader = GetScreenFader();

        if (fader != null)
        {
            yield return StartCoroutine(fader.FadeTo(1f));
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.alpha = 0f;
            gameOverCanvas.interactable = true;
            gameOverCanvas.blocksRaycasts = true;

            if (restartButton != null)
            {
                restartButton.interactable = true;
            }

            if (menuButton != null)
            {
                menuButton.interactable = true;
            }

            yield return StartCoroutine(FadeCanvasGroup(gameOverCanvas, 1f, tempoFade));
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
        bool checkpointAlreadyRestored = false;

        if (restartButton != null)
        {
            restartButton.interactable = false;
        }

        if (menuButton != null)
        {
            menuButton.interactable = false;
        }

        if (restart && CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint)
        {
            CheckpointManager.Instance.RestaurarPlayerNoCheckpoint(false);
            checkpointAlreadyRestored = true;
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.interactable = false;
            gameOverCanvas.blocksRaycasts = false;
            gameOverCanvas.alpha = 1f;
            yield return StartCoroutine(FadeCanvasGroup(gameOverCanvas, 0f, tempoFade));
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

        if (restart && !checkpointAlreadyRestored && GameManager.Instance != null)
        {
            GameManager.Instance.RestartFromCheckpoint();
        }
        else if (!restart && GameManager.Instance != null)
        {
            GameManager.Instance.VoltarAoMenu();
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(false);
        }

        Time.timeScale = 1f;
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