using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Cena de destino")]
    [SerializeField] private string cenaDestino;

    [Header("Spawn Point no destino")]
    [SerializeField] private string spawnPointDestino;

    private bool ativado;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ativado)
            return;

        if (!other.CompareTag("Player"))
            return;

        ativado = true;

        TransitionManager.Instance.Transition(
            cenaDestino,
            spawnPointDestino
        );
    }
}