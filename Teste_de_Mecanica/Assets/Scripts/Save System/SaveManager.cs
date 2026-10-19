using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
   public const int AutoSaveSlot = 0;
   public const int ManualSaveSlot = 1;
   public const int FirstManualSlot = ManualSaveSlot;
   public const int LastManualSlot = 1;
   private const int SlotCount = 2;

   public static SaveManager Instance { get; private set; }

   [SerializeField] private SaveConfig saveConfig;

   private readonly List<string> currentCollectedCoins = new List<string>();
   public IReadOnlyCollection<string> CurrentCollectedCoins => currentCollectedCoins;

   private void Awake()
   {
       if (Instance != null && Instance != this)
       {
           Destroy(gameObject);
           return;
       }

       Instance = this;
       DontDestroyOnLoad(gameObject);
   }

   private void OnEnable()
   {
       SaveEventChannel.SaveRequested += HandleSaveRequested;
       SaveEventChannel.LoadRequested += HandleLoadRequested;
   }

   private void OnDisable()
   {
       SaveEventChannel.SaveRequested -= HandleSaveRequested;
       SaveEventChannel.LoadRequested -= HandleLoadRequested;
   }

   public SaveConfig SaveConfigAsset => saveConfig;

   public string DefaultSceneName => saveConfig != null && !string.IsNullOrEmpty(saveConfig.firstLevelScene)
       ? saveConfig.firstLevelScene
       : "Level1";

   public bool SlotExists(int slot)
   {
       return IsValidSlot(slot) && File.Exists(GetPath(slot));
   }

   public bool HasAutosave => SlotExists(AutoSaveSlot);
   public bool HasManualSave => SlotExists(ManualSaveSlot);

   public bool IsValidSlot(int slot)
   {
       return slot >= 0 && slot < SlotCount;
   }

   public SaveData BuildDefaultSave()
   {
       string currentScene = SceneManager.GetActiveScene().name;
       SaveData data = new SaveData
       {
           sceneName = string.IsNullOrEmpty(currentScene) ? DefaultSceneName : currentScene,
           checkpointSceneName = CheckpointManager.Instance != null ? CheckpointManager.Instance.LastCheckpointScene : string.Empty,
           levelIndex = 0,
           coins = 0,
           playerHealth = 100,
           playerPosition = Vector3.zero,
           checkpointPosition = CheckpointManager.Instance != null ? CheckpointManager.Instance.LastCheckpointPosition : Vector3.zero,
           checkpointReached = CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint,
           interactedNPCs = new List<string>(),
           collectedImportantItems = new List<string>(),
           defeatedBosses = new List<string>()
       };

       CaptureCurrentWorldState(data);
       return data;
   }

   public SaveData BuildCheckpointSave()
   {
       SaveData data = BuildDefaultSave();
       data.checkpointReached = CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint;
       data.checkpointSceneName = CheckpointManager.Instance != null ? CheckpointManager.Instance.LastCheckpointScene : string.Empty;
       data.checkpointPosition = CheckpointManager.Instance != null ? CheckpointManager.Instance.LastCheckpointPosition : Vector3.zero;
       data.sceneName = SceneManager.GetActiveScene().name;
       return data;
   }

   public void CaptureCurrentWorldState(SaveData data)
   {
       if (data == null)
       {
           return;
       }

       GameObject player = GameObject.FindGameObjectWithTag("Player");
       if (player != null)
       {
           data.playerPosition = player.transform.position;

           HealthSystem health = player.GetComponent<HealthSystem>();
           if (health != null)
           {
               data.playerHealth = health.vida;
           }
       }

       if (CheckpointManager.Instance != null && CheckpointManager.Instance.HasCheckpoint)
       {
           data.checkpointReached = true;
           data.checkpointSceneName = CheckpointManager.Instance.LastCheckpointScene;
           data.checkpointPosition = CheckpointManager.Instance.LastCheckpointPosition;
       }
       else
       {
           data.checkpointReached = false;
           data.checkpointSceneName = string.Empty;
           data.checkpointPosition = Vector3.zero;
       }

       if (data.interactedNPCs == null)
       {
           data.interactedNPCs = new List<string>();
       }
       else
       {
           data.interactedNPCs.Clear();
       }

       NPCInteractable[] npcs = FindObjectsByType<NPCInteractable>(FindObjectsSortMode.None);
       foreach (NPCInteractable npc in npcs)
       {
           if (npc != null && npc.IsDialogueCompleted)
           {
               data.interactedNPCs.Add(npc.SaveId);
           }
       }

       if (data.defeatedBosses == null)
       {
           data.defeatedBosses = new List<string>();
       }
       else
        {
           data.defeatedBosses.Clear();
       }

       BossArena[] bossArenas = FindObjectsByType<BossArena>(FindObjectsSortMode.None);
       foreach (BossArena bossArena in bossArenas)
       {
           if (bossArena != null && bossArena.IsDefeated)
           {
               data.defeatedBosses.Add(bossArena.SaveId);
           }
       }

       if (data.collectedImportantItems == null)
       {
           data.collectedImportantItems = new List<string>();
       }
       else
       {
           data.collectedImportantItems = NormalizeIds(data.collectedImportantItems);
       }
   }

   public void SaveCurrentStateToSlot(int slot, SaveData data)
   {
       if (!IsValidSlot(slot))
        {
           Debug.LogWarning("SaveManager: slot inválido: " + slot);
           return;
       }

       SaveData save = data ?? BuildDefaultSave();
       if (string.IsNullOrEmpty(save.sceneName))
       {
           save.sceneName = SceneManager.GetActiveScene().name;
       }

       SaveToSlot(slot, save);

       if (slot != AutoSaveSlot)
       {
           SaveToSlot(AutoSaveSlot, save.Clone());
       }

       SaveEventChannel.RaiseSaveCompleted(slot, save.Clone());
   }

   public void SaveToSlot(int slot, SaveData data)
   {
       if (!IsValidSlot(slot))
       {
           Debug.LogWarning("SaveManager: slot inválido: " + slot);
           return;
       }

       if (data == null)
       {
           return;
       }

       SaveData save = data.Clone();
       if (string.IsNullOrEmpty(save.sceneName))
       {
           save.sceneName = SceneManager.GetActiveScene().name;
       }

       if (save.interactedNPCs == null)
       {
           save.interactedNPCs = new List<string>();
       }

       if (save.collectedImportantItems == null)
       {
           save.collectedImportantItems = new List<string>();
       }

       if (save.defeatedBosses == null)
       {
           save.defeatedBosses = new List<string>();
       }

       save.interactedNPCs = NormalizeIds(save.interactedNPCs);
       save.collectedImportantItems = NormalizeIds(save.collectedImportantItems);
       save.defeatedBosses = NormalizeIds(save.defeatedBosses);

       string path = GetPath(slot);
       string directory = Path.GetDirectoryName(path);
       if (!string.IsNullOrEmpty(directory))
       {
           Directory.CreateDirectory(directory);
       }

       string json = JsonUtility.ToJson(save);
       string encrypted = Encrypt(json, slot);
       File.WriteAllText(path, encrypted);
   }

   public SaveData LoadFromSlot(int slot)
   {
       if (!IsValidSlot(slot))
       {
           return null;
       }

       string path = GetPath(slot);
       if (!File.Exists(path))
       {
           return null;
       }

       try
       {
           string encryptedJson = File.ReadAllText(path);
           string json = Decrypt(encryptedJson, slot);
           SaveData data = JsonUtility.FromJson<SaveData>(json);
           if (data == null)
           {
               return null;
           }

           if (data.interactedNPCs == null)
           {
               data.interactedNPCs = new List<string>();
           }

           if (data.collectedImportantItems == null)
           {
               data.collectedImportantItems = new List<string>();
           }

           if (data.defeatedBosses == null)
           {
               data.defeatedBosses = new List<string>();
           }

           SaveEventChannel.RaiseLoadCompleted(slot, data);
           return data;
       }
       catch (Exception ex)
       {
           Debug.LogWarning("SaveManager: falha ao carregar slot " + slot + ". " + ex.Message);
           return null;
       }
   }

   public void CopySlotToAutosave(int slot)
   {
       if (slot == AutoSaveSlot || !SlotExists(slot))
       {
           return;
       }

       File.Copy(GetPath(slot), GetPath(AutoSaveSlot), true);
   }

   public bool TryGetCheckpointSave(out SaveData checkpointSave)
   {
       checkpointSave = LoadFromSlot(AutoSaveSlot);
       if (checkpointSave == null)
       {
           return false;
       }

       if (!checkpointSave.checkpointReached || string.IsNullOrEmpty(checkpointSave.checkpointSceneName))
       {
           checkpointSave = null;
           return false;
       }

       return true;
   }

   public bool TryApplyCheckpointSave()
   {
       if (!TryGetCheckpointSave(out SaveData checkpointSave))
       {
           return false;
       }

       if (SaveRestoreManager.Instance != null)
       {
           SaveRestoreManager.Instance.ApplySaveData(checkpointSave);
           return true;
       }

       return false;
   }

   public void ClearSlot(int slot)
   {
       if (!IsValidSlot(slot))
       {
           return;
       }

       string path = GetPath(slot);
       if (File.Exists(path))
       {
           File.Delete(path);
       }
   }

   public string GetPath(int slot)
   {
       if (!IsValidSlot(slot))
       {
           slot = AutoSaveSlot;
       }

       string directory = Application.persistentDataPath;
       string prefix = saveConfig != null && !string.IsNullOrEmpty(saveConfig.filePrefix) ? saveConfig.filePrefix : "save_slot_";
       string extension = saveConfig != null && !string.IsNullOrEmpty(saveConfig.fileExtension) ? saveConfig.fileExtension : ".sav";
       string fileName = $"{prefix}{slot}{extension}";
       return Path.Combine(directory, fileName);
   }

   public void MarkCoinCollected(string coinId)
   {
       if (string.IsNullOrWhiteSpace(coinId))
       {
           return;
       }

       if (!currentCollectedCoins.Contains(coinId))
       {
           currentCollectedCoins.Add(coinId);
       }
   }

   public void SetCurrentCollectedCoins(IEnumerable<string> collectedIds)
   {
       currentCollectedCoins.Clear();
       if (collectedIds == null)
       {
           return;
       }

       foreach (string id in collectedIds)
       {
           if (string.IsNullOrWhiteSpace(id))
           {
               continue;
           }

           if (!currentCollectedCoins.Contains(id))
           {
               currentCollectedCoins.Add(id);
           }
       }
   }

   public void ClearCollectedCoins()
   {
       currentCollectedCoins.Clear();
   }

   public bool IsCoinCollected(string coinId)
   {
       if (string.IsNullOrWhiteSpace(coinId))
       {
           return false;
       }

       return currentCollectedCoins.Contains(coinId);
   }

   private static List<string> NormalizeIds(IEnumerable<string> ids)
   {
       List<string> normalized = new List<string>();
       if (ids == null)
       {
           return normalized;
       }

       foreach (string id in ids)
       {
           if (string.IsNullOrWhiteSpace(id))
           {
               continue;
           }

           if (!normalized.Contains(id))
           {
               normalized.Add(id);
           }
       }

       return normalized;
   }

   private void HandleSaveRequested(int slot)
   {
       SaveCurrentStateToSlot(slot, BuildDefaultSave());
   }

   private void HandleLoadRequested(int slot)
   {
       LoadFromSlot(slot);
   }

   private string Encrypt(string plainText, int slot)
   {
       if (string.IsNullOrEmpty(plainText))
       {
           return string.Empty;
       }

       byte[] keyBytes = CreateKeyBytes(slot);
       byte[] ivBytes = CreateIvBytes(slot);

       using (Aes aes = Aes.Create())
       {
           aes.Key = keyBytes;
           aes.IV = ivBytes;
           aes.Mode = CipherMode.CBC;
           aes.Padding = PaddingMode.PKCS7;

           using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
           using (var ms = new MemoryStream())
           {
               using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
               using (var sw = new StreamWriter(cs, Encoding.UTF8))
               {
                   sw.Write(plainText);
               }

               return Convert.ToBase64String(ms.ToArray());
           }
       }
   }

   private string Decrypt(string encryptedText, int slot)
   {
       if (string.IsNullOrEmpty(encryptedText))
       {
           return string.Empty;
       }

       byte[] keyBytes = CreateKeyBytes(slot);
       byte[] ivBytes = CreateIvBytes(slot);
       byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

       using (Aes aes = Aes.Create())
       {
           aes.Key = keyBytes;
           aes.IV = ivBytes;
           aes.Mode = CipherMode.CBC;
           aes.Padding = PaddingMode.PKCS7;

           using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
           using (var ms = new MemoryStream(encryptedBytes))
           using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
           using (var sr = new StreamReader(cs, Encoding.UTF8))
           {
               return sr.ReadToEnd();
           }
       }
   }

   private byte[] CreateKeyBytes(int slot)
   {
       string seed = saveConfig != null && !string.IsNullOrEmpty(saveConfig.encryptionKey)
           ? saveConfig.encryptionKey
           : "WaldDerElfen_TCC_PJD4M_2024_Save_Key";

       string salt = $"{seed}:{slot}";
       using (SHA256 sha256 = SHA256.Create())
       {
           return sha256.ComputeHash(Encoding.UTF8.GetBytes(salt));
       }
   }

   private byte[] CreateIvBytes(int slot)
   {
       string seed = saveConfig != null && !string.IsNullOrEmpty(saveConfig.encryptionIv)
           ? saveConfig.encryptionIv
           : "WaldDerElfenTCC";

       string salt = $"{seed}:{slot}";
       using (SHA256 sha256 = SHA256.Create())
       {
           byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(salt));
           byte[] iv = new byte[16];
           Array.Copy(hash, iv, 16);
           return iv;
       }
   }
}
