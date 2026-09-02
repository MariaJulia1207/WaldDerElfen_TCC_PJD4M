using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("GUI")]
    [SerializeField] private string nomeCenaGUI = "GUI";

    private bool guiCarregada = false;

    public enum GameState
    {
        Iniciando,
        MenuPrincipal,
        Gameplay
    }

    public GameState EstadoAtual { get; private set; }

    private void Awake()
    {
        // Evita GameManagers duplicados
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // Detecta quando uma nova cena terminou de carregar
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private void Start()
    {
        string cenaAtual = SceneManager.GetActiveScene().name;

        DefinirEstado(cenaAtual);

        // Se começou diretamente no _Boot,
        // vai para a Splash
        if (cenaAtual == "_Boot")
        {
            ForceSceneChange("Splash");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
        }
    }

    // =========================================================
    // CENA CARREGADA
    // =========================================================

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        DefinirEstado(cena.name);

        // Quando entrar em um nível,
        // garante que a GUI esteja carregada
        if (cena.name == "Level1" || cena.name == "Level2")
        {
            CarregarGUI();
        }
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
    // ESTADO
    // =========================================================

    private void DefinirEstado(string nomeCena)
    {
        if (nomeCena == "_Boot" || nomeCena == "Splash")
        {
            EstadoAtual = GameState.Iniciando;
        }
        else if (nomeCena == "Menu")
        {
            EstadoAtual = GameState.MenuPrincipal;
        }
        else
        {
            EstadoAtual = GameState.Gameplay;
        }
    }

    // =========================================================
    // GUI
    // =========================================================

    private void CarregarGUI()
    {
        if (guiCarregada)
            return;

        SceneManager.LoadScene(nomeCenaGUI, LoadSceneMode.Additive);

        guiCarregada = true;
    }
}