using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class PrincessCutsceneTransition : MonoBehaviour
{
    [Header("Navegação")]
    [Tooltip("Nome exato da cena da Cutscene Final (conforme cadastrado no Build Settings).")]
    [SerializeField] private string nomeCenaCutsceneFinal = "CutsceneFinal";

    [Header("Automação via Timeline (Opcional)")]
    [Tooltip("Se arrastar a PlayableDirector da Princesa aqui, a transição ocorrerá automaticamente no fim da animação sem precisar de Signals.")]
    [SerializeField] private PlayableDirector timelinePrincesa;

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

    private void OnTimelineStopped(PlayableDirector director)
    {
        IrParaCutsceneFinal();
    }

    /// <summary>
    /// Método público para ser chamado por um Signal Receiver na Timeline ou por um UnityEvent.
    /// </summary>
    public void IrParaCutsceneFinal()
    {
        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.Transition(nomeCenaCutsceneFinal);
        }
        else
        {
            Debug.LogWarning("PrincessCutsceneTransition: LevelLoader.Instance não encontrado na cena! Usando SceneManager como fallback.", this);
            SceneManager.LoadScene(nomeCenaCutsceneFinal);
        }
    }
}