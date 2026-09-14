using System;
using UnityEngine;

public class Heart : MonoBehaviour, IItem
{
    public static event Action<int> OnHeartCollect;

    [SerializeField] private int vidaParaAdicionar = 1;

    public void Collect()
    {
        HealthSystem playerHealth = FindObjectOfType<HealthSystem>();

        if (playerHealth != null)
        {
            playerHealth.ReceberCura(vidaParaAdicionar);
            OnHeartCollect?.Invoke(vidaParaAdicionar);
        }

        Destroy(gameObject);
    }
}
