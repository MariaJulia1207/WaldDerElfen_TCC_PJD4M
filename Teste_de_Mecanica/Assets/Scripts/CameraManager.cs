using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cameraVirtual;
    [SerializeField] private CinemachineConfiner2D confiner;

    public void MudarBoundary(PolygonCollider2D novoBoundary)
    {
        if (novoBoundary == null)
        {
            Debug.LogWarning("Novo Boundary não foi definido.");
            return;
        }

        confiner.BoundingShape2D = novoBoundary;

        confiner.InvalidateBoundingShapeCache();
    }
}