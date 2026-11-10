using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheckpointButtonController : MonoBehaviour
{
    [SerializeField] private GameObject checkpointButton;
    [SerializeField] private float delayBetweenHeals = 0.3f; // segundos entre cada vida adicionada

    private Coroutine healRoutine;

    private void OnEnable()
    {
        Debug.Log("CheckpointButtonController: OnEnable -> assinando eventos.");
        ObserverManager.Subscribe("ShowCheckpointButton", ShowButton);
        ObserverManager.Subscribe("HideCheckpointButton", HideButton);
    }

    private void OnDisable()
    {
        ObserverManager.Unsubscribe("ShowCheckpointButton", ShowButton);
        ObserverManager.Unsubscribe("HideCheckpointButton", HideButton);
    }

    private void Start()
    {
        Debug.Log("CheckpointButtonController: Start -> botão inicializado.");

        if (checkpointButton != null)
        {
            checkpointButton.SetActive(false);
        }
    }

    private void Update()
    {
        // Verifica o pressionamento da tecla E via Input System
        if (checkpointButton != null && checkpointButton.activeSelf)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                OnCheckpointPressed();
            }
        }
    }

    private void ShowButton()
    {
        Debug.Log("CheckpointButtonController: ShowButton foi chamado.");

        if (checkpointButton == null)
        {
            Debug.LogWarning("CheckpointButtonController: checkpointButton == null");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("CheckpointButtonController: Player não encontrado.");
            return;
        }

        HealthSystem health = player.GetComponent<HealthSystem>();
        if (health == null)
        {
            Debug.LogWarning("CheckpointButtonController: HealthSystem não encontrado no Player.");
            return;
        }

        Debug.Log("CheckpointButtonController: vida atual = " + health.vida + ", vida máxima = " + health.vidaMaxima);

        if (health.vida < health.vidaMaxima)
        {
            checkpointButton.SetActive(true);
            Debug.Log("CheckpointButtonController: botão ativado.");
        }
        else
        {
            Debug.Log("CheckpointButtonController: vida já está no máximo, botão não será exibido.");
        }
    }

    private void HideButton()
    {
        Debug.Log("CheckpointButtonController: HideButton foi chamado.");

        if (healRoutine != null)
        {
            StopCoroutine(healRoutine);
            healRoutine = null;
            Debug.Log("CheckpointButtonController: healRoutine interrompida.");
        }

        if (checkpointButton != null)
        {
            checkpointButton.SetActive(false);
            Debug.Log("CheckpointButtonController: botão ocultado.");
        }
    }

    public void OnCheckpointPressed()
    {
        Debug.Log("CheckpointButtonController: botão pressionado.");

        // evita iniciar múltiplas coroutines
        if (healRoutine != null)
        {
            Debug.Log("CheckpointButtonController: cura já em andamento.");
            return;
        }

        // Toca o efeito sonoro "Checkpoint" cadastrado no SoundEffectLibrary
        SoundEffectManager.Play("Checkpoint");

        healRoutine = StartCoroutine(HealAllOneByOne());
    }

    private IEnumerator HealAllOneByOne()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("CheckpointButtonController: Player não encontrado ao iniciar cura.");
            healRoutine = null;
            yield break;
        }

        HealthSystem health = player.GetComponent<HealthSystem>();
        if (health == null)
        {
            Debug.LogWarning("CheckpointButtonController: HealthSystem ausente no Player ao iniciar cura.");
            healRoutine = null;
            yield break;
        }

        Debug.Log("CheckpointButtonController: iniciando cura até o máximo.");

        while (health.vida < health.vidaMaxima)
        {
            health.ReceberCura(1);
            Debug.Log("CheckpointButtonController: adicionada 1 vida -> " + health.vida + "/" + health.vidaMaxima);
            yield return new WaitForSeconds(delayBetweenHeals);
        }

        Debug.Log("CheckpointButtonController: cura completa.");
        HideButton();
        healRoutine = null;
    }
}