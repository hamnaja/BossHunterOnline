using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private const string SAVE_FILE = "save.dat";
    private const int BACKUP_SLOTS = 3;

    public PlayerData CurrentData { get; private set; } = new PlayerData();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Save()
    {
        InventoryManager.Instance?.SaveToPlayerData(CurrentData);
        string json = JsonConvert.SerializeObject(CurrentData);
        string encrypted = Encrypt(json);

        for (int i = BACKUP_SLOTS - 1; i > 0; i--)
        {
            string src = GetSavePath(i - 1);
            string dest = GetSavePath(i);
            if (File.Exists(src)) File.Copy(src, dest, true);
        }

        File.WriteAllText(GetSavePath(0), encrypted);
        Debug.Log("[SaveSystem] Saved successfully");
    }

    public bool Load()
    {
        for (int i = 0; i < BACKUP_SLOTS; i++)
        {
            string path = GetSavePath(i);
            if (!File.Exists(path)) continue;
            try
            {
                string encrypted = File.ReadAllText(path);
                string json = Decrypt(encrypted);
                CurrentData = JsonConvert.DeserializeObject<PlayerData>(json);
                InventoryManager.Instance?.LoadFromPlayerData(CurrentData);
                Debug.Log($"[SaveSystem] Loaded from slot {i}");
                return true;
            }
            catch { Debug.LogWarning($"[SaveSystem] Slot {i} corrupted"); }
        }
        CurrentData = new PlayerData();
        return false;
    }

    private string GetEncryptionKey()
    {
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        return deviceID.PadRight(32).Substring(0, 32);
    }

    private string Encrypt(string plainText)
    {
        try
        {
            byte[] key = Encoding.UTF8.GetBytes(GetEncryptionKey());
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();
            using var encryptor = aes.CreateEncryptor();
            byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            byte[] result = new byte[aes.IV.Length + encrypted.Length];
            aes.IV.CopyTo(result, 0);
            encrypted.CopyTo(result, aes.IV.Length);
            return System.Convert.ToBase64String(result);
        }
        catch { return System.Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText)); }
    }

    private string Decrypt(string cipherText)
    {
        try
        {
            byte[] key = Encoding.UTF8.GetBytes(GetEncryptionKey());
            byte[] fullBytes = System.Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            aes.Key = key;
            byte[] iv = new byte[aes.BlockSize / 8];
            byte[] encrypted = new byte[fullBytes.Length - iv.Length];
            System.Array.Copy(fullBytes, 0, iv, 0, iv.Length);
            System.Array.Copy(fullBytes, iv.Length, encrypted, 0, encrypted.Length);
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            byte[] decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch { return Encoding.UTF8.GetString(System.Convert.FromBase64String(cipherText)); }
    }

    private string GetSavePath(int slot)
    {
        string name = slot == 0 ? SAVE_FILE : $"save_backup_{slot}.dat";
        return Path.Combine(Application.persistentDataPath, name);
    }
}
