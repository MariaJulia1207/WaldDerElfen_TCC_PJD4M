using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleCutsceneController : MonoBehaviour
{
    [System.Serializable]
    public class PersonagemRoda
    {
        public GameObject transformObjeto;
        public ParticleSystem efeitoDeseitiço; // Efeito individual ao "voltar ao normal"
        [HideInInspector] public float anguloAtual;
        [HideInInspector] public bool estaParado = false;
    }

    [Header("Configurações do Círculo")]
    [SerializeField] private Transform centroDoCirculo;
    [SerializeField] private float raio = 3.5f;
    [SerializeField] private float velocidadeRotacao = 90f; // Graus por segundo

    [Header("Personagens")]
    [SerializeField] private List<PersonagemRoda> personagens;

    [Header("Efeitos Gerais")]
    [SerializeField] private ParticleSystem estrelinhasFundo; // Particle system das estrelas piscando

    private bool _rodaAtiva = false;

    private void Start()
    {
        OrganizarPersonagensNoCirculo();
    }

    private void Update()
    {
        if (!_rodaAtiva) return;

        AtualizarPosicoes();
    }

    // Distribui os personagens igualmente ao redor do centro no início
    public void OrganizarPersonagensNoCirculo()
    {
        if (personagens == null || personagens.Count == 0) return;

        float incrementoAngulo = 360f / personagens.Count;

        for (int i = 0; i < personagens.Count; i++)
        {
            personagens[i].anguloAtual = i * incrementoAngulo;
            AtualizarPosicaoPersonagem(personagens[i]);
        }
    }

    public void IniciarRoda()
    {
        _rodaAtiva = true;

        if (estrelinhasFundo != null && !estrelinhasFundo.isPlaying)
        {
            estrelinhasFundo.Play();
        }
    }

    private void AtualizarPosicoes()
    {
        Vector3 posCentro = centroDoCirculo != null ? centroDoCirculo.position : transform.position;

        foreach (var p in personagens)
        {
            if (p.estaParado) continue; // Se já voltou ao normal, para de girar

            p.anguloAtual += velocidadeRotacao * Time.deltaTime;

            // Mantém o ângulo entre 0 e 360
            if (p.anguloAtual >= 360f) p.anguloAtual -= 360f;

            AtualizarPosicaoPersonagem(p);
        }
    }

    private void AtualizarPosicaoPersonagem(PersonagemRoda p)
    {
        if (p.transformObjeto == null) return;

        Vector3 posCentro = centroDoCirculo != null ? centroDoCirculo.position : transform.position;

        // Converte o ângulo para Radianos para as funções matemáticas
        float rad = p.anguloAtual * Mathf.Deg2Rad;

        float x = posCentro.x + Mathf.Cos(rad) * raio;
        float y = posCentro.y + Mathf.Sin(rad) * raio;

        // Atualiza a posição mantendo a ROTAÇÃO ZERADA (sempre de pé)
        p.transformObjeto.transform.position = new Vector3(x, y, p.transformObjeto.transform.position.z);
        p.transformObjeto.transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// Chama esta função para fazer um personagem específico parar e voltar ao normal.
    /// Pode ser chamada via Timeline (Signal) ou por Corrotina sequencial.
    /// </summary>
    public void PararEVoltarAoNormal(int indice)
    {
        if (indice < 0 || indice >= personagens.Count) return;

        PersonagemRoda p = personagens[indice];
        if (p.estaParado) return;

        p.estaParado = true;

        // Toca o efeito individual no personagem
        if (p.efeitoDeseitiço != null)
        {
            p.efeitoDeseitiço.Play();
        }

        // Caso tenha uma troca de sprite/animação ao voltar ao normal, acione aqui:
        // p.transformObjeto.GetComponent<Animator>().SetTrigger("Desfeitiçado");
    }

    public void PararTodasEstrelasFundo()
    {
        if (estrelinhasFundo != null)
        {
            estrelinhasFundo.Stop();
        }
    }
}