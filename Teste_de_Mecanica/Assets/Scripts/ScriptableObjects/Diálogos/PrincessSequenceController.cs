using UnityEngine;
using UnityEngine.Playables;

public class PrincessSequenceController : MonoBehaviour
{
    [Header("Configurações da Timeline")]
    [SerializeField] private PlayableDirector timelineAbraco;

    [Header("Transição de Cena")]
    [SerializeField] private string nomeCenaCutsceneFinal = "CutsceneFinal";

    private void OnEnable()
    {
        if (timelineAbraco != null)
        {
            // Escuta o evento nativo de término da Timeline
            timelineAbraco.stopped += OnTimelineFinished;
        }
    }

    private void OnDisable()
    {
        if (timelineAbraco != null)
        {
            timelineAbraco.stopped -= OnTimelineFinished;
        }
    }

    /// <summary>
    /// Chamado pelo evento OnDialogueCompleted do NPCInteractable da Princesa.
    /// </summary>
    public void IniciarSequenciaAbraco()
    {
        // 1. Desativa os controles e movimentação do jogador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerController player = playerObj.GetComponent<PlayerController>();
            if (player != null)
            {
                player.DisableControls();
            }
        }

        // 2. Inicia a Timeline da animação do abraço
        if (timelineAbraco != null)
        {
            timelineAbraco.Play();
        }
        else
        {
            Debug.LogError("[PrincessSequenceController] PlayableDirector não foi atribuído no Inspector!", this);
            // Fallback direto caso a Timeline não esteja configurada
            CarregarCenaFinal();
        }
    }

    /// <summary>
    /// Disparado automaticamente assim que a Timeline conclui a reprodução.
    /// </summary>
    private void OnTimelineFinished(PlayableDirector director)
    {
        CarregarCenaFinal();
    }

    private void CarregarCenaFinal()
    {
        // 3. Transiciona para a cena final usando o LevelLoader
        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.Transition(nomeCenaCutsceneFinal);
        }
        else
        {
            Debug.LogError("[PrincessSequenceController] LevelLoader.Instance não foi encontrado na cena!", this);
        }
    }
}