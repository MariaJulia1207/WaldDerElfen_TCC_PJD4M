using UnityEngine;

public class RoomTransition : MonoBehaviour
{
    [Header("Câmera")]
    [SerializeField] private PolygonCollider2D novoBoundary;

    [Header("Entrada da Sala")]
    [SerializeField] private Transform pontoEntrada;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        PlayerController player = collision.GetComponent<PlayerController>();

        if (player == null)
            return;

        CameraManager cameraManager =
            FindFirstObjectByType<CameraManager>();

        if (cameraManager != null)
        {
            cameraManager.MudarBoundary(novoBoundary);
        }

        if (pontoEntrada != null)
        {
            player.transform.position = pontoEntrada.position;
        }
    }
}