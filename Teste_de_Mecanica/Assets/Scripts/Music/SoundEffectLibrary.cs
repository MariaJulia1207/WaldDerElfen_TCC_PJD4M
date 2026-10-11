
using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectLibrary : MonoBehaviour
{
    [SerializeField]
    private SoundEffectGroup[] soundEffectGroups;

    private Dictionary<string, List<AudioClip>> soundDictionary;

    private void Awake()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        soundDictionary =
            new Dictionary<string, List<AudioClip>>(
                StringComparer.OrdinalIgnoreCase);

        if (soundEffectGroups == null)
        {
            Debug.LogError(
                "SoundEffectLibrary: a lista de grupos não foi configurada.",
                this);
            return;
        }

        foreach (SoundEffectGroup group in soundEffectGroups)
        {
            if (ReferenceEquals(group, null))
            {
                Debug.LogWarning(
                    "SoundEffectLibrary: existe um grupo nulo.",
                    this);
                continue;
            }

            if (string.IsNullOrWhiteSpace(group.name))
            {
                Debug.LogWarning(
                    "SoundEffectLibrary: existe um grupo sem nome.",
                    this);
                continue;
            }

            if (group.audioClips == null || group.audioClips.Count == 0)
            {
                Debug.LogWarning(
                    $"SoundEffectLibrary: o grupo '{group.name}' não tem clipes.",
                    this);
                continue;
            }

            if (soundDictionary.ContainsKey(group.name))
            {
                Debug.LogWarning(
                    $"SoundEffectLibrary: nome duplicado '{group.name}'.",
                    this);
                continue;
            }

            soundDictionary.Add(group.name, group.audioClips);
        }
    }

    public AudioClip GetRandomClip(string soundName)
    {
        if (soundDictionary == null)
            InitializeDictionary();

        if (string.IsNullOrWhiteSpace(soundName))
            return null;

        if (!soundDictionary.TryGetValue(soundName, out List<AudioClip> clips))
            return null;

        if (clips == null || clips.Count == 0)
            return null;

        List<AudioClip> validClips = clips.FindAll(clip => clip != null);

        if (validClips.Count == 0)
            return null;

        return validClips[UnityEngine.Random.Range(0, validClips.Count)];
    }
}