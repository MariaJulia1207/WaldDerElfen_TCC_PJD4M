using UnityEngine;

public class CheckpointArea : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("CheckpointArea: OnTriggerEnter2D -> collider = " + other.name + " tag = " + other.tag);

        if (!other.CompareTag("Player"))
            return;

        Debug.Log("CheckpointArea: jogador entrou no checkpoint. Enviando ShowCheckpointButton.");
        ObserverManager.Notify("ShowCheckpointButton");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("CheckpointArea: OnTriggerExit2D -> collider = " + other.name + " tag = " + other.tag);

        if (!other.CompareTag("Player"))
            return;

        Debug.Log("CheckpointArea: jogador saiu do checkpoint. Enviando HideCheckpointButton.");
        ObserverManager.Notify("HideCheckpointButton");
    }
}
