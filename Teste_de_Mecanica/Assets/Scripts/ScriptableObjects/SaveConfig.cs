using UnityEngine;

[CreateAssetMenu(fileName = "SaveConfig", menuName = "Game/Save Config")]
public class SaveConfig : ScriptableObject
{
    [Header("Scenes")]
    public string menuScene = "Menu";
    public string firstLevelScene = "Level1";

    [Header("Save Slots")]
    public int autosaveSlot = 0;
    public string filePrefix = "save_slot_";
    public string fileExtension = ".sav";

    [Header("Encryption")]
    public string encryptionKey = "WaldDerElfen_TCC_PJD4M_2024_Save_Key";
    public string encryptionIv = "WaldDerElfenTCC";
}
