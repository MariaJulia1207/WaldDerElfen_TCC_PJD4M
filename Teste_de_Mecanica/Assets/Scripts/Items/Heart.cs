using System;
using UnityEngine;

public class Heart : MonoBehaviour, IItem
{
    public static event Action<int> OnHeartCollect;

    [SerializeField] private int vidaParaAdicionar = 1;

    private BounceEffect bounceEffect;

    private void Awake()
    {
        bounceEffect = GetComponent<BounceEffect>();
    }

    private void Start()
    {
        if (bounceEffect != null)
        {
            bounceEffect.StartBounce();
        }
    }

    public void Collect()
    {
        HealthSystem playerHealth = FindObjectOfType<HealthSystem>();

        if (playerHealth != null)
        {
            playerHealth.ReceberCura(vidaParaAdicionar);
            OnHeartCollect?.Invoke(vidaParaAdicionar);
        }

        SoundEffectManager.Play("HeartCollect");

        Destroy(gameObject);
    }
}