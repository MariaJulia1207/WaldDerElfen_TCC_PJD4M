using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string npcID;

    [TextArea(3, 5)]
    public string resumoDialogo;

    public List<DialogueLine> linhas;
}

[System.Serializable]
public class DialogueLine
{
    public string characterName;

    public Sprite portrait;

    [TextArea(3, 5)]
    public string text;
}