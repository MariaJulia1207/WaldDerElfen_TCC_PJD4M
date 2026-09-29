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
