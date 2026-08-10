using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Alvo")]
    public Transform player;

    [Header("Configurações")]
    public float velocidade = 5f;

    [Header("Offset")]
    public Vector3 offset;

    void LateUpdate()
    {
        if (player == null)
            return;

        // Posição desejada
        Vector3 posicaoDesejada = player.position + offset;

        // Mantém câmera no eixo Z
        posicaoDesejada.z = -10f;

        // Movimento suave
        transform.position = Vector3.Lerp(
            transform.position,
            posicaoDesejada,
            velocidade * Time.deltaTime
        );
    }
}