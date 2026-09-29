using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.AtualizarCheckpoint(SceneManager.GetActiveScene().name, other.transform.position);
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
