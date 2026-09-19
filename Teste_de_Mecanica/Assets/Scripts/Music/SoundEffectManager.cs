using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance { get; private set; }

    [SerializeField] private SoundEffectLibrary soundEffectLibrary;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Slider sfxSlider;

    private const string PlayerPrefKey = "sfxVolume";

    private List<Slider> registeredSliders = new List<Slider>();

    private float currentVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentVolume = PlayerPrefs.HasKey(PlayerPrefKey)
                ? PlayerPrefs.GetFloat(PlayerPrefKey)
                : 1f;

            if (audioSource != null)
                audioSource.volume = currentVolume;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (sfxSlider != null)
        {
            RegisterSlider(sfxSlider);
        }

        RegisterSceneSliders();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterSceneSliders();
    }

    private void RegisterSceneSliders()
    {
        Slider[] sliders =
            Resources.FindObjectsOfTypeAll<Slider>();

        foreach (var slider in sliders)
        {
            if (slider == null)
                continue;

            // Ignora prefabs e assets
            if (!slider.gameObject.scene.isLoaded)
                continue;

            // Procura pelo nome ou pela tag
            if (
                slider.gameObject.name.Equals("sfxSlider") ||
                slider.CompareTag("SfxSlider")
            )
            {
                slider.interactable = true;

                RegisterSlider(slider);
            }
        }
    }

    public void RegisterSlider(Slider slider)
    {
        if (slider == null)
            return;

        if (registeredSliders.Contains(slider))
            return;

        registeredSliders.Add(slider);

        if (sfxSlider == null)
        {
            sfxSlider = slider;
        }

        slider.SetValueWithoutNotify(currentVolume);

        slider.onValueChanged.AddListener(SetVolume);
    }

    // =========================================================
    // SONS NORMAIS
    // =========================================================

    public static void Play(string soundName)
    {
        if (Instance == null)
            return;

        AudioClip clip =
            Instance.soundEffectLibrary.GetRandomClip(soundName);

        if (clip != null)
        {
            Instance.audioSource.PlayOneShot(clip);
        }
    }

    // =========================================================
    // SONS COM PITCH
    // =========================================================

    public static void PlayWithPitch(
        string soundName,
        float pitch)
    {
        if (Instance == null)
            return;

        AudioClip clip =
            Instance.soundEffectLibrary.GetRandomClip(soundName);

        if (clip != null)
        {
            Instance.audioSource.pitch = pitch;

            Instance.audioSource.PlayOneShot(clip);

            // Volta ao pitch normal depois de configurar o som
            Instance.audioSource.pitch = 1f;
        }
    }

    // =========================================================
    // VOLUME
    // =========================================================

    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);

        if (audioSource != null)
            audioSource.volume = currentVolume;

        PlayerPrefs.SetFloat(
            PlayerPrefKey,
            currentVolume);

        PlayerPrefs.Save();

        // Atualiza todos os sliders registrados
        // sem disparar novamente o evento.
        for (int i = 0; i < registeredSliders.Count; i++)
        {
            var s = registeredSliders[i];

            if (s == null)
                continue;

            if (Mathf.Approximately(
                s.value,
                currentVolume))
                continue;

            s.SetValueWithoutNotify(
                currentVolume);
        }
    }
}