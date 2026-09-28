using UnityEngine;
using Unity.Cinemachine;

public class MapTransition : MonoBehaviour
{
    [Header("Configuração da Área")]
    [SerializeField] private PolygonCollider2D mapBoundary;

    [Header("Direção da Transição")]
    [SerializeField] private Direction direction;

    [SerializeField] private float additivePos = 2f;

    private CinemachineConfiner2D confiner;

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private void Awake()
    {
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();

        if (confiner == null)
        {
            Debug.LogError(
                "MapTransition: nenhum CinemachineConfiner2D foi encontrado na cena."
            );
        }

        if (mapBoundary == null)
        {
            Debug.LogError(
                $"MapTransition em {gameObject.name}: nenhum Map Boundary foi definido."
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (confiner == null || mapBoundary == null)
            return;

        // Troca o limite da câmera
        confiner.BoundingShape2D = mapBoundary;

        // Atualiza a posição do jogador
        UpdatePlayerPosition(collision.gameObject);

        // Em algumas versões do Cinemachine, isso força
        // a atualização do Confiner imediatamente.
        confiner.InvalidateCache();
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        Vector3 newPos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                newPos.y += additivePos;
                break;

            case Direction.Down:
                newPos.y -= additivePos;
                break;

            case Direction.Left:
                newPos.x -= additivePos;
                break;

            case Direction.Right:
                newPos.x += additivePos;
                break;
        }

        player.transform.position = newPos;
    }
}
