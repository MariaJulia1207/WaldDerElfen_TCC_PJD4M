using UnityEngine;

[CreateAssetMenu(
    fileName = "New EnemyObject", 
    menuName = "Scriptable Objects/New Enemy Object")]
public class EnemyObject : ScriptableObject
{
    [Header("Variáveis do Inimigo")]
    public string enemyName;
    public int vida;
    public float velocidade;
    public float ataque;
    
}
