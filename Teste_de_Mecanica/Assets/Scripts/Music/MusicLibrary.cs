using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MusicTrack
{
    public string name;
    public AudioClip audioClip;
}

public class MusicLibrary : MonoBehaviour
{
    [SerializeField] private MusicTrack[] musicTracks;

    private Dictionary<string, AudioClip> musicDictionary;

    private void Awake()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        musicDictionary = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);

        if (musicTracks == null)
        {
            Debug.LogError("MusicLibrary: A lista de faixas de música não foi configurada.", this);
            return;
        }

        foreach (MusicTrack track in musicTracks)
        {
            if (string.IsNullOrWhiteSpace(track.name))
            {
                Debug.LogWarning("MusicLibrary: Existe uma faixa sem nome.", this);
                continue;
            }

            if (track.audioClip == null)
            {
                Debug.LogWarning($"MusicLibrary: A faixa '{track.name}' não possui um AudioClip atribuído.", this);
                continue;
            }

            if (musicDictionary.ContainsKey(track.name))
            {
                Debug.LogWarning($"MusicLibrary: Nome duplicado de música '{track.name}'.", this);
                continue;
            }

            musicDictionary.Add(track.name, track.audioClip);
        }
    }

    public AudioClip GetClip(string musicName)
    {
        if (musicDictionary == null)
            InitializeDictionary();

        if (string.IsNullOrWhiteSpace(musicName))
            return null;

        if (musicDictionary.TryGetValue(musicName, out AudioClip clip))
        {
            return clip;
        }

        return null;
    }
}