using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SoundEffectLibrary : MonoBehaviour
{
    [SerializeField] private SoundEffectGroup[] soundEffectGroup;
    private Dictionary<string, List<AudioClip>> soundDictionary;
    private void Awake()
    {
        InitializeDictionary();
    }
    public AudioClip GetRandomClip(string name)
    {
        if(soundDictionary.ContainKey(name))
        {
            List<AudioClip> audioClip = soundDictionary[name];
            if (audioClip.Count > 0)
            {
                return audioClips[UnityEngine.RandomRange(0, audioClips.Count)];
            }
        }
        return null;
    }
}
[System.Serializable]
public struct SoundEffectGroup 
{
    public string name;
    public List<AudioClip> audioClips; 
}
