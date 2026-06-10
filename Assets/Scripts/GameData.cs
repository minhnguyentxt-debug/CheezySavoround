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
}

[System.Serializable]
public class PlateData
{
    public int X;
    public int Z;
    public List<ToppingType> Slices = new List<ToppingType>();
}