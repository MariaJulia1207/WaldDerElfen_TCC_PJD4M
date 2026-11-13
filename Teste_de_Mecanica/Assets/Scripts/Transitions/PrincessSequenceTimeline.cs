using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class PrincessCutsceneTransition : MonoBehaviour
{
    [Header("Navegação")]
    [SerializeField] private string nomeCenaCutsceneFinal = "CutsceneFinal";

    [Header("Automação via Timeline (Opcional)")]
    [SerializeField] private PlayableDirector timelinePrincesa;

    [Header("Interação da Princesa")]
    [Tooltip("Arraste o Collider2D de interação da Princesa para desativá-lo ao fim do diálogo.")]
    [SerializeField] private Collider2D colisorInteracaoPrincesa;

    private void OnEnable()
    {
        if (timelinePrincesa != null)
        {
            timelinePrincesa.stopped += OnTimelineStopped;
        }
    }

    private void OnDisable()
    {
        if (timelinePrincesa != null)
        {
            timelinePrincesa.stopped -= OnTimelineStopped;
        }
    }

    /// <summary>
    /// Chame este método assim que o diálogo com a princesa terminar (ou no início da Timeline).
    /// </summary>
    public void EsconderBotaoInteracao()
    {
        if (colisorInteracaoPrincesa != null)
        {
            colisorInteracaoPrincesa.enabled = false;
        }

        // Ajuste o nome do método de disparo conforme o seu ObserverManager (ex: Notify, Publish ou Trigger)
        ObserverManager.Notify("HideInteractButton");
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        EsconderBotaoInteracao();
        IrParaCutsceneFinal();
    }

    public void IrParaCutsceneFinal()
    {
        EsconderBotaoInteracao();

        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.Transition(nomeCenaCutsceneFinal);
        }
        else
        {
            SceneManager.LoadScene(nomeCenaCutsceneFinal);
        }
    }
}