using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance { get; private set; }

    [SerializeField] private SoundEffectLibrary soundEffectLibrary;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
            sfxSlider.onValueChanged.AddListener(SetVolume);
            SetVolume(sfxSlider.value);
        }
    }

    public static void Play(string soundName)
    {
        if (Instance == null)
            return;

        AudioClip clip = Instance.soundEffectLibrary.GetRandomClip(soundName);

        if (clip != null)
        {
            Instance.audioSource.PlayOneShot(clip);
        }
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
}