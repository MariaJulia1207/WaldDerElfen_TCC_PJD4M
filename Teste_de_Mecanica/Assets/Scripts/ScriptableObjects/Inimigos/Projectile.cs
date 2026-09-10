using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Dados")]
    [SerializeField] private ProjectileData projectileData;

    private Vector2 direcao;

    public void Inicializar(Vector2 novaDirecao)
    {
        direcao = novaDirecao.normalized;

        Destroy(gameObject, projectileData.tempoDeVida);
    }

    private void Update()
    {
        transform.Translate(
            direcao * projectileData.velocidade * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        HealthSystem health = other.GetComponent<HealthSystem>();

        if (health != null)
        {
            health.ReceberDano(projectileData.dano);
        }

        CriarParticula();

        Destroy(gameObject);
    }

    private void CriarParticula()
    {
        if (!projectileData.usarParticulaAoAtingir)
            return;

        if (projectileData.particulaImpacto == null)
            return;

        Instantiate(
            projectileData.particulaImpacto,
            transform.position,
            Quaternion.identity
        );
    }
}