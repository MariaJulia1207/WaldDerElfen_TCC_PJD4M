using UnityEngine;

public class CheckpointButtonController : MonoBehaviour
{
    [SerializeField] private GameObject checkpointButton;

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

        if (checkpointButton != null)
        {
            checkpointButton.SetActive(false);
            Debug.Log("CheckpointButtonController: botão ocultado.");
        }
    }

    public void OnCheckpointPressed()
    {
        Debug.Log("CheckpointButtonController: botão pressionado.");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("CheckpointButtonController: Player não encontrado ao pressionar botão.");
            return;
        }

        HealthSystem health = player.GetComponent<HealthSystem>();
        if (health == null)
        {
            Debug.LogWarning("CheckpointButtonController: HealthSystem ausente no Player ao pressionar botão.");
            return;
        }

        Debug.Log("CheckpointButtonController: vida antes = " + health.vida + ", cura +1");

        if (health.vida >= health.vidaMaxima)
        {
            HideButton();
            return;
        }

        health.ReceberCura(1);
        Debug.Log("CheckpointButtonController: vida depois = " + health.vida);

        if (health.vida >= health.vidaMaxima)
        {
            HideButton();
        }
    }
}
