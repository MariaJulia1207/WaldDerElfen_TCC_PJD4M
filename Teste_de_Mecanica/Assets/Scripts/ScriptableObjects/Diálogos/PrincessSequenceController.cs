using UnityEngine;
using UnityEngine.Playables;

public class PrincessSequenceController : MonoBehaviour
{
    [Header("Configuração da Cutscene")]
    [SerializeField] private PlayableDirector timelineDirector;
    [SerializeField] private string nomeCenaFinal = "CutsceneFinal";

    private void OnEnable()
    {
        if (timelineDirector != null)
        {
            // Inscreve a função no evento de fim da Timeline
            timelineDirector.stopped += OnTimelineEnded;
        }
    }

    private void OnDisable()
    {
        if (timelineDirector != null)
        {
            timelineDirector.stopped -= OnTimelineEnded;
        }
    }

    /// <summary>
    /// Chamado ao terminar o diálogo da Princesa (via UnityEvent do NPCInteractable)
    /// </summary>
    public void ExecutarCutscene()
    {
        // Bloqueia a movimentação do jogador
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.DisableControls();
        }

        if (timelineDirector != null)
        {
            timelineDirector.Play();
        }
    }

    /// <summary>
    /// Método disparado automaticamente no exato frame em que a Timeline chega ao fim
    /// </summary>
    private void OnTimelineEnded(PlayableDirector director)
    {
        if (LevelLoader.Instance != null)
        {
            LevelLoader.Instance.Transition(nomeCenaFinal);
        }
        else
        {
            Debug.LogError("[PrincessSequenceController] LevelLoader.Instance não foi encontrado na cena GUI!");
        }
    }
}