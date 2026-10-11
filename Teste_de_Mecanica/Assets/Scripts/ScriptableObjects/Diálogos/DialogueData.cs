using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif

[CreateAssetMenu(
    fileName = "New Dialogue",
    menuName = "Scriptable Objects/New Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Executar apenas uma vez")]
    public bool playOnlyOnce;

    [Header("Configuração da Voz")]
    public string voiceSoundName = "DialogueVoice";

    [Header("Diálogo Principal")]
    public DialogueLine[] mainDialogue;

    [Header("Resumo após terminar")]
    public DialogueLine[] summaryDialogue;

    [Header("Mover NPC após o diálogo principal")]
    public bool moverNpcAposDialogo;
    public Vector2 posicaoDestinoNpc;

    [Header("Item após o diálogo principal")]
    public bool criarItemAposDialogo;
    public GameObject prefabItemColetavel;
    public Vector2 deslocamentoItem = new Vector2(0f, 0.5f);

#if UNITY_EDITOR
    private static bool sincronizandoPitches;

    private void OnValidate()
    {
        if (sincronizandoPitches)
            return;

        sincronizandoPitches = true;

        try
        {
            SincronizarPitches();
        }
        finally
        {
            sincronizandoPitches = false;
        }
    }

    private void SincronizarPitches()
    {
        Dictionary<string, Vector2> pitches =
            new Dictionary<string, Vector2>(
                System.StringComparer.OrdinalIgnoreCase);

        // O asset que acabou de ser editado é a fonte
        // dos valores que serão compartilhados.
        AdicionarPitches(mainDialogue, pitches);
        AdicionarPitches(summaryDialogue, pitches);

        if (pitches.Count == 0)
            return;

        string[] guids =
            AssetDatabase.FindAssets("t:DialogueData");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            DialogueData asset =
                AssetDatabase.LoadAssetAtPath<DialogueData>(path);

            if (asset == null)
                continue;

            bool alterado = false;

            alterado |= AplicarPitches(asset.mainDialogue, pitches);
            alterado |= AplicarPitches(asset.summaryDialogue, pitches);

            if (alterado)
                EditorUtility.SetDirty(asset);
        }
    }

    private static void AdicionarPitches(
        DialogueLine[] lines,
        Dictionary<string, Vector2> pitches)
    {
        if (lines == null)
            return;

        foreach (DialogueLine line in lines)
        {
            if (line == null ||
                string.IsNullOrWhiteSpace(line.speakerName))
                continue;

            string key = line.speakerName.Trim();

            pitches[key] = new Vector2(
                Mathf.Min(line.voicePitchMin, line.voicePitchMax),
                Mathf.Max(line.voicePitchMin, line.voicePitchMax));
        }
    }

    private static bool AplicarPitches(
        DialogueLine[] lines,
        Dictionary<string, Vector2> pitches)
    {
        if (lines == null)
            return false;

        bool alterado = false;

        foreach (DialogueLine line in lines)
        {
            if (line == null ||
                string.IsNullOrWhiteSpace(line.speakerName))
                continue;

            string key = line.speakerName.Trim();

            if (!pitches.TryGetValue(key, out Vector2 pitch))
                continue;

            if (Mathf.Approximately(line.voicePitchMin, pitch.x) &&
                Mathf.Approximately(line.voicePitchMax, pitch.y))
                continue;

            line.voicePitchMin = pitch.x;
            line.voicePitchMax = pitch.y;

            alterado = true;
        }

        return alterado;
    }
#endif
}