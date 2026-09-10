using System.Collections.Generic;
using UnityEngine;

public class SoundEffectLibrary : MonoBehaviour
{
    [SerializeField] private SoundEffectGroup[] soundEffectGroups;

    private Dictionary<string, List<AudioClip>> soundDictionary;

    private void Awake()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        soundDictionary = new Dictionary<string, List<AudioClip>>();

        foreach (SoundEffectGroup group in soundEffectGroups)
        {
            if (!soundDictionary.ContainsKey(group.name))
            {
                soundDictionary.Add(group.name, group.audioClips);
            }
        }
    }

    public AudioClip GetRandomClip(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            List<AudioClip> clips = soundDictionary[soundName];

            if (clips != null && clips.Count > 0)
            {
                return clips[Random.Range(0, clips.Count)];
            }
        }

        return null;
    }
}