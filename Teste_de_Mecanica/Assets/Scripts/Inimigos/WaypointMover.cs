using System.Collections;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    public Transform waypointParent;
    public float moveSpeed = 2f;
    public float waitTime = 2f;
    public bool loopWaypoints = true;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    void Start()
    {
        waypoints = new Transform[waypointParent.childCount];

        for (int i = 0; i < waypointParent.childCount; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);
        }
    }

    void Update()
    {
        if (isWaiting || waypoints.Length == 0)
            return;

        MoveToWaypoint();
    }

    void MoveToWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];

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
    }
}