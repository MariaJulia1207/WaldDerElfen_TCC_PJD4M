using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("GUI")]
    [SerializeField] private string nomeCenaGUI = "GUI";

    [Header("Game Over")]
    [SerializeField] private string nomeCenaGameOver = "GameOver";
    public string NomeCenaGameOver => nomeCenaGameOver;

    [Header("Cenas de Gameplay")]
    [SerializeField] private List<string> cenasGameplay = new List<string>
    {
        "Level1",
        "Level2",
        "Level3"
    };

    [Header("Cenas sem GUI")]
    [SerializeField] private List<string> cenasSemGUI = new List<string>
    {
        "Splash",
        "Menu"
    };

    public enum GameState
    {
        Iniciando,
        MenuPrincipal,
        Gameplay,
        GameOver
    }

    public GameState EstadoAtual { get; private set; }

    private bool shouldResetPlayerOnLoad;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += AoCarregarCena;
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        string cenaAtual = SceneManager.GetActiveScene().name;

        DefinirEstado(cenaAtual);

        // Se começou diretamente no Boot,
        // vai para Splash
        if (cenaAtual == "_Boot")
        {
            ForceSceneChange("Splash");
        }
        else
        {
            AtualizarGUI(cenaAtual);
        }
    }

    // =========================================================
    // QUANDO UMA CENA TERMINA DE CARREGAR
    // =========================================================

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        string nomeCena = cena.name;

        Debug.Log("GameManager: cena carregada = " + nomeCena);

        DefinirEstado(nomeCena);

        AtualizarGUI(nomeCena);

        if (shouldResetPlayerOnLoad)
        {
            shouldResetPlayerOnLoad = false;
            StartCoroutine(ResetPlayerAfterSceneLoad());
        }
    }

    private System.Collections.IEnumerator ResetPlayerAfterSceneLoad()
    {
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            yield break;
        }

        HealthSystem health = player.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.ResetToFullHealth();
        }

        LifeHUD[] huds = FindObjectsByType<LifeHUD>(FindObjectsSortMode.None);
        foreach (LifeHUD hud in huds)
        {
            if (hud != null)
            {
                hud.gameObject.SetActive(true);
            }
        }
    }

    // =========================================================
    // CONTROLE DA GUI
    // =========================================================

    private void AtualizarGUI(string nomeCena)
    {
        // -----------------------------------------
        // CENAS QUE DEVEM EXIBIR A GUI
        // -----------------------------------------

        if (nomeCena == nomeCenaGameOver || cenasGameplay.Contains(nomeCena))
        {
            CarregarGUI();
            return;
        }

        // -----------------------------------------
        // OUTRAS CENAS
        // -----------------------------------------

        if (cenasSemGUI.Contains(nomeCena))
        {
            DescarregarGUI();
        }
    }

    // =========================================================
    // CARREGAR GUI
    // =========================================================

    private void CarregarGUI()
    {
        Scene guiScene = SceneManager.GetSceneByName(nomeCenaGUI);

        // Se já estiver carregada, não faz nada
        if (guiScene.isLoaded)
        {
            return;
        }

        Debug.Log("GameManager: carregando GUI.");

        SceneManager.LoadScene(nomeCenaGUI, LoadSceneMode.Additive);
    }

    // =========================================================
    // DESCARREGAR GUI
    // =========================================================

    private void DescarregarGUI()
    {
        Scene guiScene = SceneManager.GetSceneByName(nomeCenaGUI);

        if (!guiScene.isLoaded)
        {
            return;
        }

        Debug.Log("GameManager: descarregando GUI.");

        SceneManager.UnloadSceneAsync(nomeCenaGUI);
    }

    // =========================================================
    // TROCA DE CENA
    // =========================================================

    public void ForceSceneChange(string sceneName)
    {
        if (TryUseLevelLoader(sceneName))
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void RequestSceneChange(string sceneName)
    {
        if (TryUseLevelLoader(sceneName))
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private bool TryUseLevelLoader(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (string.Equals(currentScene, sceneName, System.StringComparison.Ordinal))
        {
            return false;
        }

        if (cenasGameplay.Contains(sceneName) || sceneName == nomeCenaGameOver)
        {
            return false;
        }

        LevelLoader levelLoader = FindAnyObjectByType<LevelLoader>(FindObjectsInactive.Include);
        if (levelLoader == null)
        {
            return false;
        }

        levelLoader.Transition(sceneName);
        return true;
    }

    public void RestartFromCheckpoint()
    {
        Time.timeScale = 1f;

        if (CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint)
        {
            CheckpointManager.Instance.RestoreCheckpointFromAutoSave(false);
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(currentScene) && cenasGameplay.Contains(currentScene))
        {
            shouldResetPlayerOnLoad = true;
            SceneManager.LoadScene(currentScene);
            return;
        }

        if (cenasGameplay.Count > 0)
        {
            shouldResetPlayerOnLoad = true;
            SceneManager.LoadScene(cenasGameplay[0]);
            return;
        }

        shouldResetPlayerOnLoad = false;
        SceneManager.LoadScene("Menu");
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        ForceSceneChange("Menu");
    }

    // =========================================================
    // ESTADO DO JOGO
    // =========================================================

    private void DefinirEstado(string nomeCena)
    {
        if (nomeCena == "_Boot" ||
            nomeCena == "Splash")
        {
            EstadoAtual = GameState.Iniciando;
        }
        else if (nomeCena == "Menu")
        {
            EstadoAtual = GameState.MenuPrincipal;
        }
        else if (nomeCena == nomeCenaGameOver)
        {
            EstadoAtual = GameState.GameOver;
        }
        else if (cenasGameplay.Contains(nomeCena))
        {
            EstadoAtual = GameState.Gameplay;
        }
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
        }
    }
}