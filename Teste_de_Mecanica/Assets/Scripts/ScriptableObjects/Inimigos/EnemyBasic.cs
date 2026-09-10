using UnityEngine;

public class EnemyBasic : MonoBehaviour
{
    [Header("Dados")]
    [SerializeField] private EnemyData enemyData;

    [Header("IA")]
    [SerializeField] private EnemyAI detectionArea;

    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Vector2 enemyDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (enemyData != null)
        {
            moveSpeed = enemyData.velocidade;
        }
    }

    private void FixedUpdate()
    {
        if (detectionArea == null || detectionArea.Target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Transform target = detectionArea.Target;

        float directionX = target.position.x - transform.position.x;

        if (Mathf.Abs(directionX) > 0.1f)
        {
            enemyDirection = new Vector2(
                Mathf.Sign(directionX),
                0
            );
        }

        rb.linearVelocity = enemyDirection * moveSpeed;

        AtualizarVisual();
    }

    private void AtualizarVisual()
    {
        if (enemyDirection.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (enemyDirection.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }
}
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBasic : MonoBehaviour
{
    public float _moveSpeed = 3.5;
    private Vector2 enemyDirection;
    private Rigidbody2D rb;
    public EnemyAI _detectionArea;
    private SpriteRenderer _spriteRenderer;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        enemyDirection = 
        new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
    private void FixedUpdate()
    {
        if(_detectionArea.detectedObjs.Count > 0)
        {
            enemyDirection = 
            (_detectionArea.detectedObjs[0].transform.position - transform.position).normalized;
            rb.MovePosition(rb.position + enemyDirection * _moveSpeed * Time.fixedDeltaTime)
            if(enemyDirection.x > 0)
            {
                _spriteRenderer.flipX = false;
            }
            else
            {
                _spriteRenderer.flipX = true;
            }
        }
        
    }
}

{    
    // Variáveis do inimigo
    [Header("Componentes")]
    public EnemyObject enemyData;
    public GameObject player;
    public float distanciaAtaque = 1.5f;
    // Variáveis de ataque em top-down
    [Header("TriggerDamage")] 
    [SerializeField] private TriggerDamage triggerDamage;
    [SerializeField] private GameObject hitboxAtaqueUP;
    [SerializeField] private GameObject hitboxAtaqueDOWN;
    [SerializeField] private GameObject hitboxAtaqueLEFT;
    [SerializeField] private GameObject hitboxAtaqueRIGHT;
    // Feedback de dano
    [Header("Feedback")]
    [SerializeField] private ControladorFeedBackDano feedbackDano;
    
    public void ReceberDano(int dano)
    {
        enemyData.vida -= dano;

        Debug.Log(enemyData.enemyName + " recebeu " + dano + " de dano.");

        // Feedback visual
        if (feedbackDano != null)
        {
            feedbackDano.ExecutarFeedback();
        }

        if (enemyData.vida <= 0)
        {
            Morrer();
        }
    }
    
    public void Morrer()
    {
        Destroy(gameObject);
    }
    
    
    */