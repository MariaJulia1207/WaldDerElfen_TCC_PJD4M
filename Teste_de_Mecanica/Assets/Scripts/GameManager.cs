using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("GUI")]
    [SerializeField] private string nomeCenaGUI = "GUI";

    [Header("Cenas de Gameplay")]
    [SerializeField] private List<string> cenasGameplay = new List<string>
    {
        "Level1",
        "Level2"
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
        Gameplay
    }

    public GameState EstadoAtual { get; private set; }

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
    }

    // =========================================================
    // CONTROLE DA GUI
    // =========================================================

    private void AtualizarGUI(string nomeCena)
    {
        // -----------------------------------------
        // CENA DE GAMEPLAY
        // -----------------------------------------

        if (cenasGameplay.Contains(nomeCena))
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
        SceneManager.LoadScene(sceneName);
    }

    public void RequestSceneChange(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
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