using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenuController : MonoBehaviour
{
    public static GameOverMenuController Instance;

    [Header("Painel de Game Over")]
    [SerializeField] private CanvasGroup gameOverCanvas;

    [Header("Botões")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

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

    public void ShowGameOver()
    {
        if (gameOverCanvas == null || isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        Time.timeScale = 0f;

        gameOverCanvas.gameObject.SetActive(true);
        gameOverCanvas.alpha = 1f;
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

        // Garante que o player e o HUD saiam do game over com a vida completa
        // antes da cena ser recarregada.
        if (restart)
        {
            StartCoroutine(ForcePlayerVisibleAfterRestart());
        }

        if (restart && CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint)
        {
            CheckpointManager.Instance.RestoreCheckpointFromAutoSave(false);
            checkpointAlreadyRestored = true;
        }

        if (gameOverCanvas != null)
        {
            gameOverCanvas.interactable = false;
            gameOverCanvas.blocksRaycasts = false;
            gameOverCanvas.alpha = 0f;
            gameOverCanvas.gameObject.SetActive(false);
        }

        if (restart && !checkpointAlreadyRestored && GameManager.Instance != null)
        {
            GameManager.Instance.RestartFromCheckpoint();
        }
        else if (!restart && GameManager.Instance != null)
        {
            GameManager.Instance.VoltarAoMenu();
        }

        Time.timeScale = 1f;
        isTransitioning = false;
        yield break;
    }

    private IEnumerator ForcePlayerVisibleAfterRestart()
    {
        const float waitTimeout = 0.25f;
        float elapsed = 0f;

        while (elapsed < waitTimeout)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                SpriteRenderer[] renderers = player.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (SpriteRenderer renderer in renderers)
                {
                    if (renderer == null)
                    {
                        continue;
                    }

                    renderer.enabled = true;
                    Color color = renderer.color;
                    color.a = 1f;
                    renderer.color = color;
                }

                HealthSystem health = player.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.RestaurarParaCheckpoint();
                }

                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
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