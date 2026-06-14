using System.Collections.Generic;
using System.IO;
using UnityEngine;

// 1. Chỉ giữ lại DUY NHẤT một class PlateData nằm ở ngoài như thế này:
[System.Serializable]
public class PizzaPlateSaveData
{
    public int X, Z;
    public List<ToppingType> Slices;
}

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Tìm SaveManager có sẵn trong scene
                _instance = FindAnyObjectByType<SaveManager>();
                
                // Nếu vẫn không có, tự động tạo mới
                if (_instance == null)
                {
                    GameObject saveManagerObj = new GameObject("SaveManager");
                    _instance = saveManagerObj.AddComponent<SaveManager>();
                    Debug.Log("[SaveManager] Đã tự động tạo SaveManager mới");
                }
            }
            return _instance;
        }
    }
    
    public GameData PlayerData;

    private string savePath;
    private const string encryptionKey = "PizzaSecretKey123";

    private void Awake()
    {
        if (_instance == null) 
        {
            _instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        else 
        { 
            Destroy(gameObject); return; 
        }

        savePath = Path.Combine(Application.persistentDataPath, "pizza_player_data.dat");
        LoadGame();
    }

    private void OnApplicationQuit() { SaveGame(); }
    private void OnApplicationPause(bool pauseStatus) { if (pauseStatus) SaveGame(); }
    public void SaveGameNow() { SaveGame(); Debug.Log("[SaveManager] Dữ liệu đã được lưu chủ động."); }

    // ====================================================================
    // KHU VỰC CẦN KIỂM TRA: Hãy đảm bảo KHÔNG CÒN đoạn code:
    // public class PlateData { ... } nào nằm ở đây nữa!
    // ====================================================================

    /// <summary>
    /// Đảm bảo savePath được khởi tạo - an toàn cho cả Edit Mode và Play Mode
    /// </summary>
    private void EnsureSavePathInitialized()
    {
        if (string.IsNullOrEmpty(savePath))
        {
            savePath = Path.Combine(Application.persistentDataPath, "pizza_player_data.dat");
            Debug.Log($"[SaveManager] Khởi tạo savePath: {savePath}");
        }
    }

    public void SaveGame()
    {
        EnsureSavePathInitialized(); // Đảm bảo savePath không null
        
        string json = JsonUtility.ToJson(PlayerData, true);
        string encryptedJson = EncryptDecrypt(json);
        File.WriteAllText(savePath, encryptedJson);
        Debug.Log("[SaveManager] Đã lưu dữ liệu bảo mật thành công!");
    }
    
    public void LoadGame()
    {
        EnsureSavePathInitialized(); // Đảm bảo savePath không null
        
        if (File.Exists(savePath))
        {
            string encryptedJson = File.ReadAllText(savePath);
            string decryptedJson = EncryptDecrypt(encryptedJson);
            PlayerData = JsonUtility.FromJson<GameData>(decryptedJson);
            Debug.Log("[SaveManager] Đã tải dữ liệu cũ lên thành công!");
            
            // QUAN TRỌNG: Đồng bộ coins từ SaveManager sang ScoreManager
            SyncCoinsToScoreManager();
        }
        else
        {
            PlayerData = new GameData();
            SaveGame();
        }
    }
    
    /// <summary>
    /// Đồng bộ coins từ SaveManager sang ScoreManager để đảm bảo consistency
    /// </summary>
    private void SyncCoinsToScoreManager()
    {
        if (ScoreManager.Instance != null)
        {
            // Sử dụng reflection hoặc public method để set coins
            ScoreManager.Instance.SyncCoinsFromSave(PlayerData.coins);
            Debug.Log($"[SaveManager] Đã đồng bộ {PlayerData.coins} coins sang ScoreManager");
        }
    }

    /// <summary>
    /// XÓA TOÀN BỘ dữ liệu và tạo mới - dùng để reset game về trạng thái ban đầu
    /// </summary>
    [ContextMenu("Debug/Reset ALL Data (Coins + Items + Progress)")]
    public void ResetAllData()
    {
        PlayerData = new GameData();
        SaveGame();
        
        // Reset coins về 0 trong ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SyncCoinsFromSave(0);
        }
        
        Debug.Log("[SaveManager] ✓ Đã reset TOÀN BỘ dữ liệu về mặc định!");
    }
    
    /// <summary>
    /// Reset CHỈ items về maxUses, giữ nguyên coins và progress khác
    /// </summary>
    [ContextMenu("Debug/Reset Items Only (Keep Coins)")]
    public void ResetItemsOnly()
    {
        PlayerData.itemUsages.Clear();
        SaveGame();
        
        Debug.Log("[SaveManager] ✓ Đã reset items, giữ nguyên coins và progress!");
    }
    
    /// <summary>
    /// Xóa file save vật lý - game sẽ tạo file mới khi chạy lại
    /// </summary>
    [ContextMenu("Debug/Delete Save File")]
    public void DeleteSaveFile()
    {
        EnsureSavePathInitialized(); // Đảm bảo savePath không null
        
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"[SaveManager] ✓ Đã XÓA file save tại: {savePath}");
        }
        else
        {
            Debug.LogWarning($"[SaveManager] File save không tồn tại: {savePath}");
        }
        
        // Tạo data mới trong memory
        PlayerData = new GameData();
    }

    private string EncryptDecrypt(string data)
    {
        char[] chars = data.ToCharArray();
        int keyLength = encryptionKey.Length;
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = (char)(chars[i] ^ encryptionKey[i % keyLength]);
        }
        return new string(chars);
    }
}