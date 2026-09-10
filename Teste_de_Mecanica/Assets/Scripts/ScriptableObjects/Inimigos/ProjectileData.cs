using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileData", menuName = "Projectiles/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    [Header("Identificação")]
    public string projectileName;

    [Header("Combate")]
    public int dano = 1;

    [Header("Movimento")]
    public float velocidade = 6f;
    public float tempoDeVida = 5f;

    [Header("Impacto")]
    public bool usarParticulaAoAtingir = false;
    public GameObject particulaImpacto;
}