using System;
using System.Collections.Generic;

[Serializable]
public class JSONPlateData
{
    public int x;
    public int z;
    public List<string> toppings; // Chuỗi chữ như "Red", "Yellow"
}

[Serializable]
public class LevelData
{
    public int levelNumber;
    public int targetScore;
    public List<JSONPlateData> initialPlates;
}