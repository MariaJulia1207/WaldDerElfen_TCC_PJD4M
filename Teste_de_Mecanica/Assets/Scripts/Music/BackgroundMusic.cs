using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Slider bgmSlider;

    private const string PlayerPrefKey = "bgmVolume";
    private List<Slider> registeredSliders = new List<Slider>();
    private float currentVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentVolume = PlayerPrefs.HasKey(PlayerPrefKey) ? PlayerPrefs.GetFloat(PlayerPrefKey) : 1f;
            if (audioSource != null) audioSource.volume = currentVolume;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (bgmSlider != null)
        {
            RegisterSlider(bgmSlider);
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
        // include inactive sliders; filter to those in loaded scenes
        Slider[] sliders = Resources.FindObjectsOfTypeAll<Slider>();
        foreach (var slider in sliders)
        {
            if (slider == null) continue;
            if (!slider.gameObject.scene.isLoaded) continue;

            if (slider.gameObject.name.Equals("bgmSlider") || slider.CompareTag("BgmSlider") || slider.CompareTag("bgmSlider"))
            {
                slider.interactable = true;
                RegisterSlider(slider);
            }
        }
    }

    public void RegisterSlider(Slider slider)
    {
        if (slider == null) return;
        if (registeredSliders.Contains(slider)) return;

        registeredSliders.Add(slider);
        if (bgmSlider == null)
        {
            bgmSlider = slider;
        }

        slider.SetValueWithoutNotify(currentVolume);
        slider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
        if (audioSource != null) audioSource.volume = currentVolume;

        PlayerPrefs.SetFloat(PlayerPrefKey, currentVolume);
        PlayerPrefs.Save();

        for (int i = 0; i < registeredSliders.Count; i++)
        {
            var s = registeredSliders[i];
            if (s == null) continue;
            if (Mathf.Approximately(s.value, currentVolume)) continue;
            s.SetValueWithoutNotify(currentVolume);
        }
    }

    public void PlayLoop(AudioClip clip)
    {
        if (audioSource == null) return;
        if (clip == null) return;

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void Stop()
    {
        if (audioSource == null) return;
        audioSource.Stop();
    }
}