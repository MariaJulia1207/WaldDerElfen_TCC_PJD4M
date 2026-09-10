using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identificação")]
    public string enemyName;

    [Header("Vida")]
    public int vidaMaxima = 3;

    [Header("Movimento")]
    public float velocidade = 3.5f;

    [Header("Ataque")]
    public int danoContato = 1;
}