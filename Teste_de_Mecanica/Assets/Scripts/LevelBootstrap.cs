using UnityEngine;

public class LevelBootstrap : MonoBehaviour
{
    [Header("Game Over Canvas")]
    [SerializeField] private GameObject gameOverCanvasPrefab;
    [SerializeField] private bool autoSpawnOnStart = true;

    private void Start()
    {
        if (autoSpawnOnStart)
        {
            EnsureGameOverCanvas();
        }
    }

    [ContextMenu("Ensure Game Over Canvas")]
    public void EnsureGameOverCanvas()
    {
        if (GameOverMenuController.Instance != null)
        {
            return;
        }

        if (gameOverCanvasPrefab == null)
        {
            Debug.LogWarning("LevelBootstrap: prefab do GameOverCanvas não foi atribuído.");
            return;
        }

        GameObject spawned = Instantiate(gameOverCanvasPrefab, transform);
        spawned.name = gameOverCanvasPrefab.name;

        GameOverMenuController controller = spawned.GetComponent<GameOverMenuController>();
        if (controller == null)
        {
            Debug.LogWarning("LevelBootstrap: prefab do GameOverCanvas não possui GameOverMenuController.");
        }
    }
}
