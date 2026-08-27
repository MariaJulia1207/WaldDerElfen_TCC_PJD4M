using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target; // Arraste o Player aqui
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    private void LateUpdate()
    {
        if (target != null)
        {
            // Segue o player cravado no LateUpdate para evitar qualquer tremor
            transform.position = target.position + offset;
        }
    }
}
