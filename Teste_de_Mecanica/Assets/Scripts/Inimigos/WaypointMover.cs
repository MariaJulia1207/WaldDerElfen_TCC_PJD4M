using System.Collections;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    public Transform waypointParent;
    public float moveSpeed = 2f;
    public float waitTime = 2f;
    public bool loopWaypoints = true;

    public bool IsWaiting => isWaiting;
    public bool IsPatrolActive => isPatrolActive;
    public Vector2 CurrentMoveDirection { get; private set; } = Vector2.down;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private bool isPatrolActive = true;

    void Start()
    {
        if (waypointParent == null)
            return;

        waypoints = new Transform[waypointParent.childCount];

        for (int i = 0; i < waypointParent.childCount; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);
        }

        AtualizarDirecaoMovimento();
    }

    void Update()
    {
        if (!isPatrolActive || isWaiting || waypoints == null || waypoints.Length == 0)
            return;

        MoveToWaypoint();
    }

    public void PausePatrol()
    {
        isPatrolActive = false;
        isWaiting = false;
    }

    public void ResumePatrol()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        isPatrolActive = true;
        isWaiting = false;
        AtualizarDirecaoMovimento();
    }

    void MoveToWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform target = waypoints[currentWaypointIndex];

        if (target == null)
            return;

        Vector2 direcao = ((Vector2)target.position - (Vector2)transform.position).normalized;
        CurrentMoveDirection = NormalizarDirecao(direcao);

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target.position) <= 0.01f)
        {
            transform.position = target.position;
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        yield return new WaitForSeconds(waitTime);

        if (currentWaypointIndex < waypoints.Length - 1)
        {
            currentWaypointIndex++;
        }
        else
        {
            if (loopWaypoints)
            {
                currentWaypointIndex = 0;
            }
            else
            {
                yield break;
            }
        }

        isWaiting = false;
        AtualizarDirecaoMovimento();
    }

    private void AtualizarDirecaoMovimento()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform target = waypoints[currentWaypointIndex];

        if (target == null)
            return;

        Vector2 direcao = ((Vector2)target.position - (Vector2)transform.position).normalized;
        CurrentMoveDirection = NormalizarDirecao(direcao);
    }

    private Vector2 NormalizarDirecao(Vector2 direcao)
    {
        if (Mathf.Abs(direcao.x) > Mathf.Abs(direcao.y))
        {
            return direcao.x >= 0 ? Vector2.right : Vector2.left;
        }

        if (direcao.y != 0)
        {
            return direcao.y >= 0 ? Vector2.up : Vector2.down;
        }

        return Vector2.down;
    }
}