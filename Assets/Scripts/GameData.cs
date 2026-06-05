using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int highScorers = 0;
    public int gold = 0;
    public string currentSkinId = "Default_Plate";
    public List<string> unlockedSkinIds = new List<string>() { "Default_Plate" };
    public string lastClaimedTime = ""; // Phục vụ Daily Reward ngày 4-5
    public int currentStreak = 0;
}