using UnityEngine;

public class AreaEntrance : MonoBehaviour
{
    [SerializeField] private string transitionName;

    private void Start()
    {
        if (SceneManagement.SceneTransitionName == transitionName)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                player.transform.position = transform.position;

                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }

            // Consome a transição para não reutilizá-la
            SceneManagement.ConsumeTransitionName();
        }
    }
}