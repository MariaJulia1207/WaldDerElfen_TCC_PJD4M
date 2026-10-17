using UnityEngine;

public class AdaptiveMusicZone : MonoBehaviour
{
    [Header("Configuração de Detecção")]
    [Tooltip("Tag do jogador necessária para disparar a troca de música.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Modo 1: Nome na MusicLibrary")]
    [Tooltip("Nome exato da música na MusicLibrary (ex: Floresta, Aldeia, Caverna).")]
    [SerializeField] private string musicName;

    [Header("Modo 2: Clipe Direto (Testes Rápidos)")]
    [Tooltip("Se preenchido, este AudioClip terá prioridade total sobre o nome da biblioteca.")]
    [SerializeField] private AudioClip testAudioClip;

    [Header("Transição")]
    [Tooltip("Tempo em segundos para realizar a transição suave (Crossfade).")]
    [SerializeField] private float fadeDuration = 2.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            TriggerMusicChange();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            TriggerMusicChange();
        }
    }

    public void TriggerMusicChange()
    {
        if (testAudioClip != null)
        {
            BackgroundMusic.CrossfadeTo(testAudioClip, fadeDuration);
        }
        else if (!string.IsNullOrWhiteSpace(musicName))
        {
            BackgroundMusic.CrossfadeTo(musicName, fadeDuration);
        }
        else
        {
            Debug.LogWarning("AdaptiveMusicZone: Nenhuma música definida nesta zona.", this);
        }
    }
}