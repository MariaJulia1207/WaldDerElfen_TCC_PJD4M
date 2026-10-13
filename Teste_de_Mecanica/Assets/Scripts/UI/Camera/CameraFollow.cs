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

    public void DesativarSeguido()
    {
        if (cameraCinemachine != null)
        {
            cameraCinemachine.Follow = null;
        }
    }

    public void ReativarSeguido(Transform alvo)
    {
        if (cameraCinemachine == null)
        {
            cameraCinemachine = GetComponent<CinemachineCamera>();
        }

        if (cameraCinemachine != null && alvo != null)
        {
            cameraCinemachine.Follow = alvo;
            SnapToTarget(alvo);
        }
    }

    public void SnapToTarget(Transform alvo)
    {
        if (cameraCinemachine == null)
        {
            cameraCinemachine = GetComponent<CinemachineCamera>();
        }

        if (cameraCinemachine == null || alvo == null)
        {
            return;
        }

        Vector3 novaPosicao = cameraCinemachine.transform.position;
        novaPosicao.x = alvo.position.x;
        novaPosicao.y = alvo.position.y;
        cameraCinemachine.transform.position = novaPosicao;
    }
}