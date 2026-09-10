using System.Collections.Generic;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    [SerializeField] private string _tagTargetDetection = "Player";

    public List<Collider2D> detectedObjs = new List<Collider2D>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(_tagTargetDetection))
        {
            if (!detectedObjs.Contains(collision))
            {
                detectedObjs.Add(collision);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (detectedObjs.Contains(collision))
        {
            detectedObjs.Remove(collision);
        }
    }
}