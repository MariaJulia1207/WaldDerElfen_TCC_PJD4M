
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance { get; private set; }

    [Header("Biblioteca")]
    [SerializeField] private SoundEffectLibrary soundEffectLibrary;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource voiceAudioSource;

    [Header("Volume")]
    [SerializeField] private Slider sfxSlider;

    [Header("Diagnóstico")]
    [SerializeField] private bool mostrarLogsDeVoz = true;

    private const string PlayerPrefKey = "sfxVolume";

    private readonly List<Slider> registeredSliders =
        new List<Slider>();

    private readonly Dictionary<string, float> speakerPitches =
        new Dictionary<string, float>(
            StringComparer.OrdinalIgnoreCase);

    // Intervalos atuais compartilhados por personagem.
    private readonly Dictionary<string, Vector2> speakerRanges =
        new Dictionary<string, Vector2>(
            StringComparer.OrdinalIgnoreCase);

    private float currentVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        currentVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(PlayerPrefKey, 1f));

        ConfigurarAudioSources();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void ConfigurarAudioSources()
    {
        // Recupera o AudioSource principal, usado pelos efeitos sonoros.
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Garante que o canal de voz tenha um AudioSource separado.
        if (voiceAudioSource == null || voiceAudioSource == audioSource)
        {
            AudioSource[] sources = GetComponents<AudioSource>();

            voiceAudioSource = null;

            foreach (AudioSource source in sources)
            {
                if (source != audioSource)
                {
                    voiceAudioSource = source;
                    break;
                }
            }

            if (voiceAudioSource == null)
                voiceAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configura os efeitos sonoros.
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.pitch = 1f;

        // Configura a voz.
        voiceAudioSource.playOnAwake = false;
        voiceAudioSource.loop = false;
        voiceAudioSource.pitch = 1f;
        voiceAudioSource.mute = false;
        voiceAudioSource.spatialBlend = 0f;
        voiceAudioSource.dopplerLevel = 0f;
        voiceAudioSource.volume = currentVolume;
        voiceAudioSource.priority = 64;

        // Mantém os dois canais no mesmo grupo do Audio Mixer, se houver.
        voiceAudioSource.outputAudioMixerGroup =
            audioSource.outputAudioMixerGroup;
    }
    private void Start()
    {
        if (sfxSlider != null)
            RegisterSlider(sfxSlider);

        RegisterSceneSliders();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (Instance == this)
            Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterSceneSliders();
    }

    private void RegisterSceneSliders()
    {
        Slider[] sliders =
            Resources.FindObjectsOfTypeAll<Slider>();

        foreach (Slider slider in sliders)
        {
            if (slider == null ||
                !slider.gameObject.scene.isLoaded)
                continue;

            if (slider.gameObject.name.Equals("sfxSlider") ||
                slider.CompareTag("SfxSlider"))
            {
                slider.interactable = true;
                RegisterSlider(slider);
            }
        }
    }

    public void RegisterSlider(Slider slider)
    {
        if (slider == null ||
            registeredSliders.Contains(slider))
            return;

        registeredSliders.Add(slider);

        if (sfxSlider == null)
            sfxSlider = slider;

        slider.SetValueWithoutNotify(currentVolume);
        slider.onValueChanged.AddListener(SetVolume);
    }

    // =====================================================
    // EFEITOS SONOROS NORMAIS
    // =====================================================

    public static void Play(string soundName)
    {
        if (Instance == null)
            return;

        if (Instance.soundEffectLibrary == null ||
            Instance.audioSource == null)
        {
            Debug.LogError(
                "SoundEffectManager: biblioteca ou AudioSource " +
                "dos efeitos não configurado.");
            return;
        }

        AudioClip clip =
            Instance.soundEffectLibrary.GetRandomClip(soundName);

        if (clip == null)
        {
            Debug.LogWarning(
                "SoundEffectManager: nenhum clipe encontrado para '" +
                soundName + "'.");
            return;
        }

        Instance.audioSource.pitch = 1f;
        Instance.audioSource.PlayOneShot(clip);
    }

    // =====================================================
    // VOZES
    // =====================================================

    public static void PlayWithPitch(
        string soundName,
        float pitch)
    {
        if (Instance == null)
        {
            Debug.LogError(
                "SoundEffectManager: não existe uma instância ativa.");
            return;
        }

        Instance.ReproduzirVoz(soundName, pitch);
    }

    private void ReproduzirVoz(string soundName, float pitch)
    {
        if (soundEffectLibrary == null)
        {
            Debug.LogError(
                "SoundEffectManager: SoundEffectLibrary não atribuída.",
                this);
            return;
        }

        if (voiceAudioSource == null ||
            voiceAudioSource == audioSource)
        {
            Debug.LogError(
                "SoundEffectManager: canal de voz inválido.",
                this);
            return;
        }

        AudioClip clip =
            soundEffectLibrary.GetRandomClip(soundName);

        if (clip == null)
        {
            Debug.LogError(
                "Voz não reproduzida: GetRandomClip não encontrou " +
                "um clipe para '" + soundName + "'. " +
                "Confira o nome exato na SoundEffectLibrary.",
                this);
            return;
        }

        if (!voiceAudioSource.gameObject.activeInHierarchy ||
            !voiceAudioSource.isActiveAndEnabled)
        {
            Debug.LogError(
                "Voz não reproduzida: o GameObject ou AudioSource " +
                "do canal de voz está inativo.",
                this);
            return;
        }

        voiceAudioSource.mute = false;
        voiceAudioSource.volume = currentVolume;
        voiceAudioSource.spatialBlend = 0f;
        voiceAudioSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f);

        voiceAudioSource.PlayOneShot(clip);

        if (mostrarLogsDeVoz)
        {
            Debug.Log(
                "Voz solicitada: " + soundName +
                " | Clipe: " + clip.name +
                " | Pitch: " + voiceAudioSource.pitch +
                " | Volume: " + voiceAudioSource.volume +
                " | Duração: " + clip.length +
                " | Reproduzindo: " + voiceAudioSource.isPlaying,
                this);
        }
    }

    // Atualiza o intervalo e sorteia novamente apenas
    // quando o perfil de voz é alterado.
    public static void PlayVoiceForSpeaker(
        string soundName,
        string speakerName,
        float pitchMin,
        float pitchMax)
    {
        if (Instance == null)
            return;

        if (string.IsNullOrWhiteSpace(speakerName))
        {
            PlayWithPitch(
                soundName,
                UnityEngine.Random.Range(
                    Mathf.Min(pitchMin, pitchMax),
                    Mathf.Max(pitchMin, pitchMax)));
            return;
        }

        string key = speakerName.Trim();

        Vector2 newRange = new Vector2(
            Mathf.Clamp(Mathf.Min(pitchMin, pitchMax), 0.1f, 3f),
            Mathf.Clamp(Mathf.Max(pitchMin, pitchMax), 0.1f, 3f));

        bool intervaloAlterado =
            !Instance.speakerRanges.TryGetValue(
                key, out Vector2 oldRange) ||
            !Mathf.Approximately(oldRange.x, newRange.x) ||
            !Mathf.Approximately(oldRange.y, newRange.y);

        if (intervaloAlterado)
        {
            Instance.speakerRanges[key] = newRange;

            Instance.speakerPitches[key] =
                UnityEngine.Random.Range(
                    newRange.x, newRange.y);
        }

        PlayWithPitch(
            soundName,
            Instance.speakerPitches[key]);
    }

    public static void ResetVoicePitch()
    {
        if (Instance == null ||
            Instance.voiceAudioSource == null)
            return;

        Instance.voiceAudioSource.pitch = 1f;
    }

    public static void StopVoice()
    {
        if (Instance == null || Instance.voiceAudioSource == null)
            return;

        Instance.voiceAudioSource.Stop();
        Instance.voiceAudioSource.pitch = 1f;
    }

    // =====================================================
    // VOLUME
    // =====================================================

    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);

        if (audioSource != null)
            audioSource.volume = currentVolume;

        if (voiceAudioSource != null)
            voiceAudioSource.volume = currentVolume;

        PlayerPrefs.SetFloat(PlayerPrefKey, currentVolume);
        PlayerPrefs.Save();

        for (int i = registeredSliders.Count - 1; i >= 0; i--)
        {
            Slider slider = registeredSliders[i];

            if (slider == null)
            {
                registeredSliders.RemoveAt(i);
                continue;
            }

            slider.SetValueWithoutNotify(currentVolume);
        }
    }
}