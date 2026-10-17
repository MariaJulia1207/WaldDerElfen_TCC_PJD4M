using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    [Header("Componentes")]
    [SerializeField] private AudioSource primaryAudioSource;
    [SerializeField] private AudioSource secondaryAudioSource;
    [SerializeField] private MusicLibrary musicLibrary;
    [SerializeField] private Slider bgmSlider;

    private const string PlayerPrefKey = "bgmVolume";
    private readonly List<Slider> registeredSliders = new List<Slider>();

    private float currentVolume = 1f;
    private AudioSource activeSource;
    private AudioSource inactiveSource;
    private Coroutine crossfadeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupAudioSources();

            currentVolume = PlayerPrefs.HasKey(PlayerPrefKey) ? PlayerPrefs.GetFloat(PlayerPrefKey) : 1f;
            SetVolume(currentVolume);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        primaryAudioSource = sources.Length >= 1 ? sources[0] : gameObject.AddComponent<AudioSource>();
        secondaryAudioSource = sources.Length >= 2 ? sources[1] : gameObject.AddComponent<AudioSource>();

        primaryAudioSource.loop = true;
        primaryAudioSource.playOnAwake = false;

        secondaryAudioSource.loop = true;
        secondaryAudioSource.playOnAwake = false;

        activeSource = primaryAudioSource;
        inactiveSource = secondaryAudioSource;
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
            if (slider == null || !slider.gameObject.scene.isLoaded) continue;

            if (slider.gameObject.name.Equals("bgmSlider") || slider.CompareTag("BgmSlider") || slider.CompareTag("bgmSlider"))
            {
                slider.interactable = true;
                RegisterSlider(slider);
            }
        }
    }

    public void RegisterSlider(Slider slider)
    {
        if (slider == null || registeredSliders.Contains(slider)) return;

        registeredSliders.Add(slider);
        if (bgmSlider == null) bgmSlider = slider;

        slider.SetValueWithoutNotify(currentVolume);
        slider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);

        if (crossfadeCoroutine == null)
        {
            if (activeSource != null) activeSource.volume = currentVolume;
            if (inactiveSource != null) inactiveSource.volume = 0f;
        }

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
        CrossfadeTo(musicName, 1.5f);
    }

    public static void Play(AudioClip clip)
    {
        CrossfadeTo(clip, 1.5f);
    }

    public static void CrossfadeTo(string musicName, float fadeDuration = 1.5f)
    {
        if (Instance == null) return;

        if (Instance.musicLibrary == null)
        {
            Debug.LogError("BackgroundMusic: MusicLibrary não atribuída no Inspector.", Instance);
            return;
        }

        AudioClip clip = Instance.musicLibrary.GetClip(musicName);
        if (clip != null)
        {
            Instance.CrossfadeToInternal(clip, fadeDuration);
        }
        else
        {
            Debug.LogWarning($"BackgroundMusic: Música '{musicName}' não encontrada na biblioteca.", Instance);
        }
    }

    public static void CrossfadeTo(AudioClip clip, float fadeDuration = 1.5f)
    {
        if (Instance == null) return;
        Instance.CrossfadeToInternal(clip, fadeDuration);
    }

    public void PlayLoop(AudioClip clip)
    {
        CrossfadeToInternal(clip, 0.5f);
    }

    private void CrossfadeToInternal(AudioClip newClip, float fadeDuration)
    {
        if (newClip == null) return;

        // Se o novo clipe já estiver tocando na fonte ativa, não interrompe
        if (activeSource.clip == newClip && activeSource.isPlaying) return;

        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
        }

        crossfadeCoroutine = StartCoroutine(CrossfadeRoutine(newClip, fadeDuration));
    }

    private IEnumerator CrossfadeRoutine(AudioClip newClip, float fadeDuration)
    {
        inactiveSource.clip = newClip;
        inactiveSource.volume = 0f;
        inactiveSource.Play();

        float timer = 0f;
        float startActiveVolume = activeSource.volume;

        if (fadeDuration <= 0f) fadeDuration = 0.01f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            inactiveSource.volume = Mathf.Lerp(0f, currentVolume, progress);
            activeSource.volume = Mathf.Lerp(startActiveVolume, 0f, progress);

            yield return null;
        }

        activeSource.Stop();
        activeSource.volume = 0f;
        inactiveSource.volume = currentVolume;

        // Alterna as fontes: a inativa torna-se a nova fonte ativa
        AudioSource temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;

        crossfadeCoroutine = null;
    }

    public void Stop(float fadeOutDuration = 1.0f)
    {
        if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
        StartCoroutine(FadeOutRoutine(fadeOutDuration));
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float startVolume = activeSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            activeSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        activeSource.Stop();
        inactiveSource.Stop();
        activeSource.volume = 0f;
        inactiveSource.volume = 0f;
    }
}