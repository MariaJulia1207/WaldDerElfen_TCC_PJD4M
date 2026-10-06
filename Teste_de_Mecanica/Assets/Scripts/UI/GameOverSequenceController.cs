using UnityEngine;

public class GameOverSequenceController : MonoBehaviour
{
    [Header("Transição")]
    [SerializeField] private Animator animator;

    [Header("UI")]
    [SerializeField] private GameObject blackFade;
    [SerializeField] private GameObject gameOverPanel;

    private bool isHiding;
    private bool shouldRestart;
    private bool shouldGoToMenu;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        isHiding = false;
        shouldRestart = false;
        shouldGoToMenu = false;

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

    public void PlayHideAndRestart()
    {
        if (isHiding)
        {
            return;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        isHiding = true;
        shouldRestart = true;
        shouldGoToMenu = false;

        if (animator != null)
        {
            animator.SetTrigger("Hide");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartFromCheckpoint();
        }
    }

    public void PlayHideAndGoToMenu()
    {
        if (isHiding)
        {
            return;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        isHiding = true;
        shouldRestart = false;
        shouldGoToMenu = true;

        if (animator != null)
        {
            animator.SetTrigger("Hide");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.VoltarAoMenu();
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

        Time.timeScale = 1f;
    }
}
