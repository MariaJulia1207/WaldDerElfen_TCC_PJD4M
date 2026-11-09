using UnityEngine;

public class BossArena : MonoBehaviour
{
    [Header("Configuração da Arena")]
    [SerializeField] private GameObject aberturaEntrada;
    [SerializeField] private GameObject aberturaSaida;

    [Header("Boss")]
    [SerializeField] private GameObject bossObjeto;
    [SerializeField] private string saveId;
    private IBoss boss;

    [Header("Objetos Pós-Derrota (Previamente colocados na Cena)")]
    [Tooltip("Objeto dos Cristais pré-posicionado na cena (deve começar desativado no Inspector).")]
    [SerializeField] private GameObject cristaisObjeto;

    [Tooltip("Objeto da Árvore Curada ou outro NPC pré-posicionado na cena (opcional).")]
    [SerializeField] private GameObject arvoreCuradaNPC;

    [Header("Trilha Sonora")]
    [SerializeField] private string bossMusicName;
    [SerializeField] private AudioClip bossMusicClip;

    [Space(5)]
    [SerializeField] private string areaMusicName;
    [SerializeField] private AudioClip areaMusicClip;

    [SerializeField] private float musicFadeDuration = 1.5f;

    [Header("Objeto de Teste - Opcional")]
    [SerializeField] private GameObject objetoTeste;

    private bool combateIniciado = false;
    private bool combateFinalizado = false;

    public string SaveId => string.IsNullOrEmpty(saveId) ? gameObject.name : saveId;
    public bool IsDefeated => combateFinalizado;

    private void Awake()
    {
        if (bossObjeto != null)
            boss = bossObjeto.GetComponent<IBoss>();
    }

    private void Update()
    {
        if (combateIniciado && !combateFinalizado)
        {
            if (objetoTeste != null && objetoTeste == null)
            {
                BossDerrotado();
                return;
            }

            if (bossObjeto == null)
            {
                BossDerrotado();
                return;
            }

            if (boss != null && boss.EstaMorto())
            {
                BossDerrotado();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (combateIniciado)
            return;

        if (!other.CompareTag("Player"))
            return;

        IniciarCombate();
    }

    private void IniciarCombate()
    {
        combateIniciado = true;

        if (aberturaEntrada != null)
            aberturaEntrada.SetActive(true);

        if (aberturaSaida != null)
            aberturaSaida.SetActive(true);

        if (boss != null)
            boss.IniciarBoss();

        TocarMusicaBoss();

        Debug.Log("Boss Arena: Combate iniciado!");
    }

    public void ApplySaveState(bool defeated)
    {
        combateFinalizado = defeated;

        if (defeated)
        {
            combateIniciado = true;

            if (bossObjeto != null)
                bossObjeto.SetActive(false);

            if (aberturaEntrada != null)
                aberturaEntrada.SetActive(false);

            if (aberturaSaida != null)
                aberturaSaida.SetActive(false);

            AtivarObjetosPosDerrota();
        }
        else
        {
            combateIniciado = false;

            if (bossObjeto != null)
                bossObjeto.SetActive(true);

            if (aberturaEntrada != null)
                aberturaEntrada.SetActive(false);

            if (aberturaSaida != null)
                aberturaSaida.SetActive(false);

            // Se o jogo recarregar e a luta não estiver vencida, garante que os cristais fiquem ocultos
            DesativarObjetosPosDerrota();

            if (boss != null)
            {
                boss.ResetarBoss();
            }
        }
    }

    public void BossDerrotado()
    {
        if (combateFinalizado)
            return;

        combateFinalizado = true;

        if (aberturaEntrada != null)
            aberturaEntrada.SetActive(false);

        if (aberturaSaida != null)
            aberturaSaida.SetActive(false);

        AtivarObjetosPosDerrota();
        TocarMusicaArea();

        Debug.Log("Boss Arena: Boss derrotado! Cristais ativados na cena.");
    }

    public void AtivarObjetosPosDerrota()
    {
        if (cristaisObjeto != null)
        {
            cristaisObjeto.SetActive(true);
        }

        if (arvoreCuradaNPC != null)
        {
            arvoreCuradaNPC.SetActive(true);
        }
    }

    private void DesativarObjetosPosDerrota()
    {
        if (cristaisObjeto != null)
        {
            cristaisObjeto.SetActive(false);
        }

        if (arvoreCuradaNPC != null)
        {
            arvoreCuradaNPC.SetActive(false);
        }
    }

    private void TocarMusicaBoss()
    {
        if (bossMusicClip != null)
        {
            BackgroundMusic.CrossfadeTo(bossMusicClip, musicFadeDuration);
        }
        else if (!string.IsNullOrWhiteSpace(bossMusicName))
        {
            BackgroundMusic.CrossfadeTo(bossMusicName, musicFadeDuration);
        }
    }

    private void TocarMusicaArea()
    {
        if (areaMusicClip != null)
        {
            BackgroundMusic.CrossfadeTo(areaMusicClip, musicFadeDuration);
        }
        else if (!string.IsNullOrWhiteSpace(areaMusicName))
        {
            BackgroundMusic.CrossfadeTo(areaMusicName, musicFadeDuration);
        }
    }
}