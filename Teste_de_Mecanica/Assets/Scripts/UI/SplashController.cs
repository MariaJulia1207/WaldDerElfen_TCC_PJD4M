using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the Splash scene. Shows the splash image for a fixed duration and then
/// requests a transition to the Menu scene via GameManager.
/// </summary>
public class SplashController : MonoBehaviour
{
    [Tooltip("Tempo em segundos que a splash ficará visível")]
    public float displaySeconds = 2f;

    private IEnumerator Start()
    {
        // Wait for the UI to render
        yield return null;

        // Wait the configured duration
        yield return new WaitForSeconds(displaySeconds);

        // After the splash, force the transition to Menu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ForceSceneChange("Menu");
        }
        else
        {
            Debug.LogWarning("SplashController: GameManager not found. Attempting direct scene load.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }
}

