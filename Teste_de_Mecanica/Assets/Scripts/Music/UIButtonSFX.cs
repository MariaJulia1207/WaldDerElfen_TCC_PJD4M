using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [Header("Nomes na SoundEffectLibrary")]
    [Tooltip("Nome do som ao clicar no botão.")]
    [SerializeField] private string clickSoundName = "ButtonClick";

    [Tooltip("Nome do som ao passar o mouse por cima (deixe em branco se não quiser som).")]
    [SerializeField] private string hoverSoundName = "";

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Não toca o som se o botão estiver desativado/desabilitado
        if (button != null && !button.interactable) return;

        if (!string.IsNullOrWhiteSpace(clickSoundName))
        {
            SoundEffectManager.Play(clickSoundName);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Não toca o som se o botão estiver desativado/desabilitado
        if (button != null && !button.interactable) return;

        if (!string.IsNullOrWhiteSpace(hoverSoundName))
        {
            SoundEffectManager.Play(hoverSoundName);
        }
    }
}