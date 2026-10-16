using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    [Header("Componentes")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private MusicLibrary musicLibrary;
    [SerializeField] private Slider bgmSlider;

    private const string PlayerPrefKey = "bgmVolume";
    private readonly List<Slider> registeredSliders = new List<Slider>();
    private float currentVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            currentVolume = PlayerPrefs.HasKey(PlayerPrefKey) ? PlayerPrefs.GetFloat(PlayerPrefKey) : 1f;
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
        if (bgmSlider != null)
        {
            RegisterSlider(bgmSlider);
        }

        RegisterSceneSliders();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterSceneSliders();
    }

    private void RegisterSceneSliders()
    {
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

        for (int i = registeredSliders.Count - 1; i >= 0; i--)
        {
            var s = registeredSliders[i];
            if (s == null)
            {
                registeredSliders.RemoveAt(i);
                continue;
            }

            if (Mathf.Approximately(s.value, currentVolume)) continue;
            s.SetValueWithoutNotify(currentVolume);
        }
    }

    public static void Play(string musicName)
    {
        if (Instance == null)
        {
            Debug.LogError("BackgroundMusic: Nenhuma instância ativa encontrada.");
            return;
        }

        if (Instance.musicLibrary == null)
        {
            Debug.LogError("BackgroundMusic: MusicLibrary não atribuída no Inspector.", Instance);
            return;
        }

        AudioClip clip = Instance.musicLibrary.GetClip(musicName);
        if (clip == null)
        {
            Debug.LogWarning($"BackgroundMusic: Nenhum clipe encontrado para '{musicName}'.", Instance);
            return;
        }

        Instance.PlayLoop(clip);
    }

    public static void Play(AudioClip clip)
    {
        if (Instance == null)
        {
            Debug.LogError("BackgroundMusic: Nenhuma instância ativa encontrada.");
            return;
        }

        Instance.PlayLoop(clip);
    }

    public void PlayLoop(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        // Não reinicia a música se o mesmo clipe já estiver tocando
        if (audioSource.clip == clip && audioSource.isPlaying) return;

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