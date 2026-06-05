using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public GameData PlayerData;

    private string savePath;
    private const string encryptionKey = "PizzaSecretKey123"; // Khóa mã hóa dữ liệu

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        savePath = Path.Combine(Application.persistentDataPath, "pizza_player_data.dat");
        LoadGame();
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(PlayerData, true);
        string encryptedJson = EncryptDecrypt(json); // Mã hóa chuỗi JSON trước khi ghi file

        File.WriteAllText(savePath, encryptedJson);
        Debug.Log("[SaveManager] Đã lưu dữ liệu bảo mật thành công!");
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string encryptedJson = File.ReadAllText(savePath);
            string decryptedJson = EncryptDecrypt(encryptedJson); // Giải mã dữ liệu khi đọc

            PlayerData = JsonUtility.FromJson<GameData>(decryptedJson);
            Debug.Log("[SaveManager] Đã tải dữ liệu cũ lên thành công!");
        }
        else
        {
            PlayerData = new GameData(); // Tạo mới nếu chưa có file lưu
            SaveGame();
        }
    }

    // Thuật toán mã hóa/giải mã XOR Bitwise biến đổi chuỗi JSON thành các ký tự không đọc được
    private string EncryptDecrypt(string data)
    {
        string result = "";
        for (int i = 0; i < data.Length; i++)
        {
            result += (char)(data[i] ^ encryptionKey[i % encryptionKey.Length]);
        }
        return result;
    }
}