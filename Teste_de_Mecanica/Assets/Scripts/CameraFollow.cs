using UnityEngine;
using Unity.Cinemachine;

public class CameraFollowPlayer : MonoBehaviour
{
    private CinemachineCamera cameraCinemachine;

    private void Awake()
    {
        cameraCinemachine = GetComponent<CinemachineCamera>();
    }

    private void Update()
    {
        if (cameraCinemachine.Follow == null)
        {
            EncontrarPlayer();
        }
    }

    private void EncontrarPlayer()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        cameraCinemachine.Follow = player.transform;
    }
}