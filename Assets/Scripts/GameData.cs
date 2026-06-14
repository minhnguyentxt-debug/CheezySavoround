using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int highScorers = 0;
    public int gold = 0;
    public string currentSkinId = "Default_Plate";
    public List<string> unlockedSkinIds = new List<string>() { "Default_Plate" };
    public string lastClaimedTime = "";
    public int currentStreak = 0;
    public List<PizzaPlateSaveData> Plates = new List<PizzaPlateSaveData>();
    public int currentLevel = 1; // Màn chơi hiện tại (1–30)
    
    // Lưu số coin (từ ScoreManager)
    public int coins = 0;
    
    // Lưu số lần sử dụng của mỗi item (key: itemName, value: currentUses)
    public List<ItemUsageData> itemUsages = new List<ItemUsageData>();
}

[System.Serializable]
public class ItemUsageData
{
    public string itemName;
    public int currentUses;
    
    public ItemUsageData(string name, int uses)
    {
        itemName = name;
        currentUses = uses;
    }
}

[System.Serializable]
public class PlateData
{
    public int X;
    public int Z;
    public List<ToppingType> Slices = new List<ToppingType>();
}