using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    public string LastCheckpointScene { get; private set; }
    public Vector3 LastCheckpointPosition { get; private set; }
    public bool HasCheckpoint => !string.IsNullOrEmpty(LastCheckpointScene);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void AtualizarCheckpoint(string sceneName, Vector3 position)
    {
        LastCheckpointScene = sceneName;
        LastCheckpointPosition = position;
    }

    public void RestaurarPlayerNoCheckpoint()
    {
        if (GameManager.Instance != null)
        {
            string nomeCenaGameOver = GameManager.Instance.NomeCenaGameOver;
            Scene cenaGameOver = SceneManager.GetSceneByName(nomeCenaGameOver);

            if (cenaGameOver.isLoaded)
            {
                SceneManager.UnloadSceneAsync(nomeCenaGameOver);
            }
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        CameraFollowPlayer cameraFollow = FindAnyObjectByType<CameraFollowPlayer>();

        if (cameraFollow != null)
        {
            cameraFollow.DesativarSeguido();
        }

        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.DisableControls();
            }

            player.transform.position = LastCheckpointPosition;

            HealthSystem health = player.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.RestaurarParaCheckpoint();
            }
        }

        if (cameraFollow != null && player != null)
        {
            cameraFollow.ReativarSeguido(player.transform);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!HasCheckpoint || scene.name != LastCheckpointScene)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            player.transform.position = LastCheckpointPosition;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
