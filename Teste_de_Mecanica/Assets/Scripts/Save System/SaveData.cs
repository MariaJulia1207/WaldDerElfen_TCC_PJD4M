using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public string sceneName = string.Empty;
    public int levelIndex;
    public int coins;
    public int playerHealth = 100;
    public bool checkpointReached;
    public Vector3 playerPosition;
    public Vector3 checkpointPosition;
    public List<string> interactedNPCs = new List<string>();
    public List<string> collectedImportantItems = new List<string>();
    public List<string> defeatedBosses = new List<string>();

    public SaveData Clone()
    {
        return new SaveData
        {
            sceneName = sceneName,
            levelIndex = levelIndex,
            coins = coins,
            playerHealth = playerHealth,
            checkpointReached = checkpointReached,
            playerPosition = playerPosition,
            checkpointPosition = checkpointPosition,
            interactedNPCs = new List<string>(interactedNPCs),
            collectedImportantItems = new List<string>(collectedImportantItems),
            defeatedBosses = new List<string>(defeatedBosses)
        };
    }
}