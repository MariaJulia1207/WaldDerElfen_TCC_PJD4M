using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointArea : MonoBehaviour
{
    [Header("Spawn do checkpoint")]
    [SerializeField] private Transform spawnPoint;

    public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.AtualizarCheckpoint(SceneManager.GetActiveScene().name, SpawnPosition);
        }

        ObserverManager.Notify("ShowCheckpointButton");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        ObserverManager.Notify("HideCheckpointButton");
    }
}
