using UnityEngine;

public class GameOverSequenceController : MonoBehaviour
{
    [Header("Transição")]
    [SerializeField] private Animator animator;

    [Header("UI")]
    [SerializeField] private GameObject blackFade;
    [SerializeField] private GameObject gameOverPanel;

    private bool isHiding;

    private void Start()
    {
        isHiding = false;

        if (animator != null)
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.SetTrigger("Show");
        }

        if (blackFade != null)
        {
            blackFade.SetActive(true);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void PlayHide()
    {
        if (isHiding)
        {
            return;
        }

        isHiding = true;

        if (animator != null)
        {
            animator.SetTrigger("Hide");
        }
    }

    public void OnHideComplete()
    {
        if (blackFade != null)
        {
            blackFade.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
}
