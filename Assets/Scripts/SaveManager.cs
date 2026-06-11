using System.Collections.Generic;
using System.IO;
using UnityEngine;

// 1. Chỉ giữ lại DUY NHẤT một class PlateData nằm ở ngoài như thế này:
[System.Serializable]
public class PizzaPlateSaveData
{
    public int X, Z;
    public List<ToppingType> Slices;
    public bool hasParentDoublePlate;
    public int parentDoublePlateX, parentDoublePlateZ;
    public float parentDoublePlateYRot;
    public bool isVertical;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public GameData PlayerData;

    private string savePath;
    private const string encryptionKey = "PizzaSecretKey123";

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this; 
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

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(PlayerData, true);
        string encryptedJson = EncryptDecrypt(json);
        File.WriteAllText(savePath, encryptedJson);
        Debug.Log("[SaveManager] Đã lưu dữ liệu bảo mật thành công!");
    }
    
    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string encryptedJson = File.ReadAllText(savePath);
            string decryptedJson = EncryptDecrypt(encryptedJson);
            PlayerData = JsonUtility.FromJson<GameData>(decryptedJson);
            Debug.Log("[SaveManager] Đã tải dữ liệu cũ lên thành công!");
        }
        else
        {
            PlayerData = new GameData();
            SaveGame();
        }
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