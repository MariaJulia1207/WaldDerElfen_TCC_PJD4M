using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    // Posição do jogador no mundo
    public Vector3 playerPosition;
    public string LevelName;
}
/*
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public string sceneName;
    public int levelIndex;
    public int coins;
    public Vector3 playerPosition;
    public bool checkpointPassed;
    public Vector3 checkpointPosition;
    public List<string> collectedCoinIds = new List<string>();
}
*/