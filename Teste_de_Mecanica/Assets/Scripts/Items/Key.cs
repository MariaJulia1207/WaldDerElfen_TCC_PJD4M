using UnityEngine;

public class Key : MonoBehaviour, IItem
{
    [SerializeField] private string saveId;
    [SerializeField] private Door targetDoor;

    private void Start()
    {
        // Se a chave já foi coletada neste save, destrói o objeto ao carregar a cena
        if (!string.IsNullOrEmpty(saveId) && SaveManager.Instance != null)
        {
            if (SaveManager.Instance.IsCoinCollected(saveId))
            {
                Destroy(gameObject);
            }
        }
    }

    public void Collect()
    {
        // Registra a chave como coletada no SaveManager
        if (!string.IsNullOrEmpty(saveId) && SaveManager.Instance != null)
        {
            SaveManager.Instance.MarkCoinCollected(saveId);
        }

        if (targetDoor != null)
        {
            targetDoor.OpenDoor();
        }
        SoundEffectManager.Play("HeartCollect");
        Destroy(gameObject);
    }
}