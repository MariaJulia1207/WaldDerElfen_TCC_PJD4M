using UnityEngine;

public class SceneBGM : MonoBehaviour
{
    [Header("Configuração de Execução")]
    [Tooltip("Se marcado, tocará a música automaticamente no Start.")]
    [SerializeField] private bool playOnStart = true;

    [Header("Modo 1: Nome na MusicLibrary")]
    [Tooltip("Nome exato cadastrado na MusicLibrary do BackgroundMusic.")]
    [SerializeField] private string musicName;

    [Header("Modo 2: Clipe Direto (Testes Rápido)")]
    [Tooltip("Se preenchido, este AudioClip terá prioridade total sobre o nome da biblioteca.")]
    [SerializeField] private AudioClip testAudioClip;

    private void Start()
    {
        if (playOnStart)
        {
            PlayMusic();
        }
    }

    public void PlayMusic()
    {
        if (BackgroundMusic.Instance == null)
        {
            Debug.LogError("SceneBGM: Instância do BackgroundMusic não encontrada. Certifique-se de iniciar a partir da cena Boot.", this);
            return;
        }

        // Prioridade 1: Clipe direto (ideal para testes)
        if (testAudioClip != null)
        {
            BackgroundMusic.Play(testAudioClip);
            return;
        }

        // Prioridade 2: Nome na biblioteca
        if (!string.IsNullOrWhiteSpace(musicName))
        {
            BackgroundMusic.Play(musicName);
            return;
        }

        Debug.LogWarning("SceneBGM: Nem 'testAudioClip' nem 'musicName' foram configurados neste objeto.", this);
    }
}