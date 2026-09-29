using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
   public const int AutoSaveSlot = 0;
   public static SaveManager Instance { get; private set; }

   [SerializeField] private SaveConfig saveConfig;

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
       return File.Exists(GetPath(slot));
   }

   public bool HasAutosave => SlotExists(AutoSaveSlot);

   public SaveData BuildDefaultSave()
   {
       string currentScene = SceneManager.GetActiveScene().name;
       return new SaveData
       {
           sceneName = string.IsNullOrEmpty(currentScene) ? DefaultSceneName : currentScene,
           levelIndex = 0,
           coins = 0,
           playerHealth = 100,
           playerPosition = Vector3.zero,
           checkpointPosition = Vector3.zero,
           checkpointReached = false
       };
   }

   public void SaveCurrentStateToSlot(int slot, SaveData data)
   {
       if (data == null)
       {
           data = BuildDefaultSave();
       }

       if (string.IsNullOrEmpty(data.sceneName))
       {
           data.sceneName = SceneManager.GetActiveScene().name;
       }

       SaveToSlot(slot, data);

       if (slot != AutoSaveSlot)
       {
           SaveToSlot(AutoSaveSlot, data.Clone());
       }

       SaveEventChannel.RaiseSaveCompleted(slot, data.Clone());
   }

   public void SaveToSlot(int slot, SaveData data)
   {
       if (data == null)
       {
           return;
       }

       string path = GetPath(slot);
       string directory = Path.GetDirectoryName(path);
       if (!string.IsNullOrEmpty(directory))
       {
           Directory.CreateDirectory(directory);
       }

       string json = JsonUtility.ToJson(data);
       string encrypted = Encrypt(json, slot);
       File.WriteAllText(path, encrypted);
   }

   public SaveData LoadFromSlot(int slot)
   {
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

           if (data != null && slot != AutoSaveSlot)
           {
               SaveToSlot(AutoSaveSlot, data.Clone());
           }

           SaveEventChannel.RaiseLoadCompleted(slot, data);
           return data;
       }
       catch (Exception ex)
       {
           Debug.LogWarning($"SaveManager: failed to load slot {slot}. {ex.Message}");
           return null;
       }
   }

   public void ClearSlot(int slot)
   {
       string path = GetPath(slot);
       if (File.Exists(path))
       {
           File.Delete(path);
       }
   }

   public string GetPath(int slot)
   {
       string directory = Application.persistentDataPath;
       string prefix = saveConfig != null && !string.IsNullOrEmpty(saveConfig.filePrefix) ? saveConfig.filePrefix : "save_slot_";
       string extension = saveConfig != null && !string.IsNullOrEmpty(saveConfig.fileExtension) ? saveConfig.fileExtension : ".sav";
       string fileName = $"{prefix}{slot}{extension}";
       return Path.Combine(directory, fileName);
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
       string seed = saveConfig != null && !string.IsNullOrEmpty(saveConfig.encryptionKey) ? saveConfig.encryptionKey : "WaldDerElfen_TCC_PJD4M_2024_Save_Key";
       string salt = $"{seed}:{slot}";
       using (SHA256 sha256 = SHA256.Create())
       {
           return sha256.ComputeHash(Encoding.UTF8.GetBytes(salt));
       }
   }

   private byte[] CreateIvBytes(int slot)
   {
       string seed = saveConfig != null && !string.IsNullOrEmpty(saveConfig.encryptionIv) ? saveConfig.encryptionIv : "WaldDerElfenTCC";
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
