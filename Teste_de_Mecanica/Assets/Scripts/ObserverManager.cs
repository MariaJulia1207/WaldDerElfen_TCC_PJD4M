using System.Collections.Generic;
using UnityEngine;

public class ObserverManager : MonoBehaviour
{
    public static ObserverManager Instance;

    private HashSet<string> dialoguesCompleted =
        new HashSet<string>();

    private NPCInteraction currentNPC;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetCurrentNPC(NPCInteraction npc)
    {
        currentNPC = npc;
    }

    public NPCInteraction GetCurrentNPC()
    {
        return currentNPC;
    }

    public void EndInteraction()
    {
        currentNPC = null;
    }

    public void RegisterDialogue(string npcID)
    {
        dialoguesCompleted.Add(npcID);
    }

    public bool HasTalked(string npcID)
    {
        return dialoguesCompleted.Contains(npcID);
    }
}